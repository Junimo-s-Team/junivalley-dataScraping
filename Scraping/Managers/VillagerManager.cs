using HtmlAgilityPack;
using PuppeteerSharp;
using Scraping.Common;
using Scraping.Models;
using System.Net;

namespace Scraping.Managers
{
    public class VillagerManager : BaseManager
    {
        // Constructor para recibir la instancia del navegador
        public VillagerManager(IBrowser browser) : base(browser)
        {
        }

        //Villager Detail
        public async Task<VillagerModel> GetVillagerPrimaryData(string url, VillagerModel villager, string languageCode)
        {
            HtmlDocument htmlDocument = await GetDocument(url);

            // Obtenemos el diccionario de claves para el idioma actual
            var keys = WikiKeys.GetKeys(languageCode);

            // Usamos un anclaje robusto: la tabla "infobox"
            HtmlNode infoboxNode = htmlDocument.DocumentNode.SelectSingleNode("//table[@id='infoboxtable']");
            if (infoboxNode == null) throw new Exception("No se pudo encontrar la tabla 'infobox' en la página.");

            // Buscamos elementos por su contenido, no por su posición.
            string GetInfoboxValue(string translatedKey) => infoboxNode.SelectSingleNode($".//td[@id='infoboxsection' and contains(., '{translatedKey}')]/following-sibling::td[1]")?.InnerText.Trim() ?? string.Empty;
            HtmlNode GetInfoboxNode(string translatedKey) => infoboxNode.SelectSingleNode($".//td[@id='infoboxsection' and contains(., '{translatedKey}')]/following-sibling::td[1]");

            string birthdayValue = GetInfoboxValue(keys["Birthday"]);
            var addressNode = GetInfoboxNode(keys["Address"])?.SelectSingleNode(".//a");
            string addressValue = addressNode?.InnerText.Trim() ?? string.Empty;
            string addressValueUrl = addressNode?.GetAttributeValue("href", string.Empty) ?? string.Empty;

            VillagerModel newVillager = new VillagerModel
            {
                Name = villager.Name.Trim(),
                Language = languageCode,
                Birthday = WebUtility.HtmlDecode(birthdayValue).Trim(),
                Address = addressValue,
                HasFamily = villager.HasFamily,
                Family = villager.HasFamily ? GetFamilyPeopleForVillager(infoboxNode, keys) : new List<FamilyModel>(),
                LivesIn = GetInfoboxValue(keys["LivesIn"]),
                HasClinicVisit = villager.HasClinicVisit,
                BestGifts = GetBestGiftsForVillager(infoboxNode, keys),
                TimeLocation = GetTimeLocation(htmlDocument, keys),
                // Usamos las claves correctas que corresponden a los IDs de los spans
                LovedGifts = GetGiftsForVillager(htmlDocument, keys["Love"]),
                LikedGifts = GetGiftsForVillager(htmlDocument, keys["Like"]),
                NeutralGifts = GetGiftsForVillager(htmlDocument, keys["Neutral"]),
                DislikeGifts = GetGiftsForVillager(htmlDocument, keys["Dislike"]),
                HateGifts = GetGiftsForVillager(htmlDocument, keys["Hate"]),
                Movies = GetMoviesForVillager(htmlDocument, keys),
                Concessions = GetConcessionsForVillager(htmlDocument, keys),
                ClinicVisit = WebUtility.HtmlDecode(GetInfoboxValue(keys["Clinic"])).Trim(),
                HeartEvents = GetHeartEventsForVillager(htmlDocument, keys),
                CanBeMarriage = villager.CanBeMarriage,
                Portraits = GetPortraitsForVillager(htmlDocument),
                SpousePatios = villager.CanBeMarriage ? GetSpousePatios(htmlDocument) : new List<SpousePatioModel>()
            };

            // Asignar propiedades menos fiables por separado para evitar que un fallo detenga todo.
            var descriptionNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='infoboxborder']/following-sibling::table[1]//td[contains(@class, 'quotetext')]");
            newVillager.Description = descriptionNode != null ? WebUtility.HtmlDecode(descriptionNode.InnerText).Trim() : string.Empty;

            var timelineHeader = htmlDocument.DocumentNode.SelectSingleNode("//span[@id='Timeline' or @id='L.C3.ADnea_de_Tiempo' or @id='Línea_de_Tiempo']");
            newVillager.TimeLine = timelineHeader?.SelectSingleNode("ancestor::h2/following-sibling::div[1]//img")?.GetAttributeValue("src", string.Empty) ?? string.Empty;

            // Obtener imágenes de la casa (si la dirección existe)
            if (!string.IsNullOrEmpty(addressValueUrl))
            {
                string addressUrl = $"https://{GeneralConstants.BASE_URL}{addressValueUrl}";
                
                // Hacemos una sola llamada que navega y espera por el selector del mapa.
                // Si el selector no aparece, el método continuará de todos modos.
                HtmlDocument htmlDocumentAddressDetail = await GetDocument(addressUrl, "div.location-map img");

                // Selectores robustos para las imágenes de la casa
                var outsideHouseImageNode = htmlDocumentAddressDetail.DocumentNode.SelectSingleNode("//div[@id='infoboxborder']//a[@class='image']/img");
                var mapHouseImageNode = htmlDocumentAddressDetail.DocumentNode.SelectSingleNode("//div[contains(@class, 'location-map')]//img");
                
                var outsideImageSrc = outsideHouseImageNode?.GetAttributeValue("src", string.Empty) ?? string.Empty;
                var mapImageSrc = mapHouseImageNode?.GetAttributeValue("data-src", string.Empty) ?? string.Empty;

                // Aseguramos que las URLs sean completas si son relativas
                newVillager.OutsideHouseImage = outsideImageSrc.StartsWith("/") ? $"https://{GeneralConstants.BASE_URL}{outsideImageSrc}" : outsideImageSrc;
                newVillager.MapHouseImage = mapImageSrc.StartsWith("/") ? $"https://{GeneralConstants.BASE_URL}{mapImageSrc}" : mapImageSrc;
            }

            return newVillager;
        }

        //Time location depends on seasons
        private List<TimeLocationModel> GetTimeLocation(HtmlDocument htmlDocument, Dictionary<string, string> keys)
        {
            List<TimeLocationModel> timeLocationList = new List<TimeLocationModel>();

            // 1. Find the "Schedule" header
            var scheduleHeader = htmlDocument.DocumentNode.SelectSingleNode($"//span[@id='{keys["Schedule"]}']/ancestor::h2");
            if (scheduleHeader == null) return timeLocationList;

            // 2. Find all collapsible season tables that follow the header
            var seasonTables = scheduleHeader.SelectNodes("following-sibling::table[contains(@class, 'mw-collapsible')]");
            if (seasonTables == null) return timeLocationList;

            foreach (var seasonTable in seasonTables)
            {
                // Extract the season name from the table header
                string seasonName = seasonTable.SelectSingleNode(".//th[contains(@style, 'background-color')]//a")?.InnerText.Trim() ?? "Unknown Season";

                // Get the main content cell for the season
                var contentCell = seasonTable.SelectSingleNode(".//tr/td");
                if (contentCell == null) continue;

                string currentCondition = "Regular";

                // Iterate through all child nodes (paragraphs and tables) in order
                foreach (var node in contentCell.ChildNodes)
                {
                    if (node.Name == "p" && !string.IsNullOrWhiteSpace(node.InnerText))
                    {
                        currentCondition = WebUtility.HtmlDecode(node.InnerText).Trim();
                    }
                    else if (node.Name == "table" && node.HasClass("wikitable"))
                    {
                        var rows = node.SelectNodes(".//tr[td]");
                        if (rows == null) continue;

                        foreach (var row in rows)
                        {
                            var cells = row.SelectNodes("td");
                            if (cells != null && cells.Count >= 2)
                            {
                                timeLocationList.Add(new TimeLocationModel
                                {
                                    Day = $"{seasonName} - {currentCondition}",
                                    Time = WebUtility.HtmlDecode(cells[0].InnerText).Trim(),
                                    Location = WebUtility.HtmlDecode(cells[1].InnerText).Trim()
                                });
                            }
                        }
                    }
                }
            }
            return timeLocationList;
        }

        //GET Heart Events
        private List<HeartEventsModel> GetHeartEventsForVillager(HtmlDocument htmlDocument, Dictionary<string, string> keys)
        {
            List<HeartEventsModel> heartEventList = new List<HeartEventsModel>();

            // 1. Encontrar el anclaje: el encabezado H2 de "Heart Events"
            HtmlNode heartEventsHeader = htmlDocument.DocumentNode.SelectSingleNode($"//span[@id='{keys["HeartEvents"]}']/ancestor::h2");
            if (heartEventsHeader == null) return heartEventList; // No hay eventos de corazón

            // 2. Encontrar todos los encabezados H3 que son hermanos siguientes (los títulos de cada evento)
            HtmlNodeCollection eventTitleNodes = heartEventsHeader.SelectNodes("following-sibling::h3");
            if (eventTitleNodes == null) return heartEventList;

            foreach (var titleNode in eventTitleNodes)
            {
                // 3. Para cada título, buscar la información en los elementos que le siguen
                var heartEvent = new HeartEventsModel
                {
                    Title = titleNode.SelectSingleNode(".//span[@class='mw-headline']")?.InnerText.Trim() ?? string.Empty
                };

                // El siguiente párrafo <p> suele contener la imagen del corazón
                var imageParagraph = titleNode.SelectSingleNode("following-sibling::p[1]");
                heartEvent.HeartsImage = imageParagraph?.SelectSingleNode(".//img")?.GetAttributeValue("src", string.Empty) ?? string.Empty;

                // El siguiente párrafo <p> después de la imagen suele ser la descripción
                var detailsParagraph = imageParagraph?.SelectSingleNode("following-sibling::p[1]");
                if (detailsParagraph != null)
                {
                    // Limpiamos el texto para quitar las notas de edición como "[1]"
                    detailsParagraph.SelectNodes(".//sup")?.ToList().ForEach(n => n.Remove());
                    heartEvent.Details = WebUtility.HtmlDecode(detailsParagraph.InnerText).Trim();
                }

                heartEventList.Add(heartEvent);
            }

            return heartEventList;
        }

        //GET Family
        private List<FamilyModel> GetFamilyPeopleForVillager(HtmlNode infoboxNode, Dictionary<string, string> keys)
        {
            List<FamilyModel> familyList = new List<FamilyModel>();

            // 1. Buscar la celda de la familia usando su título traducido
            HtmlNode familyCell = infoboxNode.SelectSingleNode($".//td[@id='infoboxsection' and contains(., '{keys["Family"]}')]/following-sibling::td[1]");
            if (familyCell == null) return familyList;

            // 2. Iterar sobre cada miembro de la familia, que está en un párrafo <p>
            foreach (HtmlNode pNode in familyCell.SelectNodes(".//p"))
            {
                var nameNode = pNode.SelectSingleNode(".//a");
                if (nameNode != null)
                {
                    FamilyModel person = new FamilyModel
                    {
                        Image = pNode.SelectSingleNode(".//img")?.GetAttributeValue("src", string.Empty) ?? string.Empty,
                        Name = nameNode.InnerText.Trim(),
                        Description = pNode.InnerText.Substring(pNode.InnerText.IndexOf("(") + 1).Replace("(", string.Empty).Replace(")", string.Empty) ?? string.Empty
                    };
                    familyList.Add(person);
                }
            }

            return familyList;
        }

        //GET Best Gifts
        private List<BestGiftsModel> GetBestGiftsForVillager(HtmlNode infoboxNode, Dictionary<string, string> keys)
        {
            List<BestGiftsModel> bestGifts = new List<BestGiftsModel>();
            
            // 1. Buscar la celda que contiene los mejores regalos usando su título
            HtmlNode bestGiftsCell = infoboxNode.SelectSingleNode($".//td[@id='infoboxsection' and contains(., '{keys["BestGifts"]}')]/following-sibling::td[1]");
            // Fallback: Si no encuentra "Best Gifts", busca "Loved Gifts" (inconsistencia de la wiki)
            if (bestGiftsCell == null)
            {
                bestGiftsCell = infoboxNode.SelectSingleNode($".//td[@id='infoboxsection' and contains(., 'Loved Gifts')]/following-sibling::td[1]");
            }

            if (bestGiftsCell == null) return bestGifts;

            // 2. Iterar sobre cada regalo, que está dentro de un <span>
            foreach (var giftSpan in bestGiftsCell.SelectNodes(".//span"))
            {
                var nameNode = giftSpan.SelectSingleNode(".//a");
                var imageNode = giftSpan.SelectSingleNode(".//img");

                if (nameNode != null && imageNode != null)
                {
                    bestGifts.Add(new BestGiftsModel
                    {
                        Name = nameNode.InnerText.Trim(),
                        Image = imageNode.GetAttributeValue("src", string.Empty)
                    });
                }
            }
            return bestGifts;
        }

        //GET Gifts
        private List<ItemsClass> GetGiftsForVillager(HtmlDocument htmlDocument, string translatedGiftType)
        {
             List<ItemsClass> giftList = new List<ItemsClass>();

            // 1. El selector ahora busca el ID del encabezado, que es más fiable.
            // El texto del tipo de regalo (Love, Like, etc.) es el ID del span.
            var giftHeaderNode = htmlDocument.DocumentNode.SelectSingleNode($"//span[@id='{translatedGiftType}']");
            if (giftHeaderNode == null) return giftList;

            // 2. La tabla de regalos es la que sigue al encabezado h3 que contiene el span.
            var giftTableNode = giftHeaderNode.SelectSingleNode("ancestor::h3/following-sibling::table[contains(@class, 'wikitable')]");
            if (giftTableNode == null) return giftList;

            // 3. Iteramos sobre cada fila de la tabla, saltando la cabecera (th).
            var itemRows = giftTableNode.SelectNodes(".//tr[td]");
            if (itemRows == null) return giftList;

            foreach (var row in itemRows)
            {
                var cells = row.SelectNodes("td");
                if (cells == null) continue;

                // Maneja filas especiales como "All Universal Loves" que solo tienen una o dos celdas.
                if (cells.Count < 4)
                {
                    var specialItemText = cells[0].InnerText.Trim();
                    if (!string.IsNullOrEmpty(specialItemText))
                    {
                        giftList.Add(new ItemsClass { Name = specialItemText });
                    }
                    continue;
                }

                // Procesa una fila de regalo normal.
                var item = new ItemsClass
                {
                    Id = giftList.Count,
                    Image = cells[0].SelectSingleNode(".//img")?.GetAttributeValue("src", string.Empty) ?? string.Empty,
                    Name = cells[1].InnerText.Trim(),
                    Description = cells[2].InnerText.Trim(),
                    Source = cells[3].InnerText.Trim(),
                    Ingredients = (cells.Count > 4) ? GetIngredientsModel(cells[4]) : new List<IngredientsModel>()
                };
                giftList.Add(item);
            }

            return giftList;
        }

        //GET Ingredients
        private List<IngredientsModel> GetIngredientsModel(HtmlNode tdNodes)
        {
            List<IngredientsModel> ingredientList = new List<IngredientsModel>();
            if (tdNodes == null) return ingredientList;

            // Busca todos los spans que contienen un ingrediente.
            var ingredientSpans = tdNodes.SelectNodes(".//span[@class='nametemplate']");
            if (ingredientSpans == null) return ingredientList;

            foreach (var span in ingredientSpans)
            {
                var text = span.InnerText.Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    // Maneja casos especiales como "Any Fruit"
                    if (text.ToLower().Contains("any fruit"))
                    {
                        IngredientsModel ingredientsBasicModel = new IngredientsModel
                        {
                            Name = "Any Fruit",
                            Quantity = "1"
                        };
                        continue;
                    }

                    // Extrae la cantidad del final del texto, si existe.
                    string quantity = "1"; // Por defecto es 1
                    if (text.Contains('(') && text.EndsWith(')'))
                    {
                        int openParen = text.LastIndexOf('(');
                        quantity = text.Substring(openParen + 1, text.Length - openParen - 2);
                    }

                    IngredientsModel ingredientsModel = new IngredientsModel
                    {
                        Image = span.SelectSingleNode(".//img")?.GetAttributeValue("src", string.Empty) ?? string.Empty,
                        Name = span.SelectSingleNode(".//a")?.InnerText.Trim() ?? string.Empty,
                        Quantity = quantity
                    };
                    ingredientList.Add(ingredientsModel);
                }
            }
            return ingredientList;
        }

        //GET Movies
        private List<MoviesConcessionsModel> GetMoviesForVillager(HtmlDocument htmlDocument, Dictionary<string, string> keys)
        {
            List<MoviesConcessionsModel> movieList = new List<MoviesConcessionsModel>();

            // 1. Anclaje robusto: el encabezado de la sección.
            var moviesHeader = htmlDocument.DocumentNode.SelectSingleNode($"//span[@id='{keys["MoviesAndConcessions"]}']");
            if (moviesHeader == null) return movieList;

            // 2. La tabla de películas es la primera 'wikitable' dentro de la primera celda de la tabla principal.
            var moviesTable = moviesHeader.SelectSingleNode("ancestor::h2/following-sibling::table[1]//table[contains(@class, 'wikitable')]");
            if (moviesTable == null) return movieList;
            
            return ParseMoviesConcessionsTable(moviesTable);
        }

        //GET Concessions
        private List<MoviesConcessionsModel> GetConcessionsForVillager(HtmlDocument htmlDocument, Dictionary<string, string> keys)
        {
            // La lógica es idéntica a la de las películas, solo cambia la tabla que buscamos.
            var moviesHeader = htmlDocument.DocumentNode.SelectSingleNode($"//span[@id='{keys["MoviesAndConcessions"]}']");
            if (moviesHeader == null) return new List<MoviesConcessionsModel>();

            // La tabla de concesiones es la segunda 'wikitable' dentro de la tabla principal.
            var concessionsTable = moviesHeader.SelectNodes("ancestor::h2/following-sibling::table[1]//table[contains(@class, 'wikitable')]")?.ElementAtOrDefault(1);
            if (concessionsTable == null) return new List<MoviesConcessionsModel>();

            // Reutilizamos la misma lógica de parseo que para las películas.
            return ParseMoviesConcessionsTable(concessionsTable);
        }

        private List<MoviesConcessionsModel> ParseMoviesConcessionsTable(HtmlNode tableNode)
        {
            var items = new List<MoviesConcessionsModel>();
            if (tableNode == null) return items;

            string currentType = string.Empty;
            foreach (var row in tableNode.SelectNodes(".//tr"))
            {
                var headerCell = row.SelectSingleNode("th");
                if (headerCell != null)
                {
                    // Limpiamos el tipo para que sea solo "Love", "Like", "Dislike"
                    currentType = headerCell.InnerText.Trim().Split(' ')[0];
                    continue;
                }

                var dataCell = row.SelectSingleNode("td");
                if (dataCell == null) continue;

                // Reemplazamos los <p> y <br> por un carácter especial para poder dividirlos.
                dataCell.SelectNodes(".//p|.//br")?.ToList().ForEach(n => n.ParentNode.ReplaceChild(HtmlNode.CreateNode("|"), n));

                // Dividimos el contenido de la celda por el carácter especial.
                var entries = dataCell.InnerHtml.Split('|', StringSplitOptions.RemoveEmptyEntries);

                foreach (var entry in entries)
                {
                    var tempDoc = new HtmlDocument();
                    tempDoc.LoadHtml(entry);
                    var text = WebUtility.HtmlDecode(tempDoc.DocumentNode.InnerText).Trim();

                    if (!string.IsNullOrEmpty(text) && text != "Everything else")
                    {
                        items.Add(new MoviesConcessionsModel
                        {
                            Type = currentType,
                            Title = text,
                            Image = tempDoc.DocumentNode.SelectSingleNode("//img")?.GetAttributeValue("src", string.Empty) ?? string.Empty
                        });
                    }
                }
            }
            return items;
        }

        //GET Portraits Images
        private List<string> GetPortraitsForVillager(HtmlDocument htmlDocument)
        {
            List<string> portraitImagesList = new List<string>();

            // 1. Anclaje robusto: el encabezado de la sección "Portraits" o "Retratos".
            var portraitsHeader = htmlDocument.DocumentNode.SelectSingleNode("//span[@id='Portraits' or @id='Retratos']");
            if (portraitsHeader == null) return portraitImagesList;

            // 2. Seleccionar todas las galerías de imágenes que siguen al encabezado.
            var galleryNodes = portraitsHeader.SelectNodes("ancestor::h2/following-sibling::ul[contains(@class, 'gallery')]");
            if (galleryNodes == null) return portraitImagesList;

            // 3. Extraer la URL de cada imagen dentro de las galerías.
            foreach (var gallery in galleryNodes)
            {
                var imageNodes = gallery.SelectNodes(".//a[@class='image']/img");
                if (imageNodes == null) continue;

                foreach (var imgNode in imageNodes)
                {
                    var imageSrc = imgNode.GetAttributeValue("src", string.Empty);
                    if (!string.IsNullOrEmpty(imageSrc))
                    {
                        portraitImagesList.Add(imageSrc);
                    }
                }
            }

            return portraitImagesList;
        }

        //GET Spouse Patios
        private List<SpousePatioModel> GetSpousePatios(HtmlDocument htmlDocument)
        {
            var spousePatios = new List<SpousePatioModel>();

            // 1. Anclaje: el encabezado de la sección "Marriage".
            var marriageHeader = htmlDocument.DocumentNode.SelectSingleNode("//span[@id='Marriage' or @id='Matrimonio']");
            if (marriageHeader == null) return spousePatios;

            // 2. Buscar la galería de imágenes que sigue al encabezado.
            var galleryNode = marriageHeader.SelectSingleNode("ancestor::h2/following-sibling::ul[contains(@class, 'gallery')]");
            if (galleryNode == null) return spousePatios;

            // 3. Iterar sobre cada elemento de la galería.
            var galleryItems = galleryNode.SelectNodes(".//li[@class='gallerybox']");
            if (galleryItems == null) return spousePatios;

            foreach (var item in galleryItems)
            {
                spousePatios.Add(new SpousePatioModel
                {
                    Image = item.SelectSingleNode(".//a[@class='image']/img")?.GetAttributeValue("src", string.Empty) ?? string.Empty,
                    Description = WebUtility.HtmlDecode(item.SelectSingleNode(".//div[@class='gallerytext']")?.InnerText.Trim() ?? string.Empty)
                });
            }
            return spousePatios;
        }
    }
}