namespace Scraping.Models
{
    public class StoryQuestModel
    {
        public string Name { get; set; } = string.Empty;
        public string QuestText { get; set; } = string.Empty;
        public string GivenBy { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string Reward { get; set; } = string.Empty;
    }
}