using EverettEats;
using EverettEats.Services;
using Microsoft.Extensions.Caching.Memory;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();
builder.Services.AddRazorPages();

// Enable response compression
builder.Services.AddResponseCompression(options =>
{
	options.EnableForHttps = true;
	options.MimeTypes = [
		"text/plain",
		"text/css",
		"application/javascript",
		"text/html",
		"application/xml",
		"text/xml",
		"application/json",
		"text/json",
		"image/svg+xml"
	];
});

// Register memory cache
builder.Services.AddMemoryCache();

// Register HttpClient with proper configuration for RecipeService
builder.Services.AddHttpClient<IRecipeService, RecipeService>(client =>
{
	// Use relative base address for static files
	client.BaseAddress = new Uri("/", UriKind.RelativeOrAbsolute);
});

// Register HttpClient for accessing static files (recipes.json, etc.)
builder.Services.AddScoped<HttpClient>(sp =>
{
	var navigationManager = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
	return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
});

builder.Services.AddScoped<IRecipeService>(sp =>
{
	var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
	var httpClient = httpClientFactory.CreateClient();
	httpClient.BaseAddress = new Uri("/", UriKind.RelativeOrAbsolute);
	var cache = sp.GetRequiredService<IMemoryCache>();
	return new RecipeService(httpClient, cache);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseWebAssemblyDebugging();
}
else
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

// Add browser caching headers for static assets
app.UseStaticFiles(new StaticFileOptions
{
	OnPrepareResponse = ctx =>
	{
		var headers = ctx.Context.Response.Headers;
		// Cache images, CSS, JS for 30 days
		if (ctx.File.Name.EndsWith(".js") || ctx.File.Name.EndsWith(".css") || 
		    ctx.File.Name.EndsWith(".png") || ctx.File.Name.EndsWith(".jpg") || 
		    ctx.File.Name.EndsWith(".jpeg") || ctx.File.Name.EndsWith(".svg") || 
		    ctx.File.Name.EndsWith(".ico"))
		{
			headers["Cache-Control"] = "public,max-age=2592000"; // 30 days
		}
	}
});

// Enable response compression
app.UseResponseCompression();

app.MapRazorComponents<EverettEats.App>()
	.AddInteractiveServerRenderMode();


app.Run();