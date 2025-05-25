using EverettEats.Enums;

namespace EverettEats.Services;

public interface IRecipeRegistry
{
	Task<List<RecipeCard>> GetAllRecipeCardsAsync();
	Task<RecipeCard?> GetRecipeCardBySlugAsync(string slug);
	Task<List<RecipeCard>> GetRecipeCardsByCategoryAsync(RecipeCategory category);
	Task<List<RecipeCard>> SearchRecipeCardsAsync(string searchTerm);
}