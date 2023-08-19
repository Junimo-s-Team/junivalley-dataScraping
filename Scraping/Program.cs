using Scraping.Common;
using Scraping.Managers;
using Scraping.Models;

VillagerManager villagerManager = new VillagerManager();
List<VillagerModel> villagersFailure = new List<VillagerModel>();
//Call VillagerManager
foreach (var villager in GeneralConstants.VILLAGERS)
{
    foreach (var lenguage in GeneralConstants.LENGUAGES)
    {
        try
        {
            string url = $"https://{lenguage.ToLower()}{GeneralConstants.BASE_URL}/{villager.Name}";
            VillagerModel villagerData = villagerManager.GetVillagerPrimaryData(url, villager);
            villagerManager.ConvertToJson(villagerData, lenguage);
        }

        catch (Exception ex)
        {
            villagersFailure.Add(villager);
            Console.WriteLine($"Algo ha ido mal con el aldeano {villager.Name}, en el idioma {lenguage}. Excepción: {ex}");
            Console.WriteLine(villagersFailure);
        }
    }
}