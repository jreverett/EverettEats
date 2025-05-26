using EverettEats.Client;

namespace EverettEats.Services;

public class RecipeCard
{
	public string Title { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public string ImageUrl { get; set; } = string.Empty;
	public string TotalTime { get; set; } = string.Empty;
	public RecipeCategory Category { get; set; }
	public DifficultyLevel Difficulty { get; set; }
	public List<string> Tags { get; set; } = [];
	public string RecipeUrl { get; set; } = string.Empty;
	public DateTime DateAdded { get; set; }
}
