namespace Scraping.Models
{
    internal class VillagerModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Birthday { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string LivesIn { get; set; } = string.Empty;

        public string ClinicVisit { get; set; } = string.Empty;

        public List<BestGiftsModel> BestGifts { get; set; } = new List<BestGiftsModel>();

        public List<ItemsClass> LovedGifts { get; set; } = new List<ItemsClass>();

        public List<ItemsClass> LikedGifts { get; set; } = new List<ItemsClass>();

        public List<ItemsClass> DislikeGifts { get; set; } = new List<ItemsClass>();

        public string Marriage { get; set; } = string.Empty;

    }
}
