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
            new Recipe { Id = 1, Title = "Test Slug" },
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
