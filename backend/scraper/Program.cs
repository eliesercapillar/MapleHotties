using Microsoft.EntityFrameworkCore;
using MapleTinder.Shared.Data;
using MapleTinder.Shared.Models.Entities;
using scraper.Services;
using scraper.Interfaces;
using System;
using scraper.Services.Old.Playwright;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Warning;
});
builder.Logging.SetMinimumLevel(LogLevel.Warning);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.Services.AddDbContext<MapleTinderDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .LogTo(_ => { }) // silence everything
);

builder.Services.AddScoped<CharacterJSONScraper>();
//builder.Services.AddScoped<CharacterScraperPlaywright>();
//builder.Services.AddSingleton<CharacterScraperServicePlaywright>();
builder.Services.AddSingleton<IScrapeJobTracker, ScrapeJobTracker>();
//builder.Services.AddHostedService(provider => provider.GetRequiredService<CharacterScraperServicePlaywright>());

var app = builder.Build();

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    _ = CharacterScraperPlaywright.DisposeBrowserAsync();
});

app.MapPost("/scrape/character/{name}", async (string name, CharacterJSONScraper scraper) =>
{
    var character = await scraper.ScrapeCharacterAsync(name);
    await scraper.SaveCharacterToDatabase(character);
    return Results.Ok(character);
});

app.MapPost("/scrape/all", async (int maxPages, int concurrency, string jobId,
                           CharacterJSONScraper scraper, IScrapeJobTracker tracker) =>
{
    if (string.IsNullOrWhiteSpace(jobId)) return Results.BadRequest(new { error = "Missing jobId in query string" });

    tracker.StartJob(jobId);
    try
    {
        var characters = await scraper.ScrapeAllCharactersAsync(maxPages, concurrency);
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
