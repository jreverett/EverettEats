## INSTRUCTIONS

You are a recipe formatter for the EverettEats recipe website. Convert the recipe provided below into a JSON entry that matches the exact structure used in the recipes.json file.

**JSON Structure Requirements:**
- Return ONLY valid JSON matching the structure shown in the example
- Use numeric values for category: Desserts=0, MainDishes=1, Appetizers=2, Sides=3, Beverages=4, Breakfast=5, Snacks=6
- Use numeric values for difficulty: Easy=0, Medium=1, Hard=2
- Calculate the next available ID (look at the highest ID in the current recipes.json and add 1)
- Create a URL-friendly slug by converting the title to lowercase and replacing spaces with hyphens (e.g., "Creamy Tomato Pasta" → "creamy-tomato-pasta")
- Use ISO 8601 format for dateAdded: "YYYY-MM-DDTHH:MM:SS" (use today's date)
- Format the JSON with tabs for indentation to match the existing file
- Ensure all strings are properly escaped
- Arrays should be on multiple lines for readability

**Example JSON Structure:**
```json
{
	"id": 2,
	"title": "Recipe Title",
	"description": "Brief 1-2 sentence description",
	"imageUrl": "https://images.unsplash.com/photo-xxxxx?w=400&h=300&fit=crop",
	"prepTime": "15 minutes",
	"cookTime": "25-30 minutes",
	"totalTime": "1 hour 15 minutes (includes cooling)",
	"servings": "4 servings",
	"category": 1,
	"difficulty": 0,
	"dateAdded": "2026-01-20T00:00:00",
	"slug": "recipe-title",
	"tags": ["tag1", "tag2", "tag3"],
	"ingredients": [
		"Ingredient 1 with measurements",
		"Ingredient 2 with measurements"
	],
	"instructions": [
		"Step 1 instruction",
		"Step 2 instruction"
	],
	"tips": [
		"Helpful tip 1",
		"Helpful tip 2"
	]
}
```

---

## RECIPE DETAILS TO CONVERT

### Basic Information

**Title:**
[Your recipe name here]

**Description:**
[Brief 1-2 sentence description that will appear on the recipe card]

**Image URL:**
[Unsplash URL or other image URL - format: https://images.unsplash.com/photo-xxxxx?w=400&h=300&fit=crop]
[Leave blank or write "none" if you don't have an image]

**Prep Time:**
[e.g., "15 minutes" or "30 mins"]

**Cook Time:**
[e.g., "25-30 minutes" or "1 hour"]

**Total Time:**
[Include any cooling/resting/marinating time, e.g., "1 hour 15 minutes (includes cooling)"]

**Servings:**
[e.g., "4 servings" or "12 cookies" or "6-8 people"]

**Category:**
[Choose ONE: Desserts, MainDishes, Appetizers, Sides, Beverages, Breakfast, Snacks]

**Difficulty:**
[Choose ONE: Easy, Medium, Hard]

**Tags:**
[Comma-separated list of searchable tags, e.g., "chocolate, fudgy, easy, dessert, party"]

---

### Ingredients

List each ingredient with measurements, one per line:

-
-
-
-
-
-
-
-

---

### Instructions

Write step-by-step instructions. Each step should be clear and concise:

1.
2.
3.
4.
5.
6.
7.
8.

---

### Tips (Optional)

List any helpful tips, tricks, storage instructions, or variations:

-
-
-
-

---

## OUTPUT

Provide ONLY the JSON object (properly formatted with tabs) that I can copy directly into my recipes.json file. The JSON should be ready to add to the array - do not include the surrounding array brackets, just the object.
