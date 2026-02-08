using Bunit;
using EverettEats.Components.Pages;
using EverettEats.Models;
using EverettEats.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class HomeComponentTests : TestContext
{
    [Fact]
    public void RecipeCards_DisplayTotalTime_NotCookTime()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new Recipe
            {
                Id = 1,
                Title = "Test Recipe",
                Slug = "test-recipe",
                CookTime = "30 minutes",
                TotalTime = "1 hour 15 minutes",
                Description = "A test recipe",
                Difficulty = DifficultyLevel.Easy,
            },
        };

        var mockService = new Mock<IRecipeService>();
        mockService.Setup(s => s.GetAllRecipesAsync()).ReturnsAsync(recipes);

        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<Home>();

        // Assert - should contain TotalTime, not CookTime
        Assert.Contains("1 hour 15 minutes", cut.Markup);
        Assert.DoesNotContain("30 minutes", cut.Markup);
    }

    [Fact]
    public void RecipeCards_DisplayTotalTime_ForMultipleRecipes()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new Recipe
            {
                Id = 1,
                Title = "Quick Recipe",
                Slug = "quick-recipe",
                CookTime = "10 minutes",
                TotalTime = "25 minutes",
                Description = "Fast to make",
                Difficulty = DifficultyLevel.Easy,
            },
            new Recipe
            {
                Id = 2,
                Title = "Slow Recipe",
                Slug = "slow-recipe",
                CookTime = "2 hours",
                TotalTime = "4 hours",
                Description = "Takes a while",
                Difficulty = DifficultyLevel.Hard,
            },
        };

        var mockService = new Mock<IRecipeService>();
        mockService.Setup(s => s.GetAllRecipesAsync()).ReturnsAsync(recipes);

        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<Home>();

        // Assert - should contain TotalTime values
        Assert.Contains("25 minutes", cut.Markup);
        Assert.Contains("4 hours", cut.Markup);

        // Assert - should not contain CookTime values
        Assert.DoesNotContain("10 minutes", cut.Markup);
        Assert.DoesNotContain("2 hours", cut.Markup);
    }
}
