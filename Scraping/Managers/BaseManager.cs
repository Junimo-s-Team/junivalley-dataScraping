using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Scraping.Models;

namespace Scraping.Managers
{
    public class BaseManager
    {
        public BaseManager() 
        { 
        }

        //Load document data about url
        public HtmlDocument GetDocument(string url)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            return doc;
        }

        //Save villager data in json
        public void ConvertToJson(VillagerModel villagerData)
        {
            string fileName = $"{villagerData.Name}.json";
            string path = @$"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Stardew Valley/Scrapping/EN";
            string fullFileName = Path.Combine(path, fileName);
            //lowercase
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            string json = JsonConvert.SerializeObject(villagerData, serializerSettings);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            File.WriteAllText(fullFileName, json);
        }
    }
}