namespace Scraping.Models
{
    public class VillagerModel
    {
        public int Id { get; set; }

        public string Language { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Birthday { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string OutsideHouseImage { get; set; } = string.Empty;

        public string InsideHouseImage { get; set; } = string.Empty;

        public string MapHouseImage { get; set; } = string.Empty;

        public bool HasFamily { get; set; }

        public List<FamilyModel> Family { get; set; }  = new List<FamilyModel>();

        public string LivesIn { get; set; } = string.Empty;

        public bool HasClinicVisit { get; set; }

        public string ClinicVisit { get; set; } = string.Empty;

        public string TimeLine { get; set; } = string.Empty;

        public List<TimeLocationModel> TimeLocation { get; set; } = new List<TimeLocationModel>();

        public List<BestGiftsModel> BestGifts { get; set; } = new List<BestGiftsModel>();

        public List<ItemsClass> LovedGifts { get; set; } = new List<ItemsClass>();

        public List<ItemsClass> LikedGifts { get; set; } = new List<ItemsClass>();

        public List<ItemsClass> DislikeGifts { get; set; } = new List<ItemsClass>();

        public List<ItemsClass> NeutralGifts { get; set; } = new List<ItemsClass>();

        public List<ItemsClass> HateGifts { get; set; } = new List<ItemsClass>();

        public List<MoviesConcessionsModel> Movies { get; set; } = new List<MoviesConcessionsModel>();

        public List<MoviesConcessionsModel> Concessions { get; set; } = new List<MoviesConcessionsModel>();

        public List<HeartEventsModel> HeartEvents { get; set; } = new List<HeartEventsModel>();

        public bool CanBeMarriage { get; set; }

        public List<SpousePatioModel> SpousePatios { get; set; } = new List<SpousePatioModel>();

        public List<string> Portraits { get; set; } = new List<string>();
    }
}