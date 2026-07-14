using PuppeteerSharp;
using System.Threading.Tasks;
using Scraping.Common;
using Scraping.Managers;
using Scraping.Models;

// 1. Inicializamos el navegador una sola vez.
var options = new LaunchOptions { Headless = true };
using var browserFetcher = new BrowserFetcher();
await browserFetcher.DownloadAsync();
await using var browser = await Puppeteer.LaunchAsync(options);

// 2. Pasamos la instancia del navegador al manager.
VillagerManager villagerManager = new VillagerManager(browser);
List<VillagerModel> villagersFailure = new List<VillagerModel>();

await ProcessVillagers(villagerManager);

async Task ProcessVillagers(VillagerManager manager)
{
    foreach (var villager in GeneralConstants.VILLAGERS)
    {
        foreach (var languagePrefix in GeneralConstants.LANGUAGES)
        {
            try
            {
                // Obtener el código de idioma ("ES", "PT", etc.) del prefijo.
                string languageCode = languagePrefix.Replace(".", "").ToUpper();
                if (string.IsNullOrEmpty(languageCode)) languageCode = "EN";

                string villagerNameForUrl = VillagerLanguageNames.GetTranslatedName(villager.Name, languageCode);

                string url = $"https://{languagePrefix.ToLower()}{GeneralConstants.BASE_URL}/{villagerNameForUrl}";

                VillagerModel villagerData = await manager.GetVillagerPrimaryData(url, villager, languageCode);
                manager.ConvertToJson(villagerData, villagerData.Language);
                Console.WriteLine($"[ÉXITO] Guardado: {villager.Name} en idioma {villagerData.Language}");
            }

            catch (Exception ex)
            {
                villagersFailure.Add(villager);
                Console.WriteLine($"Algo ha ido mal con el aldeano {villager.Name}, en el idioma {languagePrefix}. Excepción: {ex.Message}");
                Console.WriteLine(villagersFailure);
            }
        }
    }
}