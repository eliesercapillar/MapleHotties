using Microsoft.EntityFrameworkCore;
using MapleTinder.Shared.Data;
using MapleTinder.Shared.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.Services.AddDbContext<MapleTinderDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<CharacterScraper>();

var app = builder.Build();

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    _ = CharacterScraper.DisposeBrowserAsync();
});

app.MapPost("/scrape/character/{name}", async (string name, CharacterScraper scraper) =>
{
    var character = await scraper.ScrapeCharacterAsync(name);
    await scraper.SaveCharacterToDatabase(character);
    return Results.Ok(character);
});

app.MapPost("/scrape/all", async (int maxPages, CharacterScraper scraper) =>
{
    var characters = await scraper.ScrapeAllCharactersAsync(maxPages);
    await scraper.SaveCharactersToDatabase(characters);
    return Results.Accepted();
});


app.Run("http://0.0.0.0:8080");
