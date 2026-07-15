﻿namespace Scraping.Common
{
    public static class VillagerLanguageNames
    {
        // Un diccionario donde la clave es el nombre en inglés y el valor es otro diccionario
        // con los nombres traducidos por código de idioma.
        private static readonly Dictionary<string, Dictionary<string, string>> VillagerTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            { "Alex", new Dictionary<string, string> {
                { "EN", "Alex" }, { "ES", "Alex" }, { "FR", "Alex" }, { "PT", "Alex" }, { "DE", "Alex" }, { "JA", "アレックス" }
            }},
            { "Elliott", new Dictionary<string, string> {
                { "EN", "Elliott" }, { "ES", "Elliott" }, { "FR", "Elliott" }, { "PT", "Elliott" }, { "DE", "Elliott" }, { "JA", "エリオット" }
            }},
            { "Harvey", new Dictionary<string, string> {
                { "EN", "Harvey" }, { "ES", "Harvey" }, { "FR", "Harvey" }, { "PT", "Harvey" }, { "DE", "Harvey" }, { "JA", "ハーヴィー" }
            }},
            { "Sam", new Dictionary<string, string> {
                { "EN", "Sam" }, { "ES", "Sam" }, { "FR", "Sam" }, { "PT", "Sam" }, { "DE", "Sam" }, { "JA", "サム" }
            }},
            { "Sebastian", new Dictionary<string, string> {
                { "EN", "Sebastian" }, { "ES", "Sebastian" }, { "FR", "Sebastian" }, { "PT", "Sebastian" }, { "DE", "Sebastian" }, { "JA", "セバスチャン" }
            }},
            { "Shane", new Dictionary<string, string> {
                { "EN", "Shane" }, { "ES", "Shane" }, { "FR", "Shane" }, { "PT", "Shane" }, { "DE", "Shane" }, { "JA", "シェーン" }
            }},
            { "Abigail", new Dictionary<string, string> {
                { "EN", "Abigail" }, { "ES", "Abigail" }, { "FR", "Abigail" }, { "PT", "Abigail" }, { "DE", "Abigail" }, { "JA", "アビゲイル" }
            }},
            { "Emily", new Dictionary<string, string> {
                { "EN", "Emily" }, { "ES", "Emily" }, { "FR", "Emily" }, { "PT", "Emily" }, { "DE", "Emily" }, { "JA", "エミリー" }
            }},
            { "Haley", new Dictionary<string, string> {
                { "EN", "Haley" }, { "ES", "Haley" }, { "FR", "Haley" }, { "PT", "Haley" }, { "DE", "Haley" }, { "JA", "ヘイリー" }
            }},
            { "Leah", new Dictionary<string, string> {
                { "EN", "Leah" }, { "ES", "Leah" }, { "FR", "Leah" }, { "PT", "Leah" }, { "DE", "Leah" }, { "JA", "リア" }
            }},
            { "Maru", new Dictionary<string, string> {
                { "EN", "Maru" }, { "ES", "Maru" }, { "FR", "Maru" }, { "PT", "Maru" }, { "DE", "Maru" }, { "JA", "マル" }
            }},
            { "Penny", new Dictionary<string, string> {
                { "EN", "Penny" }, { "ES", "Penny" }, { "FR", "Penny" }, { "PT", "Penny" }, { "DE", "Penny" }, { "JA", "ペニー" }
            }},
            { "Caroline", new Dictionary<string, string> { { "EN", "Caroline" }, { "ES", "Caroline" } } },
            { "Clint", new Dictionary<string, string> { { "EN", "Clint" }, { "ES", "Clint" } } },
            { "Demetrius", new Dictionary<string, string> { { "EN", "Demetrius" }, { "ES", "Demetrius" } } },
            { "Dwarf", new Dictionary<string, string> { { "EN", "Dwarf" }, { "ES", "Enano" } }},
            { "Evelyn", new Dictionary<string, string> { { "EN", "Evelyn" }, { "ES", "Evelyn" } } },
            { "George", new Dictionary<string, string> { { "EN", "George" }, { "ES", "George" } } },
            { "Gus", new Dictionary<string, string> { { "EN", "Gus" }, { "ES", "Gus" } } },
            { "Jas", new Dictionary<string, string> { { "EN", "Jas" }, { "ES", "Jas" } } },
            { "Jodi", new Dictionary<string, string> { { "EN", "Jodi" }, { "ES", "Jodi" } } },
            { "Kent", new Dictionary<string, string> { { "EN", "Kent" }, { "ES", "Kent" } } },
            { "Krobus", new Dictionary<string, string> { { "EN", "Krobus" }, { "ES", "Krobus" } } },
            { "Leo", new Dictionary<string, string> { { "EN", "Leo" }, { "ES", "Leo" } } },
            { "Lewis", new Dictionary<string, string> { { "EN", "Lewis" }, { "ES", "Lewis" } } },
            { "Linus", new Dictionary<string, string> { { "EN", "Linus" }, { "ES", "Linus" } } },
            { "Marnie", new Dictionary<string, string> { { "EN", "Marnie" }, { "ES", "Marnie" } } },
            { "Pam", new Dictionary<string, string> { { "EN", "Pam" }, { "ES", "Pam" } } },
            { "Pierre", new Dictionary<string, string> { { "EN", "Pierre" }, { "ES", "Pierre" } } },
            { "Robin", new Dictionary<string, string> { { "EN", "Robin" }, { "ES", "Robin" } } },
            { "Sandy", new Dictionary<string, string> { { "EN", "Sandy" }, { "ES", "Sandy" } } },
            { "Vincent", new Dictionary<string, string> { { "EN", "Vincent" }, { "ES", "Vincent" } } },
            { "Willy", new Dictionary<string, string> { { "EN", "Willy" }, { "ES", "Willy" } } },
            { "Wizard", new Dictionary<string, string> { { "EN", "Wizard" }, { "ES", "Rasmodius" } }},
        };

        /// <summary>
        /// Obtiene el nombre traducido de un aldeano.
        /// </summary>
        /// <param name="englishName">El nombre en inglés del aldeano.</param>
        /// <param name="languageCode">El código de idioma (ej. "ES", "JA").</param>
        /// <returns>El nombre traducido. Si no se encuentra, devuelve el nombre en inglés.</returns>
        public static string GetTranslatedName(string englishName, string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
            {
                languageCode = "EN";
            }

            if (VillagerTranslations.TryGetValue(englishName, out var translations) &&
                translations.TryGetValue(languageCode.ToUpper(), out var translatedName))
            {
                return translatedName;
            }

            // Si no se encuentra una traducción, devolver el nombre en inglés como fallback.
            return englishName;
        }
    }
}