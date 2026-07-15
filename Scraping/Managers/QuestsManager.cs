using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PuppeteerSharp;
using Scraping.Common;
using Scraping.Models;
using System.Net;

namespace Scraping.Managers
{
    public class QuestsManager : BaseManager
    {
        public QuestsManager(IBrowser browser) : base(browser) { }

        public async Task ScrapeAndSaveQuests(string languageCode)
        {
            string url = languageCode.ToUpper() == "ES" ? "https://es.stardewvalleywiki.com/Misiones" : "https://stardewvalleywiki.com/Quests";
            HtmlDocument doc = await GetDocument(url);

            // Extraer Misiones de Historia
            var storyQuests = ScrapeStoryQuests(doc, languageCode);
            SaveDataToJson(storyQuests, "StoryQuests", languageCode);

            // Extraer Pedidos Especiales
            var specialOrders = ScrapeSpecialOrders(doc, languageCode);
            SaveDataToJson(specialOrders, "SpecialOrders", languageCode);

            // Extraer Pedidos Especiales del Sr. Qi
            var qiSpecialOrders = ScrapeQiSpecialOrders(doc, languageCode);
            SaveDataToJson(qiSpecialOrders, "QiSpecialOrders", languageCode);

            // Extraer Objetos de Misión
            var questItems = ScrapeQuestItems(doc, languageCode);
            SaveDataToJson(questItems, "QuestItems", languageCode);
        }

        private List<StoryQuestModel> ScrapeStoryQuests(HtmlDocument doc, string languageCode)
        {
            var quests = new List<StoryQuestModel>();
            string headerId = languageCode.ToUpper() == "ES" ? "Lista_de_misiones_de_historia" : "List_of_Story_Quests";
            var table = doc.DocumentNode.SelectSingleNode($"//span[@id='{headerId}']/ancestor::h2/following-sibling::table[contains(@class, 'wikitable')][1]");

            if (table == null) return quests;

            var rows = table.SelectNodes(".//tbody/tr");
            if (rows == null) return quests;

            foreach (var row in rows.Skip(1)) // Saltar la fila de encabezado
            {
                var cells = row.SelectNodes("td");
                if (cells == null || cells.Count < 5) continue;

                // Manejar rowspan para "El misterioso Qi"
                if (cells.Count == 4) // Fila con rowspan
                {
                    var previousQuest = quests.LastOrDefault();
                    if (previousQuest != null && previousQuest.Name == "El misterioso Qi")
                    {
                        // Esta fila comparte el nombre de la misión anterior
                        quests.Add(new StoryQuestModel
                        {
                            Name = previousQuest.Name,
                            QuestText = WebUtility.HtmlDecode(cells[0].InnerText).Trim(),
                            GivenBy = WebUtility.HtmlDecode(cells[1].InnerText).Trim(),
                            Requirements = WebUtility.HtmlDecode(cells[2].InnerText).Trim(),
                            Reward = WebUtility.HtmlDecode(cells[3].InnerText).Trim(),
                        });
                    }
                }
                else
                {
                    quests.Add(new StoryQuestModel
                    {
                        Name = WebUtility.HtmlDecode(cells[0].InnerText).Trim(),
                        QuestText = WebUtility.HtmlDecode(cells[1].InnerText).Trim(),
                        GivenBy = WebUtility.HtmlDecode(cells[2].InnerText).Trim(),
                        Requirements = WebUtility.HtmlDecode(cells[3].InnerText).Trim(),
                        Reward = WebUtility.HtmlDecode(cells[4].InnerText).Trim(),
                    });
                }
            }
            return quests;
        }

        private List<SpecialOrderModel> ScrapeSpecialOrders(HtmlDocument doc, string languageCode)
        {
            string headerId = languageCode.ToUpper() == "ES" ? "Lista_de_pedidos_especiales" : "List_of_Special_Orders";
            return ScrapeSpecialOrdersTable(doc, headerId);
        }

        private List<SpecialOrderModel> ScrapeQiSpecialOrders(HtmlDocument doc, string languageCode)
        {
            string headerId = languageCode.ToUpper() == "ES" ? "Lista_de_pedidos_especiales_del_Sr._Qi" : "List_of_Mr._Qi.27s_Special_Orders";
            return ScrapeSpecialOrdersTable(doc, headerId, isQiQuest: true);
        }

        private List<SpecialOrderModel> ScrapeSpecialOrdersTable(HtmlDocument doc, string headerId, bool isQiQuest = false)
        {
            var orders = new List<SpecialOrderModel>();
            var table = doc.DocumentNode.SelectSingleNode($"//span[@id='{headerId}']/ancestor::h2/following-sibling::table[contains(@class, 'wikitable')][1]");

            if (table == null) return orders;

            var rows = table.SelectNodes(".//tbody/tr");
            if (rows == null) return orders;

            foreach (var row in rows.Skip(1)) // Saltar la fila de encabezado
            {
                var cells = row.SelectNodes("td");
                if (cells == null) continue;

                // La tabla de Qi tiene menos columnas
                if (isQiQuest && cells.Count >= 5)
                {
                    orders.Add(new SpecialOrderModel
                    {
                        Name = WebUtility.HtmlDecode(cells[0].InnerText).Trim(),
                        QuestText = WebUtility.HtmlDecode(cells[1].InnerText).Trim(),
                        TimeLimit = WebUtility.HtmlDecode(cells[2].InnerText).Trim(),
                        Requirements = WebUtility.HtmlDecode(cells[3].InnerText).Trim(),
                        Reward = WebUtility.HtmlDecode(cells[4].InnerText).Trim(),
                    });
                }
                else if (!isQiQuest && cells.Count >= 8)
                {
                    orders.Add(new SpecialOrderModel
                    {
                        Name = WebUtility.HtmlDecode(cells[0].InnerText).Trim(),
                        QuestText = WebUtility.HtmlDecode(cells[1].InnerText).Trim(),
                        GivenBy = WebUtility.HtmlDecode(cells[2].InnerText).Trim(),
                        Prerequisites = WebUtility.HtmlDecode(cells[3].InnerText).Trim(),
                        TimeLimit = WebUtility.HtmlDecode(cells[4].InnerText).Trim(),
                        Requirements = WebUtility.HtmlDecode(cells[5].InnerText).Trim(),
                        Reward = WebUtility.HtmlDecode(cells[6].InnerText).Trim(),
                        IsRepeatable = WebUtility.HtmlDecode(cells[7].InnerText).Trim(),
                    });
                }
            }
            return orders;
        }

        private List<QuestItemModel> ScrapeQuestItems(HtmlDocument doc, string languageCode)
        {
            var items = new List<QuestItemModel>();
            var keys = WikiKeys.GetKeys(languageCode);
            string headerId = keys["QuestItems"];
            var table = doc.DocumentNode.SelectSingleNode($"//span[@id='{headerId}']/ancestor::h2/following-sibling::table[contains(@class, 'wikitable')][1]");

            if (table == null) return items;

            var rows = table.SelectNodes(".//tbody/tr");
            if (rows == null) return items;

            foreach (var row in rows.Skip(1)) // Saltar la fila de encabezado
            {
                var cells = row.SelectNodes("td");
                if (cells == null || cells.Count < 4) continue;

                items.Add(new QuestItemModel
                {
                    Image = ToAbsoluteUrl(cells[0].SelectSingleNode(".//img")?.GetAttributeValue("src", string.Empty)),
                    Name = WebUtility.HtmlDecode(cells[1].InnerText).Trim(),
                    Description = WebUtility.HtmlDecode(cells[2].InnerText).Trim(),
                    RelatedQuest = WebUtility.HtmlDecode(cells[3].InnerText).Trim()
                });
            }

            return items;
        }

        private void SaveDataToJson<T>(List<T> data, string fileName, string languageCode)
        {
            string fullFileName = $"{fileName}.json";
            string path = @$"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Stardew Valley/Scrapping/{languageCode}/quests";
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