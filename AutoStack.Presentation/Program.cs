using System.Text;
using System.Threading.RateLimiting;
using AutoStack.Application;
using AutoStack.Infrastructure;
using AutoStack.Presentation;
using AutoStack.Presentation.Endpoints.Cli;
using AutoStack.Presentation.Endpoints.Login;
using AutoStack.Presentation.Endpoints.Stack;
using AutoStack.Presentation.Endpoints.User;
using AutoStack.Presentation.Middleware;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddRateLimiter(options =>
{
    // Global rate limiter for all endpoints
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Strict rate limiter for authentication endpoint
    options.AddFixedWindowLimiter("login", options =>
    {
        options.PermitLimit = 5;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 0;
    });

    // Rate limiter for registration
    options.AddFixedWindowLimiter("register", options =>
    {
        options.PermitLimit = 3;
        options.Window = TimeSpan.FromHours(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 0;
    });

    // Rate limiter for token refresh
    options.AddFixedWindowLimiter("refresh", options =>
    {
        options.PermitLimit = 10;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 0;
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            success = false,
            message = "Too many requests. Please try again later."
        }, cancellationToken);
    };
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

app.UseSecurityHeaders();

app.UseRateLimiter();

app.UseCors("AllowFrontend");

app.UseCookieAuthentication();

app.UseAuthentication();
app.UseAuthorization();

app.MapUserEndpoints();
app.MapLoginEndpoints();
app.MapStackEndpoints();
app.MapCliEndpoints();

await app.RunAsync();