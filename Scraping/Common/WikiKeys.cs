namespace Scraping.Common
{
    public static class WikiKeys
    {
        private static readonly Dictionary<string, Dictionary<string, string>> Translations = new Dictionary<string, Dictionary<string, string>>
        {
            // Claves para el Infobox
            { "Birthday", new Dictionary<string, string> { { "EN", "Birthday" }, { "ES", "Cumpleaños" } } },
            { "Address", new Dictionary<string, string> { { "EN", "Address" }, { "ES", "Dirección" } } },
            { "LivesIn", new Dictionary<string, string> { { "EN", "Lives In" }, { "ES", "Vive en" } } },
            { "Family", new Dictionary<string, string> { { "EN", "Family" }, { "ES", "Familia" } } },
            { "Clinic", new Dictionary<string, string> { { "EN", "Clinic" }, { "ES", "Clínica" } } },
            { "BestGifts", new Dictionary<string, string> { { "EN", "Best Gifts" }, { "ES", "Mejores regalos" } } },

            // Claves para encabezados de sección (IDs o texto)
            { "HeartEvents", new Dictionary<string, string> { { "EN", "Heart_Events" }, { "ES", "Eventos_de_corazón" } } },
            { "Gifts", new Dictionary<string, string> { { "EN", "Gifts" }, { "ES", "Regalos" } } },
            { "Schedule", new Dictionary<string, string> { { "EN", "Schedule" }, { "ES", "Agenda" } } },
            { "MoviesAndConcessions", new Dictionary<string, string> { { "EN", "Movies_.26_Concessions" }, { "ES", "Pel.C3.ADculas_y_Refrigerios" } } },
            { "Interior", new Dictionary<string, string> { { "EN", "Interior" }, { "ES", "Interior" } } },

            // Claves para tipos de regalos
            { "Love", new Dictionary<string, string> { { "EN", "Love" }, { "ES", "Le_encanta" } } },
            { "Like", new Dictionary<string, string> { { "EN", "Like" }, { "ES", "Le_gusta" } } },
            { "Neutral", new Dictionary<string, string> { { "EN", "Neutral" }, { "ES", "Neutral" } } },
            { "Dislike", new Dictionary<string, string> { { "EN", "Dislike" }, { "ES", "No_le_gusta" } } },
            { "Hate", new Dictionary<string, string> { { "EN", "Hate" }, { "ES", "Odia" } } },

            // MARK: - Missions
            
            { "QuestItems", new Dictionary<string, string> { { "EN", "List_of_Quest_Items" }, { "ES", "Lista_de_objetos_de_misi.C3.B3n" } } },

            // MARK: - Farmhouse
            { "FarmhouseUpgrades", new Dictionary<string, string> { { "EN", "Upgrades" }, { "ES", "Mejoras" } } },
            { "FarmhouseRenovations", new Dictionary<string, string> { { "EN", "Renovations" }, { "ES", "Renovaciones" } } },
            { "SpouseRooms", new Dictionary<string, string> { { "EN", "Spouse.2FRoommate_Rooms" }, { "ES", "Habitaciones_para_c.C3.B3nyuges.2Fcompa.C3.B1eros" } } },

            // MARK: - Animals
            { "Pets", new Dictionary<string, string> { { "EN", "Pets" }, { "ES", "Mascotas" } } },
            { "Horse", new Dictionary<string, string> { { "EN", "Horse" }, { "ES", "Caballo" } } },
            { "CoopAnimals", new Dictionary<string, string> { { "EN", "Coop_Animals" }, { "ES", "Animales_de_corral" } } },
            { "BarnAnimals", new Dictionary<string, string> { { "EN", "Barn_Animals" }, { "ES", "Animales_de_establo" } } },
            { "OtherAnimals", new Dictionary<string, string> { { "EN", "Other_Animals" }, { "ES", "Otros_Animales" } } },
            { "CatGifts", new Dictionary<string, string> { { "EN", "Cat Gifts" }, { "ES", "Regalos de gatos" } } },
            { "DogGifts", new Dictionary<string, string> { { "EN", "Dog Gifts" }, { "ES", "Regalos de perros" } } },
            { "TurtleGifts", new Dictionary<string, string> { { "EN", "Turtle Gifts" }, { "ES", "Regalos de tortuga" } } },
        };

        /// <summary>
        /// Obtiene un diccionario con las claves traducidas para un idioma específico.
        /// </summary>
        /// <param name="languageCode">El código de idioma (ej. "ES", "EN").</param>
        /// <returns>Un diccionario con las claves traducidas.</returns>
        public static Dictionary<string, string> GetKeys(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode) || languageCode.ToUpper() == "EN")
            {
                languageCode = "EN"; // Inglés por defecto
            }
            else
            {
                languageCode = languageCode.ToUpper();
            }

            var translatedKeys = new Dictionary<string, string>();
            foreach (var keyPair in Translations)
            {
                // Si existe una traducción para el idioma, la usa. Si no, usa inglés como fallback.
                translatedKeys[keyPair.Key] = keyPair.Value.GetValueOrDefault(languageCode, keyPair.Value["EN"]);
            }
            return translatedKeys;
        }
    }
}