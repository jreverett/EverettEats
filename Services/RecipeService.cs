using System.Text.Json;
using EverettEats.Models;
using Microsoft.Extensions.Caching.Memory;

namespace EverettEats.Services
{
    public class RecipeService : IRecipeService
    {
        private const string _recipesCacheKey = "recipes_cache_v1";
        private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IWebHostEnvironment _env;

        public RecipeService(HttpClient httpClient, IMemoryCache cache, IWebHostEnvironment env)
        {
            _httpClient = httpClient;
            _cache = cache;
            _env = env;
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
            {
                return recipes.OrderByDescending(r => r.DateAdded).ToList();
            }

            return recipes.Where(r =>
                r.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                r.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                r.Tags.Any(tag => tag.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                r.Ingredients.Any(ingredient => ingredient.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(r => r.DateAdded).ToList();
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
                    r.Ingredients.Any(ingredient => ingredient.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
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
            if (!_cache.TryGetValue(_recipesCacheKey, out List<Recipe>? recipes) || recipes == null)
            {
                try
                {
                    // Try loading from file system first (more reliable during prerendering)
                    var filePath = Path.Combine(_env.WebRootPath, "data", "recipes.json");
                    Console.WriteLine($"Loading recipes from file: {filePath}");

                    if (File.Exists(filePath))
                    {
                        var json = await File.ReadAllTextAsync(filePath);
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                        };
                        recipes = JsonSerializer.Deserialize<List<Recipe>>(json, options) ?? new List<Recipe>();
                        Console.WriteLine($"Successfully loaded {recipes.Count} recipes from file system");
                        if (recipes.Count > 0)
                        {
                            Console.WriteLine($"First recipe: Id={recipes[0].Id}, Title={recipes[0].Title}, Slug={recipes[0].Slug}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"File not found, trying HTTP: {_httpClient.BaseAddress}data/recipes.json");
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                        };
                        recipes = await _httpClient.GetFromJsonAsync<List<Recipe>>("data/recipes.json", options) ?? new List<Recipe>();
                        Console.WriteLine($"Successfully loaded {recipes.Count} recipes via HTTP");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load recipes: {ex.GetType().Name}: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                    }

                    recipes = new List<Recipe>();
                }

                _cache.Set(_recipesCacheKey, recipes, _cacheDuration);
            }

            return recipes;
        }
    }
}
