using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using MinimalAPI.Data;
using MinimalAPI.Models;
using MinimalAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Context 
builder.Services.AddDbContext<Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Context") ?? throw new InvalidOperationException("Connection string 'Context' not found.")));

builder.Services.AddControllersWithViews();

// Servi�os
builder.Services.AddScoped<LoginService>();

// Autentica��o JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Autoriza��o JWT
builder.Services.AddAuthorization();

var app = builder.Build();

// Middlewares
app.UseAuthentication();
app.UseAuthorization();

// Cultura padr�o pt-BR
var defaultCulture = new CultureInfo("pt-BR");
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new[] { defaultCulture },
    SupportedUICultures = new[] { defaultCulture }
};

// Configura��es de localiza��o
app.UseRequestLocalization(localizationOptions);

// Rota inicial
app.MapGet("/", () => "Minimal API em execu��o");

// POST login - Valida acesso e retorna token
app.MapPost("/login", async (Login login, LoginService loginService) =>
{
    var token = await loginService.RealizaLogin(login);
    return token is null ? Results.Unauthorized() : Results.Ok(new { Token = token });
});

// GET usuarios - Retorna lista de usuarios apenas se token for v�lido
app.MapGet("/usuario", async () =>
{
    // TODO
});

app.Run();

