using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PuppeteerSharp;
using Scraping.Common;
using Scraping.Models;
using System.Net;
using System.Text.RegularExpressions;

namespace Scraping.Managers
{
    public class FarmhouseManager : BaseManager
    {
        public FarmhouseManager(IBrowser browser) : base(browser) { }

        public async Task ScrapeAndSaveFarmhouse(string languageCode)
        {
            string url = languageCode.ToUpper() == "ES" ? "https://es.stardewvalleywiki.com/Casa_de_campo" : "https://stardewvalleywiki.com/Farmhouse";
            HtmlDocument doc = await GetDocument(url);

            var keys = WikiKeys.GetKeys(languageCode);

            var farmhouse = new FarmhouseModel
            {
                Name = WebUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//h1[@id='firstHeading']")?.InnerText.Trim() ?? string.Empty),
                Description = ScrapeDescription(doc),
                Image = ToAbsoluteUrl(doc.DocumentNode.SelectSingleNode("//div[@id='infoboxborder']//a/img")?.GetAttributeValue("src", string.Empty)),
                MapImage = ToAbsoluteUrl(doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'mapcontainer')]/img[contains(@alt, 'Map.png')]")?.GetAttributeValue("src", string.Empty)),
                InitialInteriors = ScrapeGallery(doc, "Interior"),
                Upgrades = ScrapeUpgradesTable(doc, keys["FarmhouseUpgrades"]),
                Renovations = ScrapeRenovationsTable(doc, keys["FarmhouseRenovations"]),
                SpouseRooms = ScrapeGallery(doc, keys["SpouseRooms"])
            };

            SaveDataToJson(farmhouse, "Farmhouse", languageCode);
        }

        private string ScrapeDescription(HtmlDocument doc)
        {
            var descriptionNodes = doc.DocumentNode.SelectNodes("//div[@id='infoboxborder']/following-sibling::p");
            if (descriptionNodes == null) return string.Empty;

            return string.Join("\n", descriptionNodes.Select(p => WebUtility.HtmlDecode(p.InnerText).Trim()));
        }

        private List<FarmhouseDetailModel> ScrapeGallery(HtmlDocument doc, string headerId)
        {
            var items = new List<FarmhouseDetailModel>();
            var headerNode = doc.DocumentNode.SelectSingleNode($"//span[@id='{headerId}']/ancestor::h2");
            if (headerNode == null) return items;

            var galleryNode = headerNode.SelectSingleNode("following-sibling::ul[contains(@class, 'gallery')]");
            if (galleryNode == null) return items;

            var galleryItems = galleryNode.SelectNodes(".//li[@class='gallerybox']");
            if (galleryItems == null) return items;

            foreach (var item in galleryItems)
            {
                items.Add(new FarmhouseDetailModel
                {
                    Name = WebUtility.HtmlDecode(item.SelectSingleNode(".//div[@class='gallerytext']//a")?.InnerText.Trim() ?? item.SelectSingleNode(".//div[@class='gallerytext']")?.InnerText.Trim() ?? string.Empty),
                    Image = ToAbsoluteUrl(item.SelectSingleNode(".//a[@class='image']/img")?.GetAttributeValue("src", string.Empty))
                });
            }
            return items;
        }

        private List<FarmhouseUpgradeModel> ScrapeUpgradesTable(HtmlDocument doc, string headerId)
        {
            var upgrades = new List<FarmhouseUpgradeModel>();
            var table = doc.DocumentNode.SelectSingleNode($"//span[@id='{headerId}']/ancestor::h2/following-sibling::table[contains(@class, 'wikitable')]");
            if (table == null) return upgrades;

            var rows = table.SelectNodes(".//tbody/tr[td]");
            if (rows == null) return upgrades;

            foreach (var row in rows)
            {
                var cells = row.SelectNodes("td");
                if (cells == null || cells.Count < 5) continue;

                var upgrade = new FarmhouseUpgradeModel
                {
                    Name = WebUtility.HtmlDecode(cells[0].InnerText).Trim(),
                    ExteriorImage = ToAbsoluteUrl(cells[1].SelectSingleNode(".//img")?.GetAttributeValue("src", string.Empty)),
                    InteriorImage = ToAbsoluteUrl(cells[2].SelectSingleNode(".//img")?.GetAttributeValue("src", string.Empty)),
                    Price = WebUtility.HtmlDecode(cells[3].SelectSingleNode(".//span[contains(@class, 'no-wrap')]")?.InnerText.Trim() ?? string.Empty),
                    Description = WebUtility.HtmlDecode(cells[4].InnerText).Trim(),
                    Materials = ParseMaterials(cells[3])
                };
                upgrades.Add(upgrade);
            }
            return upgrades;
        }

        private List<FarmhouseUpgradeModel> ScrapeRenovationsTable(HtmlDocument doc, string headerId)
        {
            var renovations = new List<FarmhouseUpgradeModel>();
            var table = doc.DocumentNode.SelectSingleNode($"//span[@id='{headerId}']/ancestor::h2/following-sibling::table[contains(@class, 'wikitable')]");
            if (table == null) return renovations;

            var rows = table.SelectNodes(".//tbody/tr[td]");
            if (rows == null) return renovations;

            foreach (var row in rows)
            {
                var cells = row.SelectNodes("td");
                if (cells == null || cells.Count < 4) continue;

                var renovation = new FarmhouseUpgradeModel
                {
                    Name = WebUtility.HtmlDecode(cells[0].InnerText).Trim(),
                    InteriorImage = ToAbsoluteUrl(cells[1].SelectSingleNode(".//img")?.GetAttributeValue("src", string.Empty)),
                    Price = WebUtility.HtmlDecode(cells[2].SelectSingleNode(".//span[contains(@class, 'no-wrap')]")?.InnerText.Trim() ?? cells[2].InnerText.Trim()),
                    Description = WebUtility.HtmlDecode(cells[3].InnerText).Trim()
                };
                renovations.Add(renovation);
            }
            return renovations;
        }

        private List<UpgradeMaterialModel> ParseMaterials(HtmlNode cell)
        {
            var materials = new List<UpgradeMaterialModel>();
            var materialNodes = cell.SelectNodes(".//span[@class='nametemplate']");
            if (materialNodes == null) return materials;

            foreach (var node in materialNodes)
            {
                var quantityMatch = Regex.Match(node.InnerText, @"\((\d+)\)");
                materials.Add(new UpgradeMaterialModel
                {
                    Name = WebUtility.HtmlDecode(node.SelectSingleNode("a")?.InnerText.Trim() ?? string.Empty),
                    Image = ToAbsoluteUrl(node.SelectSingleNode(".//img")?.GetAttributeValue("src", string.Empty)),
                    Quantity = quantityMatch.Success ? int.Parse(quantityMatch.Groups[1].Value) : 0
                });
            }
            return materials;
        }

        private void SaveDataToJson<T>(T data, string fileName, string languageCode)
        {
            string fullFileName = $"{fileName}.json";
            string path = @$"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Stardew Valley/Scrapping/{languageCode}/general";
            string fullPath = Path.Combine(path, fullFileName);

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };
            string json = JsonConvert.SerializeObject(data, serializerSettings);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            File.WriteAllText(fullPath, json);
        }
    }
}