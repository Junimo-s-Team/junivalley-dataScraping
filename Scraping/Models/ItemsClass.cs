namespace Scraping.Models
{
    internal class ItemsClass
    {
        public int Id { get; set; }

        public string Image { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Source { get; set; } = string.Empty;

        public List<IngredientsModel> Ingredients { get; set; } = new List<IngredientsModel>();

    }
}
