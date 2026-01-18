using EverettEats.Models;
using EverettEats.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

public class RecipeServiceTests
{
    private readonly Mock<HttpClient> _httpClientMock = new();
    private readonly IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());

    [Fact]
    public async Task GetAllRecipesAsync_ReturnsRecipesOrderedByDate()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "B", DateAdded = DateTime.Now.AddDays(-1) },
            new Recipe { Id = 2, Title = "A", DateAdded = DateTime.Now },
        };
        var service = CreateService(recipes);

        // Act
        var result = await service.GetAllRecipesAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("A", result[0].Title);
        Assert.Equal("B", result[1].Title);
    }

    [Fact]
    public async Task GetRecipeByIdAsync_ReturnsCorrectRecipe()
    {
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "Test" },
        };
        var service = CreateService(recipes);
        var result = await service.GetRecipeByIdAsync(1);
        Assert.NotNull(result);
        Assert.Equal("Test", result!.Title);
    }

    [Fact]
    public async Task GetRecipeBySlugAsync_ReturnsCorrectRecipe()
    {
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "Test Slug", Slug = "test-slug" },
        };
        var service = CreateService(recipes);
        var result = await service.GetRecipeBySlugAsync("test-slug");
        Assert.NotNull(result);
        Assert.Equal("Test Slug", result!.Title);
    }

    [Fact]
    public async Task SearchRecipesAsync_FiltersByTitle()
    {
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "Apple Pie" },
            new Recipe { Id = 2, Title = "Banana Bread" },
        };
        var service = CreateService(recipes);
        var result = await service.SearchRecipesAsync("Apple");
        Assert.Single(result);
        Assert.Equal("Apple Pie", result[0].Title);
    }

    [Fact]
    public async Task GetPaginatedRecipesAsync_ReturnsCorrectPage()
    {
        var recipes = new List<Recipe>();
        for (int i = 1; i <= 25; i++)
        {
            recipes.Add(new Recipe { Id = i, Title = $"Recipe {i}", DateAdded = DateTime.Now.AddDays(-i) });
        }

        var service = CreateService(recipes);
        var (page, total) = await service.GetPaginatedRecipesAsync(2, 10);
        Assert.Equal(10, page.Count);
        Assert.Equal(25, total);
        Assert.Equal("Recipe 11", page[0].Title);
    }

    [Fact]
    public async Task GetRecipeByIdAsync_ReturnsNull_WhenRecipeNotFound()
    {
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "Exists" },
        };
        var service = CreateService(recipes);
        var result = await service.GetRecipeByIdAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetRecipeBySlugAsync_ReturnsNull_WhenSlugNotFound()
    {
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "Exists", Slug = "exists" },
        };
        var service = CreateService(recipes);
        var result = await service.GetRecipeBySlugAsync("nonexistent-slug");
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchRecipesAsync_IsCaseInsensitive()
    {
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "Chocolate Cake" },
        };
        var service = CreateService(recipes);
        var result = await service.SearchRecipesAsync("CHOCOLATE");
        Assert.Single(result);
        Assert.Equal("Chocolate Cake", result[0].Title);
    }

    [Fact]
    public async Task SearchRecipesAsync_FiltersByIngredients()
    {
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "Pasta", Ingredients = new List<string> { "tomato sauce", "pasta" } },
            new Recipe { Id = 2, Title = "Salad", Ingredients = new List<string> { "lettuce", "cucumber" } },
        };
        var service = CreateService(recipes);
        var result = await service.SearchRecipesAsync("tomato");
        Assert.Single(result);
        Assert.Equal("Pasta", result[0].Title);
    }

    [Fact]
    public async Task SearchRecipesAsync_FiltersByDescription()
    {
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "Cake", Description = "Rich chocolate dessert" },
            new Recipe { Id = 2, Title = "Bread", Description = "Simple wheat bread" },
        };
        var service = CreateService(recipes);
        var result = await service.SearchRecipesAsync("chocolate");
        Assert.Single(result);
        Assert.Equal("Cake", result[0].Title);
    }

    [Fact]
    public async Task SearchRecipesAsync_ReturnsEmpty_WhenNoMatches()
    {
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "Apple Pie" },
        };
        var service = CreateService(recipes);
        var result = await service.SearchRecipesAsync("banana");
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchRecipesAsync_ReturnsAll_WhenSearchTermEmpty()
    {
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "Recipe 1" },
            new Recipe { Id = 2, Title = "Recipe 2" },
        };
        var service = CreateService(recipes);
        var result = await service.SearchRecipesAsync(string.Empty);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllRecipesAsync_ReturnsEmpty_WhenNoRecipes()
    {
        var service = CreateService(new List<Recipe>());
        var result = await service.GetAllRecipesAsync();
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPaginatedRecipesAsync_HandlesLastPagePartialResults()
    {
        var recipes = new List<Recipe>();
        for (int i = 1; i <= 23; i++)
        {
            recipes.Add(new Recipe { Id = i, Title = $"Recipe {i}", DateAdded = DateTime.Now.AddDays(-i) });
        }

        var service = CreateService(recipes);
        var (page, total) = await service.GetPaginatedRecipesAsync(3, 10);
        Assert.Equal(3, page.Count); // Only 3 items on last page
        Assert.Equal(23, total);
    }

    [Fact]
    public async Task GetPaginatedRecipesAsync_ReturnsEmpty_WhenPageOutOfBounds()
    {
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "Recipe 1", DateAdded = DateTime.Now },
        };
        var service = CreateService(recipes);
        var (page, total) = await service.GetPaginatedRecipesAsync(10, 10);
        Assert.Empty(page);
        Assert.Equal(1, total);
    }

    private RecipeService CreateService(List<Recipe>? recipes = null)
    {
        var handler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("http://localhost/"),
        };
        var cache = new MemoryCache(new MemoryCacheOptions());
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.WebRootPath).Returns("/tmp");
        var service = new RecipeService(httpClient, cache, mockEnv.Object);
        if (recipes != null)
        {
            cache.Set("recipes_cache_v1", recipes);
        }

        return service;
    }
}
