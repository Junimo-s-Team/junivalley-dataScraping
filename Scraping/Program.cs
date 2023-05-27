using HtmlAgilityPack;
using Scraping.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using Scraping;

string url = "https://stardewvalleywiki.com";

VillagerManager villagerManager = new VillagerManager();

int villagerId = 0;

List<VillagerModel> villagers = new List<VillagerModel>
{
    new VillagerModel{ Name = "Alex", HasFamily = true, HasClinicVisit = true },
    new VillagerModel{ Name = "Elliot", HasFamily = false, HasClinicVisit = true },
    new VillagerModel{ Name = "Harvey", HasFamily = false, HasClinicVisit = false },
    new VillagerModel{ Name = "Sam", HasFamily = true, HasClinicVisit = true },
    new VillagerModel{ Name = "Sebastian", HasFamily = true, HasClinicVisit = true },
    new VillagerModel{ Name = "Shane", HasFamily = true, HasClinicVisit = false }
};

//Call VillagerManager
foreach (var villager in villagers)
{
    villagerId ++;
    VillagerModel villagerData = villagerManager.GetVillagerPrimaryData($"{url}/{villager.Name}",
                                                                        villager,
                                                                        villagerId);
    villagerManager.ConvertToJson(villagerData);
}