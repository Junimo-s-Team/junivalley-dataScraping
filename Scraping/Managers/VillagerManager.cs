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
            string GetInfoboxValue(string translatedKey) => infoboxNode.SelectSingleNode($".//td[@id='infoboxsection' and starts-with(normalize-space(.), '{translatedKey}')]/following-sibling::td[1]")?.InnerText.Trim() ?? string.Empty;
            string GetInfoboxText(string translatedKey) => WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Replace(infoboxNode.SelectSingleNode($".//td[@id='infoboxsection' and starts-with(normalize-space(.), '{translatedKey}')]/following-sibling::td[1]")?.InnerHtml ?? string.Empty, "<.*?>", string.Empty)).Trim();
            HtmlNode GetInfoboxNode(string translatedKey) => infoboxNode.SelectSingleNode($".//td[@id='infoboxsection' and starts-with(normalize-space(.), '{translatedKey}')]/following-sibling::td[1]");

            string birthdayValue = GetInfoboxText(keys["Birthday"]).Replace("\u00A0", " ");
            var addressNode = GetInfoboxNode(keys["Address"])?.SelectSingleNode(".//a");
            string addressValue = addressNode?.InnerText.Trim() ?? string.Empty;
            string clinicVisitValue = GetInfoboxText(keys["Clinic"]).Replace("\u00A0", " ");
            string addressValueUrl = addressNode?.GetAttributeValue("href", string.Empty) ?? string.Empty;

            VillagerModel newVillager = new VillagerModel
            {
                Id = villager.Id,
                Name = villager.Name,
                Language = languageCode,
                Birthday = WebUtility.HtmlDecode(birthdayValue).Trim(),
                Address = addressValue,
                HasFamily = villager.HasFamily,
                Family = villager.HasFamily ? GetFamilyPeopleForVillager(infoboxNode, keys) : new List<FamilyModel>(),
                LivesIn = GetInfoboxValue(keys["LivesIn"]),
                HasClinicVisit = villager.HasClinicVisit,
                ClinicVisit = WebUtility.HtmlDecode(clinicVisitValue).Trim(),
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
                HeartEvents = GetHeartEventsForVillager(htmlDocument, keys),
                CanBeMarriage = villager.CanBeMarriage,
                Portraits = GetPortraitsForVillager(htmlDocument),
                SpousePatios = villager.CanBeMarriage ? GetSpousePatios(htmlDocument) : new List<SpousePatioModel>()
            };

            // Scrape and assign properties that are not in the infobox
            newVillager.Description = GetDescription(htmlDocument);
            newVillager.TimeLine = GetTimelineImage(htmlDocument, keys);

            var houseImages = await GetHouseImages(addressValueUrl, keys, languageCode);
            newVillager.OutsideHouseImage = houseImages.outside;
            newVillager.MapHouseImage = houseImages.map;
            newVillager.InsideHouseImage = houseImages.interior;

            return newVillager;
        }
        //Time location depends on seasons
        private List<TimeLocationModel> GetTimeLocation(HtmlDocument htmlDocument, Dictionary<string, string> keys)
        {
            List<TimeLocationModel> timeLocationList = new List<TimeLocationModel>();

            // 1. Find the "Schedule" header
            var scheduleHeader = htmlDocument.DocumentNode.SelectSingleNode($"//span[@id='{keys["Schedule"]}']/ancestor::h2");
            if (scheduleHeader == null) return timeLocationList;

            // 2. Find the next H2 header to define the boundary of the schedule section
            var nextHeader = scheduleHeader.SelectSingleNode("following-sibling::h2[1]");

            // 3. Select all nodes between the schedule header and the next header
            var contentNodes = new List<HtmlNode>();
            var currentNode = scheduleHeader.NextSibling;
            while (currentNode != null && currentNode != nextHeader)
            {
                contentNodes.Add(currentNode);
                currentNode = currentNode.NextSibling;
            }

            if (!contentNodes.Any()) return timeLocationList;

            // Procesar tablas de horarios, ya sean colapsables o no.
            var scheduleTables = contentNodes.Where(n => n.Name == "table" && (n.HasClass("wikitable") || n.HasClass("mw-collapsible")));

            foreach (var table in scheduleTables)
            {
                // Si es una tabla colapsable, necesitamos procesar su contenido interno.
                if (table.HasClass("mw-collapsible"))
                {
                    var headerNode = table.SelectSingleNode(".//th");
                    string mainContext = headerNode != null ? WebUtility.HtmlDecode(headerNode.InnerText).Replace("\u00A0", " ").Trim().Replace("Collapse", "").Replace("Expand", "").Replace("Contraer", "").Replace("Expandir", "").Replace("ir a ", "").Replace("ir ", "").Trim() : "Unknown";

                    // Caso 1: La tabla contiene sub-tablas anidadas (formato de Alex).
                    var contentCell = table.SelectSingleNode(".//table[contains(@class,'wikitable')]")?.ParentNode;
                    if (contentCell != null)
                    {
                        string currentCondition = "Regular";
                        foreach (var innerNode in contentCell.ChildNodes.Where(n => n.NodeType == HtmlNodeType.Element))
                        {
                            if (innerNode.Name == "p" && innerNode.SelectSingleNode(".//b") != null)
                            {
                                currentCondition = WebUtility.HtmlDecode(innerNode.InnerText).Trim();
                            }
                            else if (innerNode.Name == "table" && innerNode.HasClass("wikitable"))
                            {
                                var rows = innerNode.SelectNodes(".//tr[td]");
                                if (rows == null) continue;

                                foreach (var row in rows)
                                {
                                    var cells = row.SelectNodes("td");
                                    if (cells != null && cells.Count >= 2)
                                    {
                                        string finalDay = currentCondition.StartsWith(mainContext, StringComparison.OrdinalIgnoreCase) ? currentCondition : $"{mainContext} - {currentCondition}";
                                        timeLocationList.Add(new TimeLocationModel
                                        {
                                            Day = finalDay,
                                            Time = WebUtility.HtmlDecode(cells[0].InnerText).Trim(),
                                            Location = WebUtility.HtmlDecode(cells[1].InnerText).Trim()
                                        });
                                    }
                                }
                            }
                        }
                    }
                    // Caso 2: Horarios directamente en las filas de la tabla (como las tablas principales de Pam)
                    else
                    {
                        var directRows = table.SelectNodes(".//tr[td]");
                        if (directRows == null) continue;

                        foreach (var row in directRows)
                        {
                            var cells = row.SelectNodes("td");
                            if (cells != null && cells.Count >= 2)
                            {
                                timeLocationList.Add(new TimeLocationModel
                                {
                                    Day = mainContext, // El contexto principal es el día/condición
                                    Time = WebUtility.HtmlDecode(cells[0].InnerText).Trim(),
                                    Location = WebUtility.HtmlDecode(cells[1].InnerText).Trim()
                                });
                            }
                        }
                    }
                }
                // Si es una tabla de horarios simple (no colapsable)
                else if (table.HasClass("wikitable") && !table.HasClass("mw-collapsible"))
                {
                    var rows = table.SelectNodes(".//tr[td]");
                    if (rows == null) continue;

                    foreach (var row in rows)
                    {
                        var cells = row.SelectNodes("td");
                        if (cells != null && cells.Count >= 2)
                        {
                            // Para tablas simples, el contexto es el encabezado de la tabla si existe, o "Regular"
                            var header = table.SelectSingleNode("preceding-sibling::p[1]/b")?.InnerText.Trim() ?? "Regular";
                            timeLocationList.Add(new TimeLocationModel { Day = header, Time = WebUtility.HtmlDecode(cells[0].InnerText).Trim(), Location = WebUtility.HtmlDecode(cells[1].InnerText).Trim() });
                        }
                    }
                }
            }

            // Fallback for simple text/list schedules (like Sandy or Wizard) if no tables were processed.
            if (!timeLocationList.Any())
            {
                var scheduleTextNodes = contentNodes.Where(n => n.Name == "p" || n.Name == "ul");
                string fullScheduleText = string.Join(" ", scheduleTextNodes.Select(n => WebUtility.HtmlDecode(n.InnerText).Trim()));

                if (!string.IsNullOrWhiteSpace(fullScheduleText))
                {
                    timeLocationList.Add(new TimeLocationModel { Day = "All Seasons - Regular", Time = "All Day", Location = fullScheduleText });
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
            HtmlNodeCollection eventTitleNodes = heartEventsHeader.SelectNodes("following-sibling::h3 | following-sibling::h4");
            if (eventTitleNodes == null) return heartEventList;

            foreach (var titleNode in eventTitleNodes)
            {
                // 3. Para cada título, buscar la información en los elementos que le siguen
                var heartEvent = new HeartEventsModel
                {
                    Title = titleNode.SelectSingleNode(".//span[@class='mw-headline']")?.InnerText.Trim() ?? titleNode.InnerText.Trim()
                };

                // El siguiente párrafo <p> suele contener la imagen del corazón
                var imageParagraph = titleNode.SelectSingleNode("following-sibling::p[1] | following-sibling::div[1]/p[1]");
                heartEvent.HeartsImage = ToAbsoluteUrl(imageParagraph?.SelectSingleNode(".//img")?.GetAttributeValue("src", string.Empty));

                // El siguiente párrafo <p> después de la imagen suele ser la descripción
                var detailsParagraph = titleNode.SelectSingleNode("following-sibling::p[2] | following-sibling::div[1]/p[2] | following-sibling::div[contains(@class, 'mw-collapsible')]/p | following-sibling::div[1]");
                if (detailsParagraph != null)
                {
                    // Limpiamos el texto para quitar las notas de edición como "[1]"
                    var supNodes = detailsParagraph.SelectNodes(".//sup");
                    if (supNodes != null)
                    {
                        foreach (var n in supNodes) n.Remove();
                    }
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
                    string name = nameNode.InnerText.Trim();
                    // Lógica mejorada: Decodifica el HTML, resta el nombre y limpia todos los caracteres extra.
                    string fullText = WebUtility.HtmlDecode(pNode.InnerText);
                    string description = fullText.Replace(name, "").Trim(' ', '(', ')', '\u00A0');

                    FamilyModel person = new FamilyModel
                    {
                        Image = ToAbsoluteUrl(pNode.SelectSingleNode(".//img")?.GetAttributeValue("src", string.Empty)),
                        Name = name,
                        Description = description
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
            foreach (var giftSpan in bestGiftsCell.SelectNodes(".//span") ?? Enumerable.Empty<HtmlNode>())
            {
                var nameNode = giftSpan.SelectSingleNode(".//a");
                var imageNode = giftSpan.SelectSingleNode(".//img");

                if (nameNode != null && imageNode != null)
                {
                    bestGifts.Add(new BestGiftsModel
                    {
                        Name = nameNode.InnerText.Trim(),
                        Image = ToAbsoluteUrl(imageNode.GetAttributeValue("src", string.Empty))
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
                    Image = ToAbsoluteUrl(cells[0].SelectSingleNode(".//img")?.GetAttributeValue("src", string.Empty)),
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
                        Image = ToAbsoluteUrl(span.SelectSingleNode(".//img")?.GetAttributeValue("src", string.Empty)),
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
                            Image = ToAbsoluteUrl(tempDoc.DocumentNode.SelectSingleNode("//img")?.GetAttributeValue("src", string.Empty))
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
                        portraitImagesList.Add(ToAbsoluteUrl(imageSrc));
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
                    Image = ToAbsoluteUrl(item.SelectSingleNode(".//a[@class='image']/img")?.GetAttributeValue("src", string.Empty)),
                    Description = WebUtility.HtmlDecode(item.SelectSingleNode(".//div[@class='gallerytext']")?.InnerText.Trim() ?? string.Empty)
                });
            }
            return spousePatios;
        }

        private string GetDescription(HtmlDocument htmlDocument)
        {
            var descriptionNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='infoboxborder']/following-sibling::table[1]//td[contains(@class, 'quotetext')]");
            return descriptionNode != null ? WebUtility.HtmlDecode(descriptionNode.InnerText).Trim() : string.Empty;
        }

        private string GetTimelineImage(HtmlDocument htmlDocument, Dictionary<string, string> keys)
        {
            // Note: The timeline key might not be in WikiKeys, so we use a hardcoded fallback.
            string timelineKey = keys.GetValueOrDefault("Timeline", "Timeline");
            var timelineHeader = htmlDocument.DocumentNode.SelectSingleNode($"//span[@id='{timelineKey}']/ancestor::h2");
            return timelineHeader?.SelectSingleNode("following-sibling::div[1]//img")?.GetAttributeValue("src", string.Empty) ?? string.Empty;
        }

        private async Task<(string outside, string map, string interior)> GetHouseImages(string addressUrlPart, Dictionary<string, string> keys, string languageCode)
        {
            if (string.IsNullOrEmpty(addressUrlPart))
            {
                return (string.Empty, string.Empty, string.Empty);
            }

            string languagePrefix = languageCode.ToUpper() == "EN" ? "" : $"{languageCode.ToLower()}.";
            string baseUrl = languageCode.ToUpper() == "ES" ? "es.stardewvalleywiki.com" : GeneralConstants.BASE_URL;
            string addressUrl = $"https://{baseUrl}{addressUrlPart}";

            // Navigate and wait for the map selector.
            HtmlDocument htmlDocumentAddressDetail = await GetDocument(addressUrl, "div.mapcontainer img");

            // Robust selectors for house images
            var outsideHouseImageNode = htmlDocumentAddressDetail.DocumentNode.SelectSingleNode("//div[@id='infoboxborder']//a[@class='image']/img");
            var mapHouseImageNode = htmlDocumentAddressDetail.DocumentNode.SelectSingleNode("//div[contains(@class, 'mapcontainer')]/img[contains(@alt, 'Map.png')]");
            
            // Selector for the interior image
            var interiorHeader = htmlDocumentAddressDetail.DocumentNode.SelectSingleNode($"//span[@id='Interior' or @id='{keys.GetValueOrDefault("Interior", "Interior")}']/ancestor::h2");
            var interiorImageNode = interiorHeader?.SelectSingleNode("following-sibling::p/a/img");

            var outsideImageSrc = outsideHouseImageNode?.GetAttributeValue("src", string.Empty) ?? string.Empty;
            // The map image uses 'data-src' for lazy loading
            var mapImageSrc = mapHouseImageNode?.GetAttributeValue("src", string.Empty) ?? string.Empty;
            var interiorImageSrc = interiorImageNode?.GetAttributeValue("src", string.Empty) ?? string.Empty;

            return (ToAbsoluteUrl(outsideImageSrc), ToAbsoluteUrl(mapImageSrc), ToAbsoluteUrl(interiorImageSrc));
        }
    }
}