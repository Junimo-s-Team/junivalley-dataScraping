using HtmlAgilityPack;
using PuppeteerSharp;
using Scraping.Common;
using Scraping.Models;
using System.Net;

namespace Scraping.Managers
{
    public class AnimalsManager : BaseManager
    {
        public AnimalsManager(IBrowser browser) : base(browser) { }

        public async Task ScrapeAndSaveAnimals(string languageCode)
        {
            string url = languageCode.ToUpper() == "ES" ? "https://es.stardewvalleywiki.com/Animales" : "https://stardewvalleywiki.com/Animals";
            HtmlDocument doc = await GetDocument(url);
            var keys = WikiKeys.GetKeys(languageCode);

            var animalPage = new AnimalPageModel
            {
                Title = WebUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//h1[@id='firstHeading']")?.InnerText.Trim() ?? string.Empty),
                Description = ScrapeInitialDescription(doc),
                Pets = ScrapePets(doc, keys, languageCode),
                Horse = ScrapeHorse(doc, keys["Horse"]),
                CoopAnimals = ScrapeFarmAnimals(doc, keys["CoopAnimals"], languageCode),
                BarnAnimals = ScrapeFarmAnimals(doc, keys["BarnAnimals"], languageCode),
                OtherAnimals = ScrapeOtherAnimals(doc, keys["OtherAnimals"], languageCode)
            };

            SaveDataToJson(animalPage, "Animals", languageCode, "animals");
        }

        private string ScrapeInitialDescription(HtmlDocument doc)
        {
            var nodes = doc.DocumentNode.SelectNodes("//div[@id='mw-content-text']/div/p[position() >= 1 and position() <= 3]");
            return nodes != null ? string.Join("\n\n", nodes.Select(n => WebUtility.HtmlDecode(n.InnerText).Trim())) : string.Empty;
        }

        private List<PetModel> ScrapePets(HtmlDocument doc, Dictionary<string, string> keys, string languageCode)
        {
            var pets = new List<PetModel>();
            var catDogPet = new PetModel { Type = "Cat/Dog" };
            var turtlePet = new PetModel { Type = "Turtle" };

            var petsHeader = doc.DocumentNode.SelectSingleNode($"//span[@id='{keys["Pets"]}']/ancestor::h2");
            if (petsHeader == null) return pets;

            // --- Scrape Images ---
            var currentNode = petsHeader.NextSibling;
            while (currentNode != null && currentNode.Name != "h2")
            {
                if (currentNode.Name == "ul" && currentNode.HasClass("gallery"))
                {
                    var images = currentNode.SelectNodes(".//li//img");
                    if (images == null) continue;

                    foreach (var img in images)
                    {
                        var src = img.GetAttributeValue("src", "");
                        if (string.IsNullOrEmpty(src)) continue;

                        if (src.Contains("Cat") || src.Contains("Dog")) // Includes Cat_1.png, Dog.gif, etc.
                        {
                            catDogPet.Images.Add(ToAbsoluteUrl(src));
                        }
                        else if (src.Contains("Turtle"))
                        {
                            turtlePet.Images.Add(ToAbsoluteUrl(src));
                        }
                    }
                }
                currentNode = currentNode.NextSibling;
            }

            // --- Scrape Pet Gifts ---
            var giftsHeader = doc.DocumentNode.SelectSingleNode($"//span[@id='Pet_Gifts' or @id='Regalos_de_mascotas']");
            if (giftsHeader != null)
            {
                var giftTables = giftsHeader.SelectNodes("ancestor::h4/following-sibling::table[contains(@class, 'mw-collapsible')]//table[contains(@class, 'wikitable')]");
                if (giftTables != null)
                {
                    foreach (var table in giftTables)
                    {
                        var petTypeHeader = table.SelectSingleNode(".//thead/tr[1]/th[1]")?.InnerText.Trim();
                        if (string.IsNullOrEmpty(petTypeHeader)) continue;

                        PetModel? targetPet = null;
                        if (petTypeHeader == keys["CatGifts"] || petTypeHeader == keys["DogGifts"])
                        {
                            targetPet = catDogPet;
                        }
                        else if (petTypeHeader == keys["TurtleGifts"])
                        {
                            targetPet = turtlePet;
                        }

                        if (targetPet == null) continue;

                        var rows = table.SelectNodes(".//tbody/tr[td]");
                        if (rows == null) continue;

                        foreach (var row in rows)
                        {
                            var cells = row.SelectNodes("./td");
                            if (cells == null || cells.Count < 2) continue;
                            targetPet.Gifts.Add(new PetGiftModel
                            {
                                ItemName = CleanText(cells[0], languageCode),
                                Chance = CleanText(cells[1], languageCode)
                            });
                        }
                    }
                }
            }

            if (catDogPet.Images.Any() || catDogPet.Gifts.Any()) pets.Add(catDogPet);
            if (turtlePet.Images.Any() || turtlePet.Gifts.Any()) pets.Add(turtlePet);

            return pets;
        }

        private HorseModel? ScrapeHorse(HtmlDocument doc, string headerId)
        {
            var horseHeader = doc.DocumentNode.SelectSingleNode($"//span[@id='{headerId}']/ancestor::h2");
            if (horseHeader == null) return null;

            var image = horseHeader.SelectSingleNode("following-sibling::p/a/img")?.GetAttributeValue("src", "");
            var descriptionNodes = horseHeader.SelectNodes("following-sibling::p");
            var description = descriptionNodes != null ? string.Join("\n\n", descriptionNodes.Take(4).Select(p => WebUtility.HtmlDecode(p.InnerText).Trim())) : "";

            return new HorseModel
            {
                Image = ToAbsoluteUrl(image),
                Description = description
            };
        }

        private List<FarmAnimalModel> ScrapeFarmAnimals(HtmlDocument doc, string headerId, string languageCode)
        {
            var animals = new List<FarmAnimalModel>();
            var mainHeader = doc.DocumentNode.SelectSingleNode($"//span[@id='{headerId}']/ancestor::h2");
            if (mainHeader == null) return animals;

            var currentNode = mainHeader.NextSibling;
            while (currentNode != null && currentNode.Name != "h2") // Stop at the next major section
            {
                // Find the table that follows the current h3 header
                if (currentNode.Name == "h3")
                {
                    var table = currentNode.SelectSingleNode("following-sibling::table[contains(@class, 'wikitable')]");
                    if (table == null) continue;

                    var rows = table.SelectNodes(".//tbody/tr[td]");
                    if (rows == null) continue;

                    foreach (var row in rows)
                    {
                        var cells = row.SelectNodes("td");
                        if (cells == null || cells.Count < 5) continue;

                        animals.Add(new FarmAnimalModel
                        {
                            Image = ToAbsoluteUrl(cells[0].SelectSingleNode(".//img")?.GetAttributeValue("src", "")),
                            Name = CleanText(cells[1], languageCode),
                            Cost = CleanText(cells[2], languageCode),
                            Produce = CleanText(cells[cells.Count - 2], languageCode),
                            SellPrice = CleanText(cells.Last(), languageCode),
                            Building = cells.Count > 5 ? CleanText(cells[3], languageCode) : ""
                        });
                    }
                }
                currentNode = currentNode.NextSibling;
            }
            return animals;
        }

        private List<OtherAnimalModel> ScrapeOtherAnimals(HtmlDocument doc, string headerId, string languageCode)
        {
            // This section is less structured, so we'll do a simpler scrape.
            // For now, we'll leave this as a placeholder for future implementation if needed.
            return new List<OtherAnimalModel>();
        }
    }
}