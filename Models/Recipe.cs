using System.ComponentModel.DataAnnotations;

namespace EverettEats.Models
{
    public class Recipe
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        public string PrepTime { get; set; } = string.Empty;

        [Required]
        public string CookTime { get; set; } = string.Empty;

        public string TotalTime { get; set; } = string.Empty;

        public string Servings { get; set; } = string.Empty;

        public List<string> Ingredients { get; set; } = [];

        public List<string> Instructions { get; set; } = [];

        public List<string> Tips { get; set; } = [];

        public DateTime DateAdded { get; set; } = DateTime.Now;

        public RecipeCategory Category { get; set; } = RecipeCategory.Desserts;

        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Easy;

        public List<string> Tags { get; set; } = [];

        // SEO-friendly URL slug
        public string Slug { get; set; } = string.Empty;
    }
}
