﻿using Scraping.Models;

namespace Scraping.Common
{
    static class GeneralConstants
    {
        public const string BASE_URL = "stardewvalleywiki.com";
        public static readonly List<string> LANGUAGES = new List<string> { "", "ES." };

        // IMPORTANTE: Cada aldeano en esta lista debe tener una entrada correspondiente en VillagerLanguageNames.cs
        public static readonly List<VillagerModel> VILLAGERS = new List<VillagerModel>
        {
            //marriage candidates

            //Boys
            new VillagerModel{ Id = 1, Name = "Alex", HasFamily = true, HasClinicVisit = true, CanBeMarriage = true },
            new VillagerModel{ Id = 2, Name = "Elliott", HasFamily = false, HasClinicVisit = true, CanBeMarriage = true },
            new VillagerModel{ Id = 3, Name = "Harvey", HasFamily = false, HasClinicVisit = false, CanBeMarriage = true },
            new VillagerModel{ Id = 4, Name = "Sam", HasFamily = true, HasClinicVisit = true, CanBeMarriage = true },
            new VillagerModel{ Id = 5, Name = "Sebastian", HasFamily = true, HasClinicVisit = true, CanBeMarriage = true },
            new VillagerModel{ Id = 6, Name = "Shane", HasFamily = true, HasClinicVisit = false, CanBeMarriage = true },

            //Girls
            new VillagerModel{ Id = 7, Name = "Abigail", HasFamily = true, HasClinicVisit = true, CanBeMarriage = true },
            new VillagerModel{ Id = 8, Name = "Emily", HasFamily = true, HasClinicVisit = true, CanBeMarriage = true },
            new VillagerModel{ Id = 9, Name = "Haley", HasFamily = true, HasClinicVisit = true, CanBeMarriage = true },
            new VillagerModel{ Id = 10, Name = "Leah", HasFamily = false, HasClinicVisit = true, CanBeMarriage = true },
            new VillagerModel{ Id = 11, Name = "Maru", HasFamily = true, HasClinicVisit = false, CanBeMarriage = true },
            new VillagerModel{ Id = 12, Name = "Penny", HasFamily = true, HasClinicVisit = true, CanBeMarriage = true },

            //non-marriage candidates
            new VillagerModel{ Id = 13, Name = "Caroline", HasFamily = true, HasClinicVisit = true },
            new VillagerModel{ Id = 14, Name = "Clint", HasFamily = false, HasClinicVisit = true },
            new VillagerModel{ Id = 15, Name = "Demetrius", HasFamily = true, HasClinicVisit = true },
            new VillagerModel{ Id = 16, Name = "Dwarf", HasFamily = false, HasClinicVisit = false },
            new VillagerModel{ Id = 17, Name = "Evelyn", HasFamily = true, HasClinicVisit = true },
            new VillagerModel{ Id = 18, Name = "George", HasFamily = true, HasClinicVisit = true },
            new VillagerModel{ Id = 19, Name = "Gus", HasFamily = false, HasClinicVisit = true },
            new VillagerModel{ Id = 20, Name = "Jas", HasFamily = true, HasClinicVisit = true },
            new VillagerModel{ Id = 21, Name = "Jodi", HasFamily = true, HasClinicVisit = true },
            new VillagerModel{ Id = 22, Name = "Kent", HasFamily = true, HasClinicVisit = false },
            new VillagerModel{ Id = 23, Name = "Krobus", HasFamily = false, HasClinicVisit = false },
            new VillagerModel{ Id = 24, Name = "Leo", HasFamily = false, HasClinicVisit = false },
            new VillagerModel{ Id = 25, Name = "Lewis", HasFamily = false, HasClinicVisit = true },
            new VillagerModel{ Id = 26, Name = "Linus", HasFamily = false, HasClinicVisit = false },
            new VillagerModel{ Id = 27, Name = "Marnie", HasFamily = true, HasClinicVisit = true },
            new VillagerModel{ Id = 28, Name = "Pam", HasFamily = true, HasClinicVisit = true },
            new VillagerModel{ Id = 29, Name = "Pierre", HasFamily = true, HasClinicVisit = false },
            new VillagerModel{ Id = 30, Name = "Robin", HasFamily = true, HasClinicVisit = true },
            new VillagerModel{ Id = 31, Name = "Sandy", HasFamily = false, HasClinicVisit = false },
            new VillagerModel{ Id = 32, Name = "Vincent", HasFamily = true, HasClinicVisit = true },
            new VillagerModel{ Id = 33, Name = "Willy", HasFamily = false, HasClinicVisit = true },
            new VillagerModel{ Id = 34, Name = "Wizard", HasFamily = false, HasClinicVisit = false }
        };
    }
}
