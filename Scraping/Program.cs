using Scraping.Common;
using Scraping.Managers;
using Scraping.Models;

VillagerManager villagerManager = new VillagerManager();
VillagerLanguageNames villagerNames = new VillagerLanguageNames();

//Call VillagerManager
foreach (var villager in GeneralConstants.VILLAGERS)
{
    try
    {
        villagerId++;
        VillagerModel villagerData = villagerManager.GetVillagerPrimaryData($"{GeneralConstants.URL}/{villager.Name}", villager, villagerId);
        villagerManager.ConvertToJson(villagerData);
    }
    catch (Exception ex)
    {
        villagersFailure.Add(villager);
        Console.WriteLine($"Algo ha ido mal con el aldeano {villager.Name}. Excepción: {ex}");
        Console.WriteLine(villagersFailure);
    }
}