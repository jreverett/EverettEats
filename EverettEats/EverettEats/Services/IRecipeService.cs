
using EverettEats.Models;

namespace EverettEats.Services
{
	public interface IRecipeService
	{
		Task<List<Recipe>> GetAllRecipesAsync();
		Task<Recipe?> GetRecipeByIdAsync(int id);
		Task<Recipe?> GetRecipeBySlugAsync(string slug);
		Task<List<Recipe>> GetRecipesByCategoryAsync(RecipeCategory category);
		Task<List<Recipe>> SearchRecipesAsync(string searchTerm);
	}
}
