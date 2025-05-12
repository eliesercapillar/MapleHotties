using api.Interfaces;
using api.Services;
using MapleTinder.Shared.Data;
using MapleTinder.Shared.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

// Add services to the container.

builder.Services.AddControllers();

// EF Core
builder.Services.AddDbContext<MapleTinderDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<MapleTinderDbContext>()
    .AddDefaultTokenProviders();

// Authentication: JWT, Google, Discord
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
            ValidateIssuerSigningKey = true
        };
    })
    .AddGoogle("Google", options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        options.SignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddOAuth("Discord", options =>
    {
        options.ClientId = builder.Configuration["Authentication:Discord:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Discord:ClientSecret"]!;
        options.CallbackPath = "/signin-discord";
        options.AuthorizationEndpoint = "https://discord.com/api/oauth2/authorize";
        options.TokenEndpoint = "https://discord.com/api/oauth2/token";
        options.UserInformationEndpoint = "https://discord.com/api/users/@me";
        options.SaveTokens = true;
        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "username");
        options.Scope.Add("identify");
        options.Events = new OAuthEvents
        {
            OnCreatingTicket = async ctx =>
            {
                var userJson = await ctx.Backchannel.GetStringAsync(ctx.Options.UserInformationEndpoint);
                var user = JsonDocument.Parse(userJson);
                ctx.RunClaimActions(user.RootElement);
            }
        };
    });
builder.Services.AddAuthorization();

// Expose scraper container
builder.Services.AddHttpClient<ICharacterScraperClient, HttpCharacterScraperClient>(client =>
{
    client.BaseAddress = new Uri("http://scraper:8080"); // 'scraper' is the container name in docker-compose
});

// Swagger
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevAllowVue", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // The Vite frontend URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("DevAllowVue");
app.UseAuthorization();
app.MapControllers();

app.Run();