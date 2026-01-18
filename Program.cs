using EverettEats.Services;

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

// Register HttpClient factory
builder.Services.AddHttpClient();

// Register HttpClient for accessing static files (recipes.json, etc.)
builder.Services.AddScoped<HttpClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var navigationManager = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    var client = httpClientFactory.CreateClient();
    client.BaseAddress = new Uri(navigationManager.BaseUri);
    return client;
});

builder.Services.AddScoped<IRecipeService, RecipeService>();

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
