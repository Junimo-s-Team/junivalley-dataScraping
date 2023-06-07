using HtmlAgilityPack;
using Scraping.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using Scraping;

string baseUrl = "https://stardewvalleywiki.com";
List<string> languages = new List<string> { "EN", "ES", "FR", "PT", "DE", "JA" };

VillagerManager villagerManager = new VillagerManager();
VillagerLanguageNames villagerNames = new VillagerLanguageNames();
Dictionary<string, Dictionary<string, (string Name, bool HasFamily, bool HasClinicVisit)>> villagers = villagerNames.Villagers;

List<KeyValuePair<string, Dictionary<string, (string name, bool hasFamily, bool hasClinicVisit)>>> villagersFailure = new List<KeyValuePair<string, Dictionary<string, (string name, bool hasFamily, bool hasClinicVisit)>>>();
int villagerId = 0;

//Call VillagerManager
foreach (var villager in villagers)
{
    try
    {
        villagerId++;

        foreach (var language in languages)
        {
            string languageUrl = language == "EN" ? baseUrl : $"https://{language.ToLower()}.stardewvalleywiki.com";
            string villagerName = villager.Value.ContainsKey(language) ? villager.Value[language].Name : villager.Key;
            bool hasFamily = villager.Value.ContainsKey(language) ? villager.Value[language].HasFamily : false;
            bool hasClinicVisit = villager.Value.ContainsKey(language) ? villager.Value[language].HasClinicVisit : false;

            VillagerModel villagerData = villagerManager.GetVillagerPrimaryData($"{languageUrl}/{villagerName}",
                                                                                villager.Value,
                                                                                language,
                                                                                villagerId);

            villagerManager.ConvertToJson(villagerData, language);
        }
    }
    catch (Exception ex)
    {
        villagersFailure.Add(villager);
        Console.WriteLine($"Something has gone wrong {ex}");
        Console.WriteLine(villagersFailure);
    }
}