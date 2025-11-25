using AutoStack.Application.Features.Users.Commands.Login;
using AutoStack.Application.Features.Users.Commands.RefreshToken;
using AutoStack.Application.Features.Users.Commands.Register;
using AutoStack.Infrastructure.Security;
using MediatR;
using Microsoft.Extensions.Options;

namespace AutoStack.Presentation.Endpoints.Login;

public static class LoginEndpoints
{
    public static void MapLoginEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/")
            .WithTags("Login");

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("User login")
            .Produces(200)
            .Produces(400);
        
        group.MapPost("/register", Register)
            .WithName("CreateUser")
            .WithSummary("Create User")
            .Produces(200)
            .Produces(400);

        group.MapPost("/refresh", RefreshToken)
            .WithName("RefreshToken")
            .WithSummary("Refresh token")
            .Produces(200)
            .Produces(400);

        group.MapPost("/logout", Logout)
            .WithName("Logout")
            .WithSummary("Logout user")
            .Produces(200);
    }

    private static async Task<IResult> Login(
        LoginCommand command,
        IMediator mediator,
        ICookieManager cookieManager,
        IOptions<JwtSettings> jwtSettings,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new
            {
                success = false,
                message = result.Message,
                errors = result.ValidationErrors
            });
        }

        cookieManager.SetAccessTokenCookie(
            httpContext,
            result.Value.AccessToken,
            jwtSettings.Value.ExpirationMinutes
        );

        cookieManager.SetRefreshTokenCookie(
            httpContext,
            result.Value.RefreshToken,
            jwtSettings.Value.RefreshTokenExpirationDays
        );

        return Results.Ok(new
        {
            success = true,
            data = result.Value
        });
    }
    
    private static async Task<IResult> Register(
        RegisterCommand command,
        IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new
            {
                success = false,
                message = result.Message,
                errors = result.ValidationErrors
            });
        }

        return Results.Ok(new
        {
            success = true,
            data    = result.Value
        });
    }


    private static async Task<IResult> RefreshToken(
        IMediator mediator,
        ICookieManager cookieManager,
        IOptions<JwtSettings> jwtSettings,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var refreshToken = cookieManager.GetRefreshTokenFromCookie(httpContext);

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Results.BadRequest(new
            {
                success = false,
                message = "Refresh token not found"
            });
        }

        var command = new RefreshTokenCommand(refreshToken);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            cookieManager.ClearAuthCookies(httpContext);

            return Results.BadRequest(new
            {
                success = false,
                message = result.Message
            });
        }

        cookieManager.SetAccessTokenCookie(
            httpContext,
            result.Value.AccessToken,
            jwtSettings.Value.ExpirationMinutes
        );

        cookieManager.SetRefreshTokenCookie(
            httpContext,
            result.Value.RefreshToken,
            jwtSettings.Value.RefreshTokenExpirationDays
        );

        return Results.Ok(new
        {
            success = true,
            message = "Token refreshed successfully"
        });
    }

    private static IResult Logout(
        ICookieManager cookieManager,
        HttpContext httpContext)
    {
        cookieManager.ClearAuthCookies(httpContext);

        return Results.Ok(new
        {
            success = true,
            message = "Logged out successfully"
        });
    }
}
