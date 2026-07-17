﻿﻿﻿﻿﻿using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PuppeteerSharp;
using Scraping.Models;
using Scraping.Common;

namespace Scraping.Managers
{
    public class BaseManager
    {
        private readonly IBrowser _browser;

        public BaseManager(IBrowser browser) 
        {
            _browser = browser;
        }

        //Load document data about url
        public async Task<HtmlDocument> GetDocument(string url, string? waitForSelector = null)
        {
            // Reutilizamos la instancia del navegador en lugar de crear una nueva.
            await using var page = await _browser.NewPageAsync();
            
            // Hacemos que parezca un navegador normal
            await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36");

            await page.GoToAsync(url, new NavigationOptions { Timeout = 60000 }); // Aumentamos el timeout a 60 segundos

            // Si se proporciona un selector, esperamos a que aparezca.
            // Si no aparece en el tiempo límite, continuamos sin lanzar un error.
            if (!string.IsNullOrEmpty(waitForSelector))
            {
                try
                {
                    await page.WaitForSelectorAsync(waitForSelector, new WaitForSelectorOptions { Timeout = 5000 });
                }
                catch (WaitTaskTimeoutException) { /* Ignoramos el timeout y continuamos */ }
            }

            string content = await page.GetContentAsync();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);
            return await Task.FromResult(doc);
        }

        //Save generic data in json
        protected void SaveDataToJson<T>(T data, string fileName, string languageCode, string subfolder)
        {
            string fullFileName = $"{fileName}.json";
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Stardew Valley", "Scrapping", languageCode, subfolder);
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
        //Save villager data in json
        public void ConvertToJson(VillagerModel villagerData, string language)
        {
            string fileName = $"{villagerData.Name}.json";
            string path = @$"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Stardew Valley/Scrapping/{language}/villagers";
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

        // Función de ayuda para convertir URLs relativas a absolutas
        protected string ToAbsoluteUrl(string? url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }
            if (url.StartsWith("/mediawiki/"))
            {
                return $"https://{GeneralConstants.BASE_URL}{url}";
            }
            return url;
        }
    }
}