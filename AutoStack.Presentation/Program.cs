using System.Text;
using AutoStack.Application;
using AutoStack.Infrastructure;
using AutoStack.Presentation;
using AutoStack.Presentation.Endpoints.Login;
using AutoStack.Presentation.Endpoints.User;
using AutoStack.Presentation.Middleware;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                  "http://localhost:3000",
                  "https://localhost:3000",
                  "https://autostack.dk",
                  "http://autostack.dk")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddPresentation();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthorizationService(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    { 
        options.DarkMode = true;
        options.HideDarkModeToggle = true;
        options.Authentication = new ScalarAuthenticationOptions()
        {
            PreferredSecuritySchemes = ["Bearer"]
        };
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseCookieAuthentication();

app.UseAuthentication();
app.UseAuthorization();

app.MapUserEndpoints();
app.MapLoginEndpoints();

await app.RunAsync();