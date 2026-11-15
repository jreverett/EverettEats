using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EverettEats.Models
{
    public class Recipe
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public List<string> GalleryImages { get; set; } = [];

        [Required]
        public string PrepTime { get; set; } = string.Empty;

        [Required]
        public string CookTime { get; set; } = string.Empty;

        public string CoolTime { get; set; } = string.Empty;

        public string TotalTime { get; set; } = string.Empty;

        public string Servings { get; set; } = string.Empty;

        public List<string> Ingredients { get; set; } = [];

        public List<string> Equipment { get; set; } = [];

        public List<string> Instructions { get; set; } = [];

        public List<string> Tips { get; set; } = [];

        public DateTime DateAdded { get; set; } = DateTime.Now;

        public RecipeCategory Category { get; set; } = RecipeCategory.Desserts;

        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Easy;

        public List<string> Tags { get; set; } = [];

        // SEO-friendly URL slug
        public string Slug { get; set; } = string.Empty;

        // The handwritten heading font (Kalam) has no Vietnamese glyphs, so titles
        // containing them switch wholesale to a fallback font rather than mixing
        // two typefaces mid-word.
        [JsonIgnore]
        public bool TitleNeedsFallbackFont => ContainsVietnamese(Title);

        public static bool ContainsVietnamese(string text) =>
            text.Any(c =>
                c is '\u0110' or '\u0111' // Đ đ
                or '\u01A0' or '\u01A1' // Ơ ơ
                or '\u01AF' or '\u01B0' // Ư ư
                or (>= '\u1EA0' and <= '\u1EF9') // precomposed letters with tone marks
                or (>= '\u0300' and <= '\u0323')); // combining tone marks
    }
}
