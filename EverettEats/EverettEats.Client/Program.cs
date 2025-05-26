using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using EverettEats.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add HTTP client for API calls
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register services
builder.Services.AddScoped<IRecipeService, RecipeService>();

await builder.Build().RunAsync();