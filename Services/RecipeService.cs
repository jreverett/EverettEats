using EverettEats.Models;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;

namespace EverettEats.Services
{
	public class RecipeService : IRecipeService
	{
		private readonly HttpClient _httpClient;
		private readonly IMemoryCache _cache;
		private const string RecipesCacheKey = "recipes_cache_v1";
		private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

		public RecipeService(HttpClient httpClient, IMemoryCache cache)
		{
			_httpClient = httpClient;
			_cache = cache;
		}

		public async Task<List<Recipe>> GetAllRecipesAsync()
		{
			var recipes = await GetRecipesFromCacheAsync();
			return recipes.OrderByDescending(r => r.DateAdded).ToList();
		}

		public async Task<Recipe?> GetRecipeByIdAsync(int id)
		{
			var recipes = await GetRecipesFromCacheAsync();
			return recipes.FirstOrDefault(r => r.Id == id);
		}

		public async Task<Recipe?> GetRecipeBySlugAsync(string slug)
		{
			var recipes = await GetRecipesFromCacheAsync();
			return recipes.FirstOrDefault(r => r.Slug == slug);
		}

		public async Task<List<Recipe>> GetRecipesByCategoryAsync(RecipeCategory category)
		{
			var recipes = await GetRecipesFromCacheAsync();
			return recipes.Where(r => r.Category == category).ToList();
		}

		public async Task<List<Recipe>> SearchRecipesAsync(string searchTerm)
		{
			var recipes = await GetRecipesFromCacheAsync();
			if (string.IsNullOrWhiteSpace(searchTerm))
				return recipes.OrderByDescending(r => r.DateAdded).ToList();

			return recipes.Where(r =>
				r.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
				r.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
				r.Tags.Any(tag => tag.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
				r.Ingredients.Any(ingredient => ingredient.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
			).OrderByDescending(r => r.DateAdded).ToList();
		}

		public async Task<(List<Recipe> Recipes, int TotalCount)> GetPaginatedRecipesAsync(int pageNumber, int pageSize, string? searchTerm = null, RecipeCategory? category = null)
		{
			var recipes = await GetRecipesFromCacheAsync();
			IEnumerable<Recipe> filtered = recipes;
			if (!string.IsNullOrWhiteSpace(searchTerm))
			{
				filtered = filtered.Where(r =>
					r.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
					r.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
					r.Tags.Any(tag => tag.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
					r.Ingredients.Any(ingredient => ingredient.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
				);
			}
			if (category.HasValue)
			{
				filtered = filtered.Where(r => r.Category == category.Value);
			}
			var total = filtered.Count();
			var page = filtered
				.OrderByDescending(r => r.DateAdded)
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToList();
			return (page, total);
		}

		private async Task<List<Recipe>> GetRecipesFromCacheAsync()
		{
			if (!_cache.TryGetValue(RecipesCacheKey, out List<Recipe>? recipes) || recipes == null)
			{
				try
				{
					recipes = await _httpClient.GetFromJsonAsync<List<Recipe>>("data/recipes.json") ?? new List<Recipe>();
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Failed to load recipes: {ex.Message}");
					recipes = new List<Recipe>();
				}
				_cache.Set(RecipesCacheKey, recipes, CacheDuration);
			}
			return recipes;
		}
	}
}
