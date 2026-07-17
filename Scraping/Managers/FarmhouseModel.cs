namespace Scraping.Models
{
    public class FarmhouseModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string MapImage { get; set; } = string.Empty;
        public List<FarmhouseDetailModel> InitialInteriors { get; set; } = new List<FarmhouseDetailModel>();
        public List<FarmhouseUpgradeModel> Upgrades { get; set; } = new List<FarmhouseUpgradeModel>();
        public List<FarmhouseUpgradeModel> Renovations { get; set; } = new List<FarmhouseUpgradeModel>();
        public List<FarmhouseDetailModel> SpouseRooms { get; set; } = new List<FarmhouseDetailModel>();
    }

    public class FarmhouseDetailModel
    {
        public string Name { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
    }

    public class FarmhouseUpgradeModel
    {
        public string Name { get; set; } = string.Empty;
        public string ExteriorImage { get; set; } = string.Empty;
        public string InteriorImage { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public List<UpgradeMaterialModel> Materials { get; set; } = new List<UpgradeMaterialModel>();
        public string Description { get; set; } = string.Empty;
    }

    public class UpgradeMaterialModel
    {
        public string Name { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}