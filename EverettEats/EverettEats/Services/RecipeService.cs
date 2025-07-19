using EverettEats.Models;
using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace EverettEats.Services
{
	public class RecipeService : IRecipeService
	{
		private readonly HttpClient _httpClient;
		private readonly IJSRuntime _jsRuntime;
		private List<Recipe>? _recipes;
		private const string RecipesCacheKey = "recipes_cache_v1";

		public RecipeService(HttpClient httpClient, IJSRuntime jsRuntime)
		{
			_httpClient = httpClient;
			_jsRuntime = jsRuntime;
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
			if (_recipes != null) return;

			// Try to load from localStorage first
			var cached = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", RecipesCacheKey);
			if (!string.IsNullOrEmpty(cached))
			{
				try
				{
					_recipes = System.Text.Json.JsonSerializer.Deserialize<List<Recipe>>(cached);
				}
				catch { _recipes = null; }
			}

			if (_recipes == null || _recipes.Count == 0)
			{
				try
				{
					_recipes = await _httpClient.GetFromJsonAsync<List<Recipe>>("data/recipes.json") ?? new List<Recipe>();
					// Cache in localStorage
					var json = System.Text.Json.JsonSerializer.Serialize(_recipes);
					await _jsRuntime.InvokeVoidAsync("localStorage.setItem", RecipesCacheKey, json);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Failed to load recipes: {ex.Message}");
					_recipes = new List<Recipe>();
				}
			}
		}

	}
}
