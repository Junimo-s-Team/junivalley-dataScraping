using Scraping.Common;
using Scraping.Managers;
using Scraping.Models;

VillagerManager villagerManager = new VillagerManager();
List<VillagerModel> villagersFailure = new List<VillagerModel>();
//VillagerLanguageNames villagerNames = new VillagerLanguageNames();
int villagerId = 0;

//Call VillagerManager
foreach (var villager in GeneralConstants.VILLAGERS)
{
    try
    {
        villagerId++;
        VillagerModel villagerData = villagerManager.GetVillagerPrimaryData($"{GeneralConstants.BASE_URL}/{villager.Name}", villager, villagerId);
        villagerManager.ConvertToJson(villagerData);
    }
    catch (Exception ex)
    {
        villagersFailure.Add(villager);
        Console.WriteLine($"Algo ha ido mal con el aldeano {villager.Name}. Excepción: {ex}");
        Console.WriteLine(villagersFailure);
    }
}