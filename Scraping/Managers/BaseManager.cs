﻿using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PuppeteerSharp;
using Scraping.Models;

namespace Scraping.Managers
{
    public class BaseManager
    {
        public BaseManager() 
        { 
        }

        //Load document data about url
        public async Task<HtmlDocument> GetDocument(string url)
        {
            var options = new LaunchOptions { Headless = true };
            
            // Descarga Chromium si no existe
            using var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            await using var browser = await Puppeteer.LaunchAsync(options);
            await using var page = await browser.NewPageAsync();
            
            // Hacemos que parezca un navegador normal
            await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36");

            await page.GoToAsync(url, new NavigationOptions { Timeout = 60000 }); // Aumentamos el timeout a 60 segundos
            string content = await page.GetContentAsync();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);
            return await Task.FromResult(doc);
        }

        //Save villager data in json
        public void ConvertToJson(VillagerModel villagerData, string language)
        {
            string fileName = $"{villagerData.Name}.json";
            string path = @$"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Stardew Valley/Scrapping/{language}";
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