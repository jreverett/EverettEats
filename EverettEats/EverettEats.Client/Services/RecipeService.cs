using EverettEats.Client.Models;
using System.Net.Http.Json;

namespace EverettEats.Client.Services;

public class RecipeService : IRecipeService
{
	private readonly HttpClient _httpClient;
	private List<Recipe>? _recipes;

	public RecipeService(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}
	public async Task<List<Recipe>> GetAllRecipesAsync()
	{
		await EnsureRecipesLoadedAsync();
		return _recipes!.OrderByDescending(r => r.DateAdded).ToList();
	}

	public async Task<Recipe?> GetRecipeByIdAsync(int id)
	{
		await EnsureRecipesLoadedAsync();
		return _recipes!.FirstOrDefault(r => r.Id == id);
	}

	public async Task<Recipe?> GetRecipeBySlugAsync(string slug)
	{
		await EnsureRecipesLoadedAsync();
		return _recipes!.FirstOrDefault(r => r.Slug == slug);
	}

	public async Task<List<Recipe>> GetRecipesByCategoryAsync(RecipeCategory category)
	{
		await EnsureRecipesLoadedAsync();
		return _recipes!.Where(r => r.Category == category).ToList();
	}
	
	public async Task<List<Recipe>> SearchRecipesAsync(string searchTerm)
	{
		if (string.IsNullOrWhiteSpace(searchTerm))
			return await GetAllRecipesAsync();

		await EnsureRecipesLoadedAsync();
		var results = _recipes!.Where(r =>
			r.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
			r.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
			r.Tags.Any(tag => tag.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
			r.Ingredients.Any(ingredient => ingredient.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
		).ToList();

		return results;
	}

	private async Task EnsureRecipesLoadedAsync()
	{
		if (_recipes == null)
		{
			try
			{
				_recipes = await _httpClient.GetFromJsonAsync<List<Recipe>>("data/recipes.json") ?? [];
			}
			catch (Exception ex)
			{
				// Fallback to empty list if JSON loading fails
				Console.WriteLine($"Failed to load recipes: {ex.Message}");
				_recipes = [];
			}
		}
	}

}
