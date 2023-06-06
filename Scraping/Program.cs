using HtmlAgilityPack;
using Scraping.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using Scraping;

string baseUrl = "https://stardewvalleywiki.com";
List<string> languages = new List<string> { "EN", "ES", "FR", "PT", "DE", "JA" };

VillagerManager villagerManager = new VillagerManager();
List<KeyValuePair<string, Dictionary<string, (string name, bool hasFamily, bool hasClinicVisit)>>> villagersFailure = new List<KeyValuePair<string, Dictionary<string, (string name, bool hasFamily, bool hasClinicVisit)>>>();
int villagerId = 0;

/*List<VillagerModel> villagers = new List<VillagerModel>
    {
    //marriage candidates
    //Boys
    new VillagerModel{ Name = "Alex", HasFamily = true, HasClinicVisit = true },
    new VillagerModel{ Name = "Elliot", HasFamily = false, HasClinicVisit = true },
    new VillagerModel{ Name = "Harvey", HasFamily = false, HasClinicVisit = false },
    new VillagerModel{ Name = "Sam", HasFamily = true, HasClinicVisit = true },
    new VillagerModel{ Name = "Sebastian", HasFamily = true, HasClinicVisit = true },
    new VillagerModel{ Name = "Shane", HasFamily = true, HasClinicVisit = false },
    //Girls
    new VillagerModel{ Name = "Abigail", HasFamily = true, HasClinicVisit = true },
    new VillagerModel{ Name = "Emily", HasFamily = true, HasClinicVisit = true },
    new VillagerModel{ Name = "Haley", HasFamily = true, HasClinicVisit = true },
    new VillagerModel{ Name = "Leah", HasFamily = false, HasClinicVisit = true },
    new VillagerModel{ Name = "Maru", HasFamily = true, HasClinicVisit = false },
    new VillagerModel{ Name = "Penny", HasFamily = true, HasClinicVisit = true },

    //non-marriage candidates
    new VillagerModel{ Name = "Caroline", HasFamily = true, HasClinicVisit = true },
    new VillagerModel{ Name = "Clint", HasFamily = false, HasClinicVisit = true },
    new VillagerModel{ Name = "Demetrius", HasFamily = true, HasClinicVisit = true },
    new VillagerModel{ Name = "Dwarf", HasFamily = false, HasClinicVisit = false }, // contains differents model (shop, relationships...)
    new VillagerModel{ Name = "Demetrius", HasFamily = true, HasClinicVisit = true }, // clinicVisit is different because are two texts
    new VillagerModel{ Name = "George", HasFamily = true, HasClinicVisit = true },
    new VillagerModel{ Name = "Gus", HasFamily = false, HasClinicVisit = true },
    new VillagerModel{ Name = "Jas", HasFamily = true, HasClinicVisit = true },
    new VillagerModel{ Name = "Jodi", HasFamily = true, HasClinicVisit = true }, // clinicVisit is different because has two different dates 
    new VillagerModel{ Name = "Kent", HasFamily = true, HasClinicVisit = false },
    new VillagerModel{ Name = "Krobus", HasFamily = false, HasClinicVisit = false }, // marriage: no, but can become a roommate
    new VillagerModel{ Name = "Leo", HasFamily = false, HasClinicVisit = false }, // address is different (hut <6 <3 Treehouse (≥6 <3)
    new VillagerModel{ Name = "Lewis", HasFamily = false, HasClinicVisit = true },
    new VillagerModel{ Name = "Linus", HasFamily = false, HasClinicVisit = false },
    new VillagerModel{ Name = "Marnie", HasFamily = true, HasClinicVisit = true }, // clinicVisit is different because has two different dates
    new VillagerModel{ Name = "Pam", HasFamily = true, HasClinicVisit = true },
    new VillagerModel{ Name = "Pierre", HasFamily = true, HasClinicVisit = false },
    new VillagerModel{ Name = "Robin", HasFamily = true, HasClinicVisit = true },
    new VillagerModel{ Name = "Sandy", HasFamily = false, HasClinicVisit = false },
    new VillagerModel{ Name = "Vicent", HasFamily = true, HasClinicVisit = true }, // worse birthday?
    new VillagerModel{ Name = "Willy", HasFamily = false, HasClinicVisit = true },
    new VillagerModel{ Name = "Wizard", HasFamily = false, HasClinicVisit = false },

    //non-giftable candidates
    //Change names depends on language
    new VillagerModel{ Name = "Birdie", HasFamily = false, HasClinicVisit = false },
    new VillagerModel{ Name = "Bouncer", HasFamily = false, HasClinicVisit = false },
    new VillagerModel{ Name = "Gil", HasFamily = false, HasClinicVisit = false },
    new VillagerModel{ Name = "Governor", HasFamily = false, HasClinicVisit = false },
    new VillagerModel{ Name = "Grandpa", HasFamily = false, HasClinicVisit = false },
    new VillagerModel{ Name = "Gunther", HasFamily = false, HasClinicVisit = false },
    new VillagerModel{ Name = "Henchman", HasFamily = false, HasClinicVisit = false },
    new VillagerModel{ Name = "Marlon", HasFamily = false, HasClinicVisit = false },
    new VillagerModel{ Name = "Mr. Qi", HasFamily = false, HasClinicVisit = false },
    new VillagerModel{ Name = "Professor Snail", HasFamily = false, HasClinicVisit = false }
    }; */

Dictionary<string, Dictionary<string, (string Name, bool HasFamily, bool HasClinicVisit)>> villagers = new Dictionary<string, Dictionary<string, (string Name, bool HasFamily, bool HasClinicVisit)>>
{
    { "Alex", new Dictionary<string, (string Name, bool HasFamily, bool HasClinicVisit)>
        {
            { "EN", ("Alex", true, true) },
            { "ES", ("Alex", true, true) },
            { "FR", ("Alex", true, true) },
            { "PT", ("Alex", true, true) },
            { "DE", ("Alex", true, true) },
            { "JA", ("アレックス", true, true) }
        }
    },
    { "Elliott", new Dictionary<string, (string Name, bool HasFamily, bool HasClinicVisit)>
        {
            { "EN", ("Elliott", false, true) },
            { "ES", ("Elliott", false, true) },
            { "FR", ("Elliott", false, true) },
            { "PT", ("Elliott", false, true) },
            { "DE", ("Elliott", false, true) },
            { "JA", ("エリオット", false, true) },
        }
    },
    { "Harvey", new Dictionary<string, (string Name, bool HasFamily, bool HasClinicVisit)>
        {
            { "EN", ("Harvey", false, false) },
            { "ES", ("Harvey", false, false) },
            { "FR", ("Harvey", false, false) },
            { "PT", ("Harvey", false, false) },
            { "DE", ("Harvey", false, false) },
            { "JA", ("ハーヴィー", false, false) },
        }
    },
    { "Sam", new Dictionary<string, (string Name, bool HasFamily, bool HasClinicVisit)>
        {
            { "EN", ("Sam", true, true) },
            { "ES", ("Sam", true, true) },
            { "FR", ("Sam", true, true) },
            { "PT", ("Sam", true, true) },
            { "DE", ("Sam", true, true) },
            { "JA", ("サム", true, true) },
        }
    },
};


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