using System.Net;
using AutoStack.Application;
using AutoStack.Infrastructure;
using AutoStack.Presentation;
using AutoStack.Presentation.Endpoints.Cli;
using AutoStack.Presentation.Endpoints.EmailVerification;
using AutoStack.Presentation.Endpoints.Login;
using AutoStack.Presentation.Endpoints.Stack;
using AutoStack.Presentation.Endpoints.TwoFactor;
using AutoStack.Presentation.Endpoints.User;
using AutoStack.Presentation.Extensions;
using AutoStack.Presentation.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// Configure forwarded headers to properly read client IP behind our reverse proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    var proxyIp = builder.Configuration["ReverseProxy:KnownProxyIp"];
    if (!string.IsNullOrEmpty(proxyIp) && IPAddress.TryParse(proxyIp, out var parsedIp))
    {
        options.KnownProxies.Add(parsedIp);
    }
    else
    {
        // Development fallback - accept from any proxy (less secure)
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    }
});

builder.Services.AddRateLimiting();

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

// Must be called before other middleware to properly read forwarded headers
app.UseForwardedHeaders();

app.UseExceptionHandler();

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

// Configure static file serving for uploaded avatars
var uploadsPath = builder.Configuration["FileStorage:AvatarPath"] ?? "uploads/avatars";
var uploadDirectory = Path.IsPathRooted(uploadsPath)
    ? uploadsPath
    : Path.Combine(Directory.GetCurrentDirectory(), uploadsPath);

if (!Directory.Exists(uploadDirectory))
{
    Directory.CreateDirectory(uploadDirectory);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadDirectory),
    RequestPath = "/uploads/avatars",
    OnPrepareResponse = ctx =>
    {
        // Cache avatars for 7 days
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=604800");
    }
});

app.UseSecurityHeaders();

app.UseRateLimiter();

app.UseCors("AllowFrontend");

app.UseCookieAuthentication();

app.UseAuthentication();
app.UseHttpContextLogging();
app.UseAuthorization();

app.UseCliIdentification();

app.MapUserEndpoints();
app.MapLoginEndpoints();
app.MapTwoFactorEndpoints();
app.MapStackEndpoints();
app.MapCliEndpoints();
app.MapEmailVerificationEndpoints();

await app.RunAsync();