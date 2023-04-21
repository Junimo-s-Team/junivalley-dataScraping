using HtmlAgilityPack;
using Scraping.Models;

string url = "https://stardewvalleywiki.com/Leah";

VillagerModel villager = GetVillagerPrimaryData(url);

HtmlDocument GetDocument(string url)
{
    HtmlWeb web = new HtmlWeb();
    HtmlDocument doc = web.Load(url);
    return doc;
}


VillagerModel GetVillagerPrimaryData(string url)
{
    HtmlDocument htmlDocument = GetDocument(url);
    string birthdayStation = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"infoboxdetail\"]/span/a").InnerText;
    string birthdayNumber = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"infoboxdetail\"]").InnerText;

    VillagerModel villager = new VillagerModel
    {
        Name = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"infoboxheader\"]").InnerText,
        Birthday = $"{birthdayStation.Trim()} {birthdayNumber.Trim()}",
        Address = htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[6]/td[2]/a").InnerText,
        LivesIn = htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[5]/td[2]/a").InnerText,
        ClinicVisit = htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[8]/td[2]").InnerText,
        Marriage = htmlDocument.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[7]/td[2]").InnerText,
        BestGifts = GetBestGiftsForVillager(htmlDocument),
        LovedGifts = GetLovedGiftsForVillager(htmlDocument)
    };
    return villager;
}


List<BestGiftsModel> GetBestGiftsForVillager(HtmlDocument htmlDocument)
{
    List<BestGiftsModel> bestGifts = new List<BestGiftsModel>();
    HtmlNodeCollection bestGiftsNodes = htmlDocument.DocumentNode.SelectNodes("/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[9]/td[2]");

    foreach (var bestGiftsNode in bestGiftsNodes)
    {
        var childNodes = bestGiftsNode.SelectNodes("span");

        for (int i = 0; i < childNodes.Count; i++)
        {
            BestGiftsModel item = new BestGiftsModel
            {
                Name = childNodes[i].SelectSingleNode($"/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[9]/td[2]/span[{i + 1}]/a").InnerText ?? "",
                Image = childNodes[i].SelectSingleNode($"/html/body/div[3]/div[3]/div[5]/div/div[1]/table/tbody/tr[9]/td[2]/span[{i + 1}]/img").GetAttributeValue("src", "") ?? ""
            };
            bestGifts.Add(item);
        }
    }
    return bestGifts;
}



List<ItemsClass> GetLovedGiftsForVillager(HtmlDocument htmlDocument)
{
    List<ItemsClass> lovedGifts = new List<ItemsClass>();
    HtmlNodeCollection bestGiftsNodes = htmlDocument.DocumentNode.SelectNodes("/html/body/div[3]/div[3]/div[5]/div/table[12]/tbody");

    foreach (var bestGiftsNode in bestGiftsNodes)
    {
        var trNodes = bestGiftsNode.SelectNodes("tr");

        for (int i = 1; i < trNodes.Count; i++)
        {
            var tdNodes = trNodes[i].SelectNodes("td");

            if (tdNodes != null && tdNodes.Count >= 4)
            {
                ItemsClass item = new ItemsClass
                {
                    Image = tdNodes[0].SelectSingleNode("div/div/a/img")?.GetAttributeValue("src", "") ?? "",
                    Name = tdNodes[1].InnerText,
                    Description = tdNodes[2].InnerText,
                    Source = tdNodes[3].InnerText,
                    Ingredients = GetIngredientsModel(tdNodes[4])
                };
                lovedGifts.Add(item);
            }
        }
    }
    return lovedGifts;
}

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

