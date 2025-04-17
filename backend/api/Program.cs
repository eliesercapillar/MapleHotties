using api.Data;
using api.Services;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<MapleTinderDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<CharacterScraper>();
//builder.Services.AddHostedService<CharacterScraperService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

//add argument when calling docker run or include commandLineArgs in launchSettings.json.
//Or just leave as it always try to install
if (args.Length > 0 && args[0] == "-i")
    PerformInitialPlaywrightInstall();


static void PerformInitialPlaywrightInstall()
{
    var exitCode = Microsoft.Playwright.Program.Main(new[] { "install", "--with-deps", "chromium" });
    if (exitCode != 0)
    {
        throw new Exception($"Playwright exited with code {exitCode}");
    }
}