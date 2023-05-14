using HtmlAgilityPack;
using Scraping.Models;
using Newtonsoft.Json;
using System;
using System.IO;


string url = "https://stardewvalleywiki.com";
List<VillagerModel> villagersData = new List<VillagerModel>();
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

foreach (var villager in villagers)
{
    villagerId ++;
    VillagerModel villagerData = GetVillagerPrimaryData($"{url}/{villager.Name}", villagerId, villager.HasFamily, villager.HasClinicVisit);
    villagersData.Add(villagerData);
}


HtmlDocument GetDocument(string url)
{
    HtmlWeb web = new HtmlWeb();
    HtmlDocument doc = web.Load(url);
    return doc;
}


//Villager Detail
VillagerModel GetVillagerPrimaryData(string url, int id, bool hasFamily, bool hasClinicVisit)
{
    HtmlDocument htmlDocument = GetDocument(url);
    string birthdayStation = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"infoboxdetail\"]/span/a").InnerText;
    string birthdayNumber = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"infoboxdetail\"]").InnerText;

    VillagerModel villager = new VillagerModel
    {
        Id = villagerId,
        Name = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"infoboxheader\"]").InnerText,
        Description = htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[5]/div/table[1]/tbody/tr[1]/td[2]").InnerText.Replace("&#8220;", ""),
        Birthday = $"{birthdayStation.Trim()} {birthdayNumber.Trim()}",
        Address = htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[6]/td[2]/a").InnerText,
        Family = hasFamily ? GetFamilyPeopleForVillager(htmlDocument) : new List<FamilyModel>(),
        LivesIn = htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[5]/td[2]/a").InnerText,
        Marriage = hasFamily
                   ? htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[8]/td[2]").InnerText
                   : htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[7]/td[2]").InnerText,
        BestGifts = GetBestGiftsForVillager(htmlDocument, hasFamily, hasClinicVisit),
        //TimeLocationSpring = GetTimeLocation(htmlDocument, idTableSeason: 2),
        LovedGifts = GetGiftsForVillager(htmlDocument, idTable: hasFamily ? 13 : 12),
        LikedGifts = GetGiftsForVillager(htmlDocument, idTable: hasFamily ? 15 : 14),
        NeutralGifts = GetGiftsForVillager(htmlDocument, idTable: hasFamily ? 17 : 16),
        DislikeGifts = GetGiftsForVillager(htmlDocument, idTable: hasFamily ? 19 : 18),
        HateGifts = GetGiftsForVillager(htmlDocument, idTable: hasFamily ? 21 : 20),
        Movies = GetMoviesForVillager(htmlDocument, idTable: hasFamily ? 22 : 21),
        Concessions = GetConcessionsForVillager(htmlDocument, idTable: hasFamily ? 22 : 21),
        //HeartEvents = GetHeartEventsForVillager(htmlDocument),
        //Portraits = GetPortraitsForVillager(htmlDocument)

    };

    //Check clinic visit
    if (hasClinicVisit)
    {
        //Check family
        if (hasFamily)
        {
            //tr value 9
            villager.ClinicVisit = htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[9]/td[2]").InnerText.Replace("&#160;", "");
        }
        //not family
        else
        {
            //tr value 8
            villager.ClinicVisit = htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[8]/td[2]").InnerText.Replace("&#160;", "");
        };
    }
    //not clinic visit
    else
    {
        //null
        villager.ClinicVisit = string.Empty;
    }
    string json = JsonConvert.SerializeObject(villagersData);
    string filePath = "Villagers-test.json";
    string fullPath = Path.GetFullPath(filePath);
    File.WriteAllText(filePath, json);

    Console.WriteLine("Ruta completa del archivo: " + fullPath);

    return villager;
}

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
                Image = pNode.SelectSingleNode("img")?.GetAttributeValue("src", "") ?? string.Empty,
                Name = pNode.SelectSingleNode("a")?.InnerText ?? string.Empty,
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
                Name = childNodes[i].SelectSingleNode(nameXPath)?.InnerText ?? "",
                Image = childNodes[i].SelectSingleNode(imageXPath)?.GetAttributeValue("src", "") ?? ""
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
                    Id = i-1,
                    Image = tdNodes[0].SelectSingleNode("div/div/a/img")?.GetAttributeValue("src", "") ?? string.Empty,
                    Name = tdNodes[1].InnerText ?? string.Empty,
                    Description = tdNodes[2].InnerText ?? string.Empty,
                    Source = tdNodes[3].InnerText ?? string.Empty
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
                  Name = node.SelectSingleNode($"{node.XPath}/a")?.InnerText ?? string.Empty,
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

    if (movieNodes != null)
    {
        foreach (HtmlNode table in movieNodes)
        {
            HtmlNodeCollection rows = table.SelectNodes("tr");
            for (int i = 1; i <= rows.Count; i++)
            {
                MoviesConcessionsModel movies = new MoviesConcessionsModel();

                // Obtiene valores fuera de <p>
                movies.Type = table.SelectSingleNode($"tr[{i}]/th")?.InnerText ?? string.Empty;
                movies.Title = table.SelectSingleNode($"tr[{i}]/td/text()")?.InnerText ?? string.Empty;
                movies.Image = table.SelectSingleNode($"tr[{i}]/td/img")?.GetAttributeValue("src", string.Empty) ?? string.Empty;
                movieList.Add(movies);

                // Si tengo valores <p> dentro de los td
                var tdNode = table.SelectSingleNode($"tr[{i}]/td");
                if (tdNode != null && tdNode.Descendants("p").Any())
                {
                    int pIndex = 1; // Índice del elemento <p>
                    foreach (HtmlNode pNode in tdNode.Descendants("p"))
                    {
                        //Comprobar si <p> contiene [valor]
                        string pValue = pNode.GetAttributeValue("value", string.Empty);
                        if (!string.IsNullOrEmpty(pValue))
                        {
                            // Utiliza pIndex para el valor de <p>
                            MoviesConcessionsModel moviesWithPValue = new MoviesConcessionsModel();
                            moviesWithPValue.Type = movies.Type;
                            moviesWithPValue.Title = table.SelectSingleNode($"tr[{i}]/td/p[{pIndex}]/text()")?.InnerText ?? string.Empty;
                            moviesWithPValue.Image = table.SelectSingleNode($"tr[{i}]/td/p[{pIndex}]/img")?.GetAttributeValue("src", string.Empty) ?? string.Empty;
                            movieList.Add(moviesWithPValue);
                        }
                        else
                        {
                            // Si no tiene el atributo 'value', entonces es <p>
                            MoviesConcessionsModel moviesWithoutPValue = new MoviesConcessionsModel();
                            moviesWithoutPValue.Type = movies.Type;
                            moviesWithoutPValue.Title = table.SelectSingleNode($"tr[{i}]/td/p[{pIndex}]/text()")?.InnerText ?? string.Empty;
                            moviesWithoutPValue.Image = table.SelectSingleNode($"tr[{i}]/td/p[{pIndex}]/img")?.GetAttributeValue("src", string.Empty) ?? string.Empty;
                            movieList.Add(moviesWithoutPValue);
                            pIndex++; // Incrementa el índice del elemento <p> para el siguiente bucle
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
    List<MoviesConcessionsModel> concessionList = new List<MoviesConcessionsModel>(); ///html/body/div[3]/div[3]/div[5]/div/table[22]/tbody/tr/td[3]/table/tbody //alex
    HtmlNodeCollection concessionNodes = htmlDocument.DocumentNode.SelectNodes($"/html/body/div[3]/div[3]/div[5]/div/table[{idTable}]/tbody/tr/td[3]/table/tbody");

    if (concessionNodes != null) {
        foreach (HtmlNode table in concessionNodes)
        {
            HtmlNodeCollection rows = table.SelectNodes("tr");
            for (int i = 1; i <= rows.Count; i++)
            {
                var tdNode = table.SelectSingleNode($"tr[{i}]/td");

                MoviesConcessionsModel concessions = new MoviesConcessionsModel();
                // Obtiene valores que no tienen posicion
                concessions.Type = table.SelectSingleNode($"tr[{i}]/th")?.InnerText ?? string.Empty;
                concessions.Title = table.SelectSingleNode($"tr[{i}]/td/text()")?.InnerText ?? string.Empty;
                concessions.Image = table.SelectSingleNode($"tr[{i}]/td/img")?.GetAttributeValue("src", string.Empty) ?? string.Empty;
                concessionList.Add(concessions);

                // Si tengo valores <i> dentro de los td (en este caso siempre vendra como texto unicamente)
                if (tdNode != null)
                {
                    if (tdNode.Descendants("i").Any())
                    {
                        // guardar texto vacio
                        MoviesConcessionsModel concessionI = new MoviesConcessionsModel();
                        concessionI.Title = table.SelectSingleNode($"tr[{i}]/td/i")?.InnerText ?? string.Empty;
                        concessionList.Add(concessionI);
                    }

                    foreach (HtmlNode childNode in tdNode.ChildNodes)
                    {
                        if (childNode.Name == "#text" || childNode.Name == "img")
                        {
                            MoviesConcessionsModel concessionContent = new MoviesConcessionsModel();
                            concessionContent.Title = childNode.InnerText ?? string.Empty;
                            concessionContent.Image = childNode?.GetAttributeValue("src", string.Empty) ?? string.Empty;
                            concessionList.Add(concessionContent);
                        }
                        else
                        {

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