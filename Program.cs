using EverettEats;
using EverettEats.Services;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Enable response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Register memory cache
builder.Services.AddMemoryCache();

// Register HttpClient for accessing static files (recipes.json, etc.)
builder.Services.AddScoped<HttpClient>(sp =>
{
    var navigationManager = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
});

builder.Services.AddScoped<IRecipeService>(sp =>
{
    var navigationManager = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    var httpClient = sp.GetRequiredService<HttpClient>();
    var cache = sp.GetRequiredService<IMemoryCache>();
    return new RecipeService(httpClient, cache);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// IMPORTANT: UseStaticFiles must come before UseRouting
app.UseStaticFiles();

app.UseRouting();

// Enable response compression
app.UseResponseCompression();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
