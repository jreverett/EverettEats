using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using EverettEats.Client.Services;
using EverettEats.Client.Components;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<EverettEats.Client.Components.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add HTTP client for API calls
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register services
builder.Services.AddScoped<EverettEats.Client.Services.IRecipeService>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var jsRuntime = sp.GetRequiredService<Microsoft.JSInterop.IJSRuntime>();
    return new EverettEats.Client.Services.RecipeService(httpClient, jsRuntime);
});

await builder.Build().RunAsync();