using EverettEats.Models;
using Xunit;

public class RecipeTitleFontTests
{
    [Theory]
    [InlineData("Bánh Bò Nướng (Vietnamese Pandan Honeycomb Cake)", true)]
    [InlineData("Phở Bò", true)]
    [InlineData("Bún đậu", true)]
    [InlineData("Guinness Cake", false)]
    [InlineData("Crème Brûlée", false)]
    [InlineData("", false)]
    public void TitleNeedsFallbackFont_DetectsVietnameseCharacters(string title, bool expected)
    {
        var recipe = new Recipe { Title = title };

        Assert.Equal(expected, recipe.TitleNeedsFallbackFont);
    }
}
