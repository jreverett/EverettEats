using EverettEats.Client.Models;

namespace EverettEats.Client.Services;

public interface IRecipeService
{
	Task<List<Recipe>> GetAllRecipesAsync();
	Task<Recipe?> GetRecipeByIdAsync(int id);
	Task<Recipe?> GetRecipeBySlugAsync(string slug);
	Task<List<Recipe>> GetRecipesByCategoryAsync(RecipeCategory category);
	Task<List<Recipe>> SearchRecipesAsync(string searchTerm);
}
