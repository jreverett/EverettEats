using EverettEats.Models;
using EverettEats.Services;
using Microsoft.AspNetCore.Components;

namespace EverettEats.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject] public IRecipeService RecipeService { get; set; } = default!;

    protected string searchTerm = string.Empty;
    protected int pageNumber = 1;
    protected int _pageSize = 10;
    protected int pageSize
    {
        get => _pageSize;
        set
        {
            if (_pageSize != value)
            {
                _pageSize = value;
                pageNumber = 1;
                _ = LoadRecipesAsync();
            }
        }
    }
    protected int totalPages = 1;
    protected int totalCount = 0;
    protected List<int> pageSizeOptions = new() { 10, 20, 50 };
    protected List<Recipe> recipes = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadRecipesAsync();
    }

    protected async Task LoadRecipesAsync()
    {
        var result = await RecipeService.GetPaginatedRecipesAsync(pageNumber, pageSize, searchTerm);
        recipes = result.Recipes;
        totalCount = result.TotalCount;
        totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        if (pageNumber > totalPages && totalPages > 0)
        {
            pageNumber = totalPages;
            await LoadRecipesAsync();
        }
        StateHasChanged();
    }

    protected async Task PreviousPage()
    {
        if (pageNumber > 1)
        {
            pageNumber--;
            await LoadRecipesAsync();
        }
    }

    protected async Task NextPage()
    {
        if (pageNumber < totalPages)
        {
            pageNumber++;
            await LoadRecipesAsync();
        }
    }

    protected async Task OnSearchChanged(ChangeEventArgs e)
    {
        searchTerm = e.Value?.ToString() ?? string.Empty;
        pageNumber = 1;
        await LoadRecipesAsync();
    }

    protected async Task OnSearchInput(ChangeEventArgs e)
    {
        searchTerm = e.Value?.ToString() ?? string.Empty;
        pageNumber = 1;
        await LoadRecipesAsync();
    }

    protected async Task ClearSearch()
    {
        searchTerm = string.Empty;
        pageNumber = 1;
        await LoadRecipesAsync();
    }

    protected string GetDifficultyText(DifficultyLevel difficulty)
    {
        return difficulty switch
        {
            DifficultyLevel.Easy => "Easy",
            DifficultyLevel.Medium => "Medium",
            DifficultyLevel.Hard => "Hard",
            _ => "Unknown"
        };
    }

    protected string GetCategoryText(RecipeCategory category)
    {
        return category switch
        {
            RecipeCategory.Appetizers => "Appetizers",
            RecipeCategory.MainDishes => "Main Dishes",
            RecipeCategory.Desserts => "Desserts",
            RecipeCategory.Beverages => "Beverages",
            RecipeCategory.Breakfast => "Breakfast",
            RecipeCategory.Sides => "Sides",
            RecipeCategory.Snacks => "Snacks",
            _ => "Other"
        };
    }
}
