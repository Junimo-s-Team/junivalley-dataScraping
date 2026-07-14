using System.Threading.Tasks;
using Scraping.Common;
using Scraping.Managers;
using Scraping.Models;

VillagerManager villagerManager = new VillagerManager();
List<VillagerModel> villagersFailure = new List<VillagerModel>();
//Call VillagerManager

await ProcessVillagers();

async Task ProcessVillagers()
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

                VillagerModel villagerData = await villagerManager.GetVillagerPrimaryData(url, villager, languageCode);
                villagerManager.ConvertToJson(villagerData, villagerData.Language);
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