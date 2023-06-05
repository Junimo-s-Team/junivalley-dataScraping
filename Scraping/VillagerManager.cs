using System;
using System.Xml;
using System.Net;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Scraping.Models;

namespace Scraping
{
	public class VillagerManager
	{
        //Villager Detail
        internal VillagerModel GetVillagerPrimaryData(string url, VillagerModel villager, int villagerId)
        {
            HtmlDocument htmlDocument = GetDocument(url);
            string birthdayNumber = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"infoboxdetail\"]").InnerText.Trim();

            VillagerModel newVillager = new VillagerModel
            {
                Id = villagerId,
                Name = villager.Name.Trim(),
                Description = WebUtility.HtmlDecode(htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[5]/div/table[1]/tbody/tr[1]/td[2]").InnerText.Trim()),
                Birthday = WebUtility.HtmlDecode(birthdayNumber.Trim()),
                Address = htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[6]/td[2]/a").InnerText.Trim(),
                HasFamily = villager.HasFamily,
                Family = villager.HasFamily ? GetFamilyPeopleForVillager(htmlDocument) : new List<FamilyModel>(),
                LivesIn = htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[5]/td[2]/a").InnerText.Trim(),
                HasClinicVisit = villager.HasClinicVisit,
                Marriage = villager.HasFamily
                           ? htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[8]/td[2]").InnerText.Trim()
                           : htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[7]/td[2]").InnerText.Trim(),
                BestGifts = GetBestGiftsForVillager(htmlDocument, villager.HasFamily, villager.HasClinicVisit),
                //TimeLocation = GetTimeLocation(htmlDocument, idTableSeason: 2),
                LovedGifts = GetGiftsForVillager(htmlDocument, idTable: villager.HasFamily ? 13 : 12),
                LikedGifts = GetGiftsForVillager(htmlDocument, idTable: villager.HasFamily ? 15 : 14),
                NeutralGifts = GetGiftsForVillager(htmlDocument, idTable: villager.HasFamily ? 17 : 16),
                DislikeGifts = GetGiftsForVillager(htmlDocument, idTable: villager.HasFamily ? 19 : 18),
                HateGifts = GetGiftsForVillager(htmlDocument, idTable: villager.HasFamily ? 21 : 20),
                Movies = GetMoviesForVillager(htmlDocument, idTable: villager.HasFamily ? 22 : 21),
                Concessions = GetConcessionsForVillager(htmlDocument, idTable: villager.HasFamily ? 22 : 21),
                ClinicVisit = WebUtility.HtmlDecode(GetClinicVisitFamily(villager.HasFamily, villager.HasClinicVisit, htmlDocument))
                //HeartEvents = GetHeartEventsForVillager(htmlDocument),
                //Portraits = GetPortraitsForVillager(htmlDocument)

            };
           
            return newVillager;
        }

        //Save villager data in json
        internal void ConvertToJson(VillagerModel villagerData)
        {
            string json = JsonConvert.SerializeObject(villagerData);
            string fileName = $"{villagerData.Name}.json";
            string path = @"/Users/estherhuecas/Documents/Stardew Valley/Scrapping/EN";
            string fullFileName = Path.Combine(path, fileName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            File.WriteAllText(fullFileName, json);
        }


        //Load document data about url
        HtmlDocument GetDocument(string url)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            return doc;
        }


        //Check clinic visit
        String GetClinicVisitFamily(bool hasFamily, bool hasClinicVisit, HtmlDocument htmlDocument)
        {
            if (hasClinicVisit)
            {
                //Check family
                if (hasFamily)
                {
                    //tr value 9
                    return htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[9]/td[2]").InnerText.Trim();
                }
                //not family
                else
                {
                    //tr value 8
                    return htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[8]/td[2]").InnerText.Trim();
                };
            }
            //not clinic visit
            else
            {
                //null
                return string.Empty;
            }
        }


        //Time location depends on seasons
        List<TimeLocationModel> GetTimeLocation(HtmlDocument htmlDocument, int idTableSeason)
        {
            List<TimeLocationModel> timeLocationList = new List<TimeLocationModel>();
            HtmlNodeCollection seasonInfoNodes = htmlDocument.DocumentNode.SelectNodes($"/html/body/div[3]/div[3]/div[5]/div/table[{idTableSeason}]/tbody/tr[2]/td");

            foreach (HtmlNode content in seasonInfoNodes)
            {
                if (content.Descendants("p").Any() || content.Descendants("table").Any())
                {
                    foreach (HtmlNode pNode in content.Descendants("p"))
                    {
                        var tableNode = content.Descendants("table").FirstOrDefault();
                        if (tableNode != null)
                        {
                            foreach (var tableContentNode in tableNode.Descendants("tr"))
                            {
                                if (tableContentNode.Descendants("td").Any())
                                {
                                    var tdNodes = tableContentNode.Descendants("td").ToList();
                                    if (tdNodes.Count >= 2)
                                    {
                                        TimeLocationModel timeLocationInfo = new TimeLocationModel()
                                        {
                                            Day = pNode?.InnerText ?? string.Empty,
                                            Time = tdNodes[0]?.InnerText ?? string.Empty,
                                            Location = tdNodes[1]?.InnerText ?? string.Empty
                                        };
                                        timeLocationList.Add(timeLocationInfo);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return timeLocationList;
        }


        //GET Heart Events
        List<HeartEventsModel> GetHeartEventsForVillager(HtmlDocument htmlDocument)
        {
            List<HeartEventsModel> heartEventList = new List<HeartEventsModel>();
            HtmlNodeCollection heartNodes = htmlDocument.DocumentNode.SelectNodes("/html/body/div[3]/div[3]/div[5]/div");

            foreach (HtmlNode heartNode in heartNodes)
            {
                //Title
                if (heartNode.SelectNodes("h3").Any())
                {
                    for (int i = 6; i <= 14; i++)
                    {
                        HeartEventsModel heartEvents = new HeartEventsModel
                        {
                            Title = heartNode.Descendants("h3").ElementAtOrDefault(i - 1)?.SelectSingleNode("span")?.InnerText ?? string.Empty,
                        };
                        heartEventList.Add(heartEvents);
                    }
                }
                //Heart image
                //if (heartNode.SelectNodes("p").Descendants("img").Any())
                //{
                //    HtmlNodeCollection pNodes = heartNode.SelectNodes("p");
                //    for (int i = 9; i <= 27; i += 2)
                //    {
                //        HeartEventsModel heartEvents = new HeartEventsModel
                //        {
                //            HeartsImage = pNodes.ElementAtOrDefault(i - 1)?.Descendants("img").FirstOrDefault()?.GetAttributeValue("src", string.Empty) ?? string.Empty
                //        };
                //        heartEventList.Add(heartEvents);
                //    }
                //}

                //if (heartNode.SelectNodes("p").Any())
                //{
                //    HtmlNodeCollection pNodes = heartNode.SelectNodes("p");
                //    for (int i = 10; i <= pNodes.Count; i += 2)
                //    {
                //        HeartEventsModel heartEvents = new HeartEventsModel
                //        {
                //            Place = pNodes.ElementAtOrDefault(i - 1)?.InnerText ?? string.Empty,
                //        };
                //        heartEventList.Add(heartEvents);
                //    }
                //}
            }
            return heartEventList;
        }


        //GET Family
        List<FamilyModel> GetFamilyPeopleForVillager(HtmlDocument htmlDocument)
        {
            List<FamilyModel> familyList = new List<FamilyModel>();
            HtmlNodeCollection familyNodes = htmlDocument.DocumentNode.SelectNodes("/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[7]/td[2]");

            foreach (HtmlNode tdNode in familyNodes)
            {
                foreach (HtmlNode pNode in tdNode.Descendants("p"))
                {
                    FamilyModel person = new FamilyModel
                    {
                        Image = pNode.SelectSingleNode("img")?.GetAttributeValue("src", string.Empty) ?? string.Empty,
                        Name = pNode.SelectSingleNode("a")?.InnerText.Trim() ?? string.Empty,
                        Description = pNode.InnerText.Substring(pNode.InnerText.IndexOf("(") + 1).Replace("(", string.Empty).Replace(")", string.Empty) ?? string.Empty
                    };
                    familyList.Add(person);
                }
            }

            return familyList;
        }


        //GET Best Gifts
        List<BestGiftsModel> GetBestGiftsForVillager(HtmlDocument htmlDocument, bool hasFamily, bool hasClinicVisit)
        {
            List<BestGiftsModel> bestGifts = new List<BestGiftsModel>();
            int trIndex = 10; // Índice predeterminado para el último tr

            if (!hasFamily && !hasClinicVisit)
            {
                trIndex = 8;
            }
            else if (!hasFamily || !hasClinicVisit)
            {
                trIndex = 9;
            }

            string fullXPath = $"/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[{trIndex}]/td[2]";
            HtmlNodeCollection bestGiftsNodes = htmlDocument.DocumentNode.SelectNodes(fullXPath);

            foreach (var bestGiftsNode in bestGiftsNodes)
            {
                var childNodes = bestGiftsNode.SelectNodes("span");

                for (int i = 0; i < childNodes.Count; i++)
                {
                    string nameXPath = $"/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[{trIndex}]/td[2]/span[{i + 1}]/a";
                    string imageXPath = $"/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[{trIndex}]/td[2]/span[{i + 1}]/img";

                    BestGiftsModel item = new BestGiftsModel
                    {
                        Name = childNodes[i].SelectSingleNode(nameXPath)?.InnerText.Trim() ?? string.Empty,
                        Image = childNodes[i].SelectSingleNode(imageXPath)?.GetAttributeValue("src", string.Empty) ?? string.Empty
                    };
                    bestGifts.Add(item);
                }
            }
            return bestGifts;
        }


        //GET Gifts
        List<ItemsClass> GetGiftsForVillager(HtmlDocument htmlDocument, int idTable)
        {
            List<ItemsClass> giftList = new List<ItemsClass>();
            HtmlNodeCollection bestGiftsNodes = htmlDocument.DocumentNode.SelectNodes($"/html/body/div[3]/div[3]/div[5]/div/table[{idTable}]/tbody");

            foreach (var bestGiftsNode in bestGiftsNodes)
            {
                var trNodes = bestGiftsNode.SelectNodes("tr");

                for (int i = 0; i < trNodes.Count; i++)
                {
                    var tdNodes = trNodes[i].SelectNodes("td");

                    if (tdNodes != null && tdNodes.Count >= 4)
                    {
                        ItemsClass item = new ItemsClass
                        {
                            Id = i - 1,
                            Image = tdNodes[0].SelectSingleNode("div/div/a/img")?.GetAttributeValue("src", string.Empty) ?? string.Empty,
                            Name = tdNodes[1].InnerText.Trim() ?? string.Empty,
                            Description = tdNodes[2].InnerText.Trim() ?? string.Empty,
                            Source = tdNodes[3].InnerText.Trim() ?? string.Empty
                        };

                        if (tdNodes.Count >= 5 && tdNodes[4].ChildNodes.Any())
                        {
                            item.Ingredients = GetIngredientsModel(tdNodes[4]);
                        }

                        giftList.Add(item);
                    }
                }
            }
            return giftList;
        }


        //GET Ingredients
        List<IngredientsModel> GetIngredientsModel(HtmlNode tdNodes)
        {
            List<IngredientsModel> ingredientList = new List<IngredientsModel>();

            foreach (var node in tdNodes.ChildNodes)
            {
                if (node.ChildNodes.Any())
                {
                    if (node.InnerText.Contains("fruit"))
                    {
                        IngredientsModel ingredientsBasicModel = new IngredientsModel
                        {
                            Name = "Any Fruit",
                            Quantity = "1"
                        };
                        continue;
                    }

                    IngredientsModel ingredientsModel = new IngredientsModel
                    {
                        Image = node.SelectSingleNode($"{node.XPath}/img")?.GetAttributeValue("src", string.Empty) ?? string.Empty,
                        Name = node.SelectSingleNode($"{node.XPath}/a")?.InnerText.Trim() ?? string.Empty,
                        Quantity = node?.InnerText.Substring(node.InnerText.Length - 3).Replace("(", string.Empty).Replace(")", string.Empty) ?? string.Empty
                    };
                    ingredientList.Add(ingredientsModel);
                }
            }
            return ingredientList;
        }


        //GET Movies
        List<MoviesConcessionsModel> GetMoviesForVillager(HtmlDocument htmlDocument, int idTable)
        {
            List<MoviesConcessionsModel> movieList = new List<MoviesConcessionsModel>();
            HtmlNodeCollection movieNodes = htmlDocument.DocumentNode.SelectNodes($"/html/body/div[3]/div[3]/div[5]/div/table[{idTable}]/tbody/tr/td[1]/table/tbody");

            var thType = "";

            if (movieNodes != null)
            {
                foreach (HtmlNode table in movieNodes)
                {
                    HtmlNodeCollection rows = table.SelectNodes("tr");
                    for (int i = 1; i <= rows.Count; i++)
                    {
                        MoviesConcessionsModel movies = new MoviesConcessionsModel();

                        // Obtiene valores fuera de <p>
                        HtmlNode thNode = table.SelectSingleNode($"tr[{i}]/th");
                        if (thNode != null)
                        {
                            thType = thNode.InnerText.Trim();
                        }

                        var tdNode = table.SelectSingleNode($"tr[{i}]/td");
                        if (tdNode != null)
                        {
                            // Obtiene valores de <td> sin <p>
                            HtmlNode tdTextNode = tdNode.SelectSingleNode("./text()");
                            if (tdTextNode != null)
                            {
                                movies.Type = thType;
                                movies.Title = tdTextNode.InnerText.Trim();
                                movies.Image = tdNode.SelectSingleNode("img")?.GetAttributeValue("src", string.Empty) ?? string.Empty;
                                movieList.Add(movies);
                            }

                            // Obtiene valores de <p> dentro de <td>
                            HtmlNodeCollection pNodes = tdNode.SelectNodes("p");
                            if (pNodes != null)
                            {
                                foreach (HtmlNode pNode in pNodes)
                                {
                                    MoviesConcessionsModel moviesWithPValue = new MoviesConcessionsModel();
                                    moviesWithPValue.Type = thType;
                                    moviesWithPValue.Title = pNode.SelectSingleNode("text()")?.InnerText.Trim() ?? string.Empty;
                                    moviesWithPValue.Image = pNode.SelectSingleNode("img")?.GetAttributeValue("src", string.Empty) ?? string.Empty;
                                    movieList.Add(moviesWithPValue);
                                }
                            }
                        }
                    }
                }
            }
            return movieList;
        }



        //GET Concessions
        List<MoviesConcessionsModel> GetConcessionsForVillager(HtmlDocument htmlDocument, int idTable)
        {
            List<MoviesConcessionsModel> concessionList = new List<MoviesConcessionsModel>();
            HtmlNodeCollection concessionNodes = htmlDocument.DocumentNode.SelectNodes($"/html/body/div[3]/div[3]/div[5]/div/table[{idTable}]/tbody/tr/td[3]/table/tbody");

            if (concessionNodes != null)
            {
                foreach (HtmlNode table in concessionNodes)
                {
                    HtmlNodeCollection rows = table.SelectNodes("tr");

                    string thType = string.Empty; // Variable para almacenar el valor de Type

                    for (int i = 1; i <= rows.Count; i++)
                    {
                        MoviesConcessionsModel concessions = new MoviesConcessionsModel();

                        HtmlNode thNode = table.SelectSingleNode($"tr[{i}]/th");
                        if (thNode != null)
                        {
                            thType = thNode.InnerText.Trim();
                        }

                        HtmlNode tdNode = table.SelectSingleNode($"tr[{i}]/td");

                        if (tdNode != null)
                        {
                            if (tdNode.Descendants("i").Any())
                            {
                                MoviesConcessionsModel concessionI = new MoviesConcessionsModel();
                                concessionI.Type = thType;
                                concessionI.Title = tdNode.SelectSingleNode("i")?.InnerText ?? string.Empty;
                                concessionI.Image = tdNode.SelectSingleNode("img")?.GetAttributeValue("src", string.Empty) ?? string.Empty;
                                concessionList.Add(concessionI);
                            }
                            else
                            {
                                // Obtiene valores que no tienen posición
                                MoviesConcessionsModel concessionContent = new MoviesConcessionsModel();
                                concessionContent.Type = thType;

                                HtmlNodeCollection textNodes = tdNode.SelectNodes("text()");
                                if (textNodes != null)
                                {
                                    foreach (HtmlNode textNode in textNodes)
                                    {
                                        string text = textNode.InnerText.Trim();
                                        if (!string.IsNullOrEmpty(text))
                                        {
                                            MoviesConcessionsModel concession = new MoviesConcessionsModel();
                                            concession.Type = thType;
                                            concession.Title = text;
                                            concession.Image = tdNode.SelectSingleNode("img")?.GetAttributeValue("src", string.Empty) ?? string.Empty;
                                            concessionList.Add(concession);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return concessionList;
        }


        //GET Portraits Images
        List<string> GetPortraitsForVillager(HtmlDocument htmlDocument)
        {
            List<string> portraitImagesList = new List<string>();
            HtmlNodeCollection liNodes = htmlDocument.DocumentNode.SelectNodes("/html/body/div[3]/div[3]/div[5]/div/ul[7]/li");

            foreach (HtmlNode liNode in liNodes)
            {
                foreach (HtmlNode div in liNode.Descendants("div"))
                {
                    var imgNode = div.SelectSingleNode("./div[1]/div/a/img");
                    if (imgNode != null)
                    {
                        var images = imgNode.GetAttributeValue("src", string.Empty) ?? string.Empty;
                        portraitImagesList.Add(images);
                    }
                }
            }
            return portraitImagesList;
        }
    }
}

