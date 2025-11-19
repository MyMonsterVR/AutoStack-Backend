using AutoStack.Application;
using AutoStack.Infrastructure;
using AutoStack.Presentation;
using AutoStack.Presentation.Endpoints.User;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddPresentation();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
    
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        // Oh, you wanted to use light mode? nah, that's not allowed in our docs
        options.DarkMode = true;
        options.HideDarkModeToggle = true;
    });
}

app.UseHttpsRedirection();

app.MapUserEndpoints();

await app.RunAsync();