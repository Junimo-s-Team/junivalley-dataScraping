namespace Scraping.Models
{
    public class AnimalPageModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<PetModel> Pets { get; set; } = new List<PetModel>();
        public HorseModel? Horse { get; set; }
        public List<FarmAnimalModel> CoopAnimals { get; set; } = new List<FarmAnimalModel>();
        public List<FarmAnimalModel> BarnAnimals { get; set; } = new List<FarmAnimalModel>();
        public List<OtherAnimalModel> OtherAnimals { get; set; } = new List<OtherAnimalModel>();
    }

    public class PetModel
    {
        public string Type { get; set; } = string.Empty;
        public List<string> Images { get; set; } = new List<string>();
        public List<PetGiftModel> Gifts { get; set; } = new List<PetGiftModel>();
    }

    public class PetGiftModel
    {
        public string ItemName { get; set; } = string.Empty;
        public string Chance { get; set; } = string.Empty;
    }

    public class HorseModel
    {
        public string Image { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class FarmAnimalModel
    {
        public string Name { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Cost { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public string Produce { get; set; } = string.Empty;
        public string SellPrice { get; set; } = string.Empty;
    }

    public class OtherAnimalModel
    {
        public string Name { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}