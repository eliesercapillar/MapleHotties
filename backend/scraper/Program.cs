using Microsoft.EntityFrameworkCore;
using MapleTinder.Shared.Data;
using MapleTinder.Shared.Models.Entities;
using scraper.Services;
using scraper.Interfaces;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.Services.AddDbContext<MapleTinderDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<CharacterScraper>();
builder.Services.AddSingleton<CharacterScraperService>();
builder.Services.AddSingleton<IScrapeJobTracker, ScrapeJobTracker>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<CharacterScraperService>());

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

app.MapPost("/scrape/all", async (int maxPages, string jobId, 
                           CharacterScraper scraper, IScrapeJobTracker tracker) =>
{
    if (string.IsNullOrWhiteSpace(jobId)) return Results.BadRequest(new { error = "Missing jobId in query string" });

    tracker.StartJob(jobId);
    try
    {
        var characters = await scraper.ScrapeAllCharactersAsync(maxPages);
        await scraper.SaveCharactersToDatabase(characters);
        tracker.CompleteJob(jobId);
        return Results.Accepted($"/scrape/status/{jobId}", new
        {
            message = "Scrape complete",
            jobId
        });
    }
    catch (Exception ex)
    {
        tracker.FailJob(jobId);
        return Results.Problem($"Scraping failed: {ex.Message}");
    }
});

app.MapGet("/scrape/status/{jobId}", (string jobId, IScrapeJobTracker tracker) =>
{
    var status = tracker.GetStatus(jobId);
    return status == null
        ? Results.NotFound(new { message = $"No job found for {jobId}" })
        : Results.Ok(new { jobId, status });
});


app.Run("http://0.0.0.0:8080");
