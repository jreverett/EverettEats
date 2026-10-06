using System.Text;
using Bunit;
using EverettEats.Components.Pages;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Xunit;

public class ImpossibleSourdoughTests
{
    [Fact]
    public async Task Open_StreamsContentLargerThanOneSignalRMessage()
    {
        using var context = new TestContext();
        var text = new string('a', 80 * 1024) + " 🥖";
        using var data = new MemoryStream(Encoding.UTF8.GetBytes($"<p>{text}</p>"));
        var reference = new Mock<IJSStreamReference>();
        reference.Setup(r => r.OpenReadStreamAsync(128 * 1024, It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);
        var js = RegisterJs(context, reference.Object);
        var page = context.RenderComponent<ImpossibleSourdough>();

        page.Find("input").Input("test phrase");
        await page.Find(".draft-open-button").ClickAsync(new MouseEventArgs());

        Assert.Equal(text, page.Find(".draft-output p").TextContent);
        Assert.Empty(page.FindAll(".draft-gate"));
        var invocation = Assert.Single(js.Invocations);
        Assert.Equal("kitchenOpen", invocation.Arguments[0]);
        Assert.True(Assert.IsType<CancellationToken>(invocation.Arguments[1]).CanBeCanceled);
        Assert.Equal(new object[] { "/data/impossible-sourdough.json", "test phrase" }, invocation.Arguments[2]);
        reference.Verify(r => r.OpenReadStreamAsync(128 * 1024, It.Is<CancellationToken>(t => t.CanBeCanceled)), Times.Once);
        reference.Verify(r => r.DisposeAsync(), Times.Once);
        Assert.False(data.CanRead);
    }

    [Fact]
    public async Task Open_RejectsOversizedStreamAndReenablesRetry()
    {
        using var context = new TestContext();
        var reference = new Mock<IJSStreamReference>();
        reference.Setup(r => r.OpenReadStreamAsync(128 * 1024, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("The stream exceeds the allowed size."));
        RegisterJs(context, reference.Object);
        var page = context.RenderComponent<ImpossibleSourdough>();

        await page.Find(".draft-open-button").ClickAsync(new MouseEventArgs());

        Assert.Empty(page.Find(".draft-output").TextContent.Trim());
        Assert.NotEmpty(page.Find(".draft-error").TextContent);
        Assert.False(page.Find(".draft-open-button").HasAttribute("disabled"));
        reference.Verify(r => r.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task Open_WrongPhraseLeavesDraftClosedAndAllowsRetry()
    {
        using var context = new TestContext();
        var js = new Mock<IJSRuntime>();
        js.Setup(r => r.InvokeAsync<IJSStreamReference>(
            "kitchenOpen", It.IsAny<CancellationToken>(), It.IsAny<object?[]>()))
            .ThrowsAsync(new JSException("Decryption failed."));
        context.Services.AddSingleton(js.Object);
        var page = context.RenderComponent<ImpossibleSourdough>();

        await page.Find(".draft-open-button").ClickAsync(new MouseEventArgs());

        Assert.Empty(page.Find(".draft-output").TextContent.Trim());
        Assert.NotEmpty(page.FindAll(".draft-gate"));
        Assert.False(page.Find(".draft-open-button").HasAttribute("disabled"));
    }

    private static Mock<IJSRuntime> RegisterJs(TestContext context, IJSStreamReference reference)
    {
        var js = new Mock<IJSRuntime>();
        js.Setup(r => r.InvokeAsync<IJSStreamReference>(
            "kitchenOpen", It.IsAny<CancellationToken>(), It.IsAny<object?[]>()))
            .ReturnsAsync(reference);
        context.Services.AddSingleton(js.Object);
        return js;
    }
}
