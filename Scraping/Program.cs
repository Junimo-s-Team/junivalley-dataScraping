using HtmlAgilityPack;
using Scraping.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using Scraping;

string url = "https://stardewvalleywiki.com";

VillagerManager villagerManager = new VillagerManager();

int villagerId = 0;

try
{
    List<VillagerModel> villagers = new List<VillagerModel>
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
    };


    //Call VillagerManager
    foreach (var villager in villagers)
    {
        villagerId++;
        VillagerModel villagerData = villagerManager.GetVillagerPrimaryData($"{url}/{villager.Name}",
                                                                            villager,
                                                                            villagerId);
        villagerManager.ConvertToJson(villagerData);
    }
}
catch
{
    Console.WriteLine("Something has gone wrong");
    return;
}