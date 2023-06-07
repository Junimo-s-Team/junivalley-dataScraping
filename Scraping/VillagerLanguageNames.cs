using System;

namespace Scraping
{
    public class VillagerLanguageNames
    {
        public Dictionary<string, Dictionary<string, (string Name, bool HasFamily, bool HasClinicVisit)>> Villagers { get; }

        public VillagerLanguageNames()
        {
            Villagers = new Dictionary<string, Dictionary<string, (string Name, bool HasFamily, bool HasClinicVisit)>>
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
                { "Sebastian", new Dictionary<string, (string Name, bool HasFamily, bool HasClinicVisit)>
                {
                    { "EN", ("Sebastian", true, true) },
                    { "ES", ("Sebastian", true, true) },
                    { "FR", ("Sebastian", true, true) },
                    { "PT", ("Sebastian", true, true) },
                    { "DE", ("Sebastian", true, true) },
                    { "JA", ("セバスチャン", true, true) },
                }
                },
                { "Shane", new Dictionary<string, (string Name, bool HasFamily, bool HasClinicVisit)>
                {
                    { "EN", ("Shane", true, false) },
                    { "ES", ("Shane", true, false) },
                    { "FR", ("Shane", true, false) },
                    { "PT", ("Shane", true, false) },
                    { "DE", ("Shane", true, false) },
                    { "JA", ("シェーン", true, false) },
                }
                },
                { "Abigail", new Dictionary<string, (string Name, bool HasFamily, bool HasClinicVisit)>
                {
                    { "EN", ("Abigail", true, true) },
                    { "ES", ("Abigail", true, true) },
                    { "FR", ("Abigail", true, true) },
                    { "PT", ("Abigail", true, true) },
                    { "DE", ("Abigail", true, true) },
                    { "JA", ("アビゲイル", true, true) },
                }
                },
                { "Emily", new Dictionary<string, (string Name, bool HasFamily, bool HasClinicVisit)>
                {
                    { "EN", ("Emily", true, true) },
                    { "ES", ("Emily", true, true) },
                    { "FR", ("Emily", true, true) },
                    { "PT", ("Emily", true, true) },
                    { "DE", ("Emily", true, true) },
                    { "JA", ("エミリー", true, true) },
                }
                },
                { "Haley", new Dictionary<string, (string Name, bool HasFamily, bool HasClinicVisit)>
                {
                    { "EN", ("Haley", true, true) },
                    { "ES", ("Haley", true, true) },
                    { "FR", ("Haley", true, true) },
                    { "PT", ("Haley", true, true) },
                    { "DE", ("Haley", true, true) },
                    { "JA", ("ヘイリー", true, true) },
                }
                },
                { "Leah", new Dictionary<string, (string Name, bool HasFamily, bool HasClinicVisit)>
                {
                    { "EN", ("Leah", false, true) },
                    { "ES", ("Leah", false, true) },
                    { "FR", ("Leah", false, true) },
                    { "PT", ("Leah", false, true) },
                    { "DE", ("Leah", false, true) },
                    { "JA", ("リア", false, true) },
                }
                },
                { "Maru", new Dictionary<string, (string Name, bool HasFamily, bool HasClinicVisit)>
                {
                    { "EN", ("Maru", true, false) },
                    { "ES", ("Maru", true, false) },
                    { "FR", ("Maru", true, false) },
                    { "PT", ("Maru", true, false) },
                    { "DE", ("Maru", true, false) },
                    { "JA", ("マル", true, false) },
                }
                },
                { "Penny", new Dictionary<string, (string Name, bool HasFamily, bool HasClinicVisit)>
                {
                    { "EN", ("Penny", true, true) },
                    { "ES", ("Penny", true, true) },
                    { "FR", ("Penny", true, true) },
                    { "PT", ("Penny", true, true) },
                    { "DE", ("Penny", true, true) },
                    { "JA", ("ペニー", true, true) },
                }
                },
            };
        }
    }
}

