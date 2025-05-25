using EverettEats.Enums;
using EverettEats.Models;

namespace EverettEats.Services;

public class RecipeService : IRecipeService
{
	private readonly List<Recipe> _recipes = GetSampleRecipes();

	public Task<List<Recipe>> GetAllRecipesAsync()
	{
		return Task.FromResult(_recipes.OrderByDescending(r => r.DateAdded).ToList());
	}

	public Task<Recipe?> GetRecipeByIdAsync(int id)
	{
		return Task.FromResult(_recipes.FirstOrDefault(r => r.Id == id));
	}

	public Task<Recipe?> GetRecipeBySlugAsync(string slug)
	{
		return Task.FromResult(_recipes.FirstOrDefault(r => r.Slug == slug));
	}

	public Task<List<Recipe>> GetRecipesByCategoryAsync(RecipeCategory category)
	{
		return Task.FromResult(_recipes.Where(r => r.Category == category).ToList());
	}

	public Task<List<Recipe>> SearchRecipesAsync(string searchTerm)
	{
		if (string.IsNullOrWhiteSpace(searchTerm))
			return GetAllRecipesAsync();

		var results = _recipes.Where(r =>
			r.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
			r.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
			r.Tags.Any(tag => tag.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
		).ToList();

		return Task.FromResult(results);
	}	private static List<Recipe> GetSampleRecipes()
	{
		return
		[
			new Recipe
			{
				Id = 1,
				Title = "Ultimate Fudgy Brownies",
				Description = "Super rich, moist brownies with the perfect fudgy texture. These brownies are dense, chocolatey, and absolutely irresistible.",
				ImageUrl = "https://images.unsplash.com/photo-1606313564200-e75d5e30476c?w=400&h=300&fit=crop",
				PrepTime = "15 minutes",
				CookTime = "25-30 minutes",
				TotalTime = "1 hour 15 minutes (includes cooling)",
				Servings = "16 brownies",
				Category = RecipeCategory.Desserts,
				Difficulty = DifficultyLevel.Easy,
				DateAdded = new DateTime(2025, 5, 17),
				Tags = ["chocolate", "fudgy", "easy", "crowd-pleaser"],
				Ingredients =
				[
					"225g unsalted butter (1 cup)",
					"2 tablespoons vegetable oil",
					"300g caster sugar (1½ cups)",
					"2 whole eggs + 1 egg yolk, room temperature",
					"1½ teaspoons vanilla extract",
					"65g unsweetened cocoa powder (¾ cup)",
					"65g plain flour (½ cup)",
					"¼ teaspoon salt",
					"125g dark chocolate, chopped (¾ cup)",
					"Optional: 60ml golden syrup (¼ cup) OR 2 tablespoons corn syrup"
				],
				Instructions =
				[
					"Preheat oven to 180°C (160°C fan/gas mark 4). Line a 20cm square tin with baking parchment, leaving an overhang.",
					"MELT: Microwave butter in 30-second intervals until melted. Immediately stir in oil and sugar. Let cool for 5 minutes.",
					"MIX WET: Beat in 2 eggs + 1 yolk one at a time, then vanilla (and golden syrup if using). Mix until just combined.",
					"ADD DRY: Sift cocoa powder, flour, and salt directly over wet ingredients. Fold gently until just combined - DO NOT OVERMIX. Fold in chopped chocolate.",
					"Optional but recommended: Refrigerate batter for 15 minutes.",
					"BAKE: Pour into tin. Bake for 25-30 minutes. The top should be set but still glossy. A skewer should come out with moist crumbs.",
					"COOL: Let cool in tin for 30 minutes, then refrigerate for at least 1 hour before cutting."
				],
				Tips =
				[
					"For extra fudgy brownies, don't overmix the batter once you add the flour",
					"The slight sinking in the middle is normal and indicates fudgy texture",
					"Use a sharp knife and clean between cuts for neat squares",
					"Store in an airtight container at room temperature for up to 5 days",
					"Add a piece of bread to the container to maintain moisture if they start to dry out",
					"For 24cm x 14cm tin: Use same recipe but bake 2-3 minutes longer as brownies will be slightly thicker",
					"Freezer method for quick cooling: Cool 10-15 mins at room temp, then 15 mins in freezer"
				]
			}
		];
	}
}