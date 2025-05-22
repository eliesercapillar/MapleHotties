using api.Authentication.Interfaces;
using api.Authentication.Service;
using api.Interfaces;
using api.Services;
using MapleTinder.Shared.Data;
using MapleTinder.Shared.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;


// ----------------------------------------------------------
// Build
// ----------------------------------------------------------

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

// ----------------------------------------------------------
// Build - Services
// ----------------------------------------------------------

// ASP.NET Controllers
builder.Services.AddControllers();

// EF Core
builder.Services.AddDbContext<MapleTinderDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<MapleTinderDbContext>()
    .AddDefaultTokenProviders();

// Authentication: JWT, Google, Discord
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
    options.DefaultChallengeScheme =
    options.DefaultForbidScheme =
    options.DefaultScheme =
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;

    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})  
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,

            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"]!))
        };
    })
    //.AddGoogle("Google", options =>
    //{
    //    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    //    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    //options.SignInScheme = IdentityConstants.ExternalScheme;
    //})
    .AddDiscord(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Discord:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Discord:ClientSecret"]!;

        options.Scope.Add("identify");

        options.SignInScheme = IdentityConstants.ExternalScheme;
        //options.CallbackPath = new PathString("/auth/login/discord/success");
        options.AccessDeniedPath = new PathString("/auth/login/discord/fail");

        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "username");
        options.SaveTokens = true;
        

        //options.CorrelationCookie.Name = ".AspNetCore.Correlation.Discord";
        //options.CorrelationCookie.SameSite = SameSiteMode.None;
        //options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        //options.CorrelationCookie.Domain = "localhost";
        //options.CorrelationCookie.Path = "/auth/login/discord/success";
    });
//.AddOAuth("Discord", options =>
//{
//    options.AuthorizationEndpoint = "https://discord.com/oauth2/authorize";
//    options.TokenEndpoint = "https://discord.com/api/oauth2/token";
//    options.UserInformationEndpoint = "https://discord.com/api/users/@me";

//    options.ClientId = builder.Configuration["Authentication:Discord:ClientId"]!;
//    options.ClientSecret = builder.Configuration["Authentication:Discord:ClientSecret"]!;
//    options.Scope.Add("identify");

//    options.CallbackPath = new PathString("/auth/login/discord/success");
//    options.AccessDeniedPath = new PathString("/auth/login/discord/fail");



//    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
//    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "username");
//    options.SaveTokens = true;

//    options.CorrelationCookie.Path = "/";
//    options.CorrelationCookie.SameSite = SameSiteMode.None;
//    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
//    options.SignInScheme = IdentityConstants.ExternalScheme;

//    options.Events = new OAuthEvents
//    {
//        OnCreatingTicket = async context =>
//        {
//            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
//            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

//            var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
//            response.EnsureSuccessStatusCode();

//            var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
//            context.RunClaimActions(document);
//        }
//    };
//});

// Authorization
builder.Services.AddAuthorization();

// Cookies
builder.Services.ConfigureExternalCookie(opts =>
{
    opts.Cookie.Name = ".AspNetCore.External";
    opts.Cookie.SameSite = SameSiteMode.None;           // REQUIRED for cross-site flows
    opts.Cookie.SecurePolicy = CookieSecurePolicy.Always;   // HTTPS only
    opts.Cookie.HttpOnly = true;
    opts.Cookie.Domain = "localhost";                 // unify across ports
    opts.Cookie.Path = "/";                         // so it’s sent on both /auth/login and /auth/login/success
});

// Expose scraper container
builder.Services.AddHttpClient<ICharacterScraperClient, HttpCharacterScraperClient>(client =>
{
    client.BaseAddress = new Uri("http://scraper:8080"); // 'scraper' is the container name in docker-compose
});

// Swagger
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "MapleTinder API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

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

// Other Services
builder.Services.AddScoped<ITokenService, TokenService>(); // JWT Token Creation Service

// ----------------------------------------------------------
// App
// ----------------------------------------------------------

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("DevAllowVue");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();