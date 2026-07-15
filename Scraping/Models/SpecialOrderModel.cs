namespace Scraping.Models
{
    public class SpecialOrderModel
    {
        public string Name { get; set; } = string.Empty;
        public string QuestText { get; set; } = string.Empty;
        public string GivenBy { get; set; } = string.Empty;
        public string Prerequisites { get; set; } = string.Empty;
        public string TimeLimit { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string Reward { get; set; } = string.Empty;
        public string IsRepeatable { get; set; } = string.Empty;
    }
}