using System.Security.Claims;
using AutoStack.Application.Features.Auth.Commands.TwoFactor.BeginSetup;
using AutoStack.Application.Features.Auth.Commands.TwoFactor.ConfirmSetup;
using AutoStack.Application.Features.Auth.Commands.TwoFactor.Disable;
using AutoStack.Application.Features.Auth.Commands.TwoFactor.RegenerateRecoveryCodes;
using AutoStack.Application.Features.Auth.Commands.TwoFactor.UseRecoveryCode;
using AutoStack.Application.Features.Auth.Commands.TwoFactor.VerifyLogin;
using AutoStack.Application.Features.Auth.Queries.TwoFactor.GetStatus;
using AutoStack.Infrastructure.Security;
using AutoStack.Infrastructure.Security.Models;
using AutoStack.Presentation.Models;
using MediatR;
using Microsoft.Extensions.Options;

namespace AutoStack.Presentation.Endpoints.TwoFactor;

public static class TwoFactorEndpoints
{
    public static void MapTwoFactorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/2fa")
            .WithTags("Two-Factor Authentication");

        // Setup endpoints (require authentication)
        group.MapPost("/setup/begin", BeginSetup)
            .WithName("BeginTwoFactorSetup")
            .WithSummary("Begin 2FA setup - generates secret and QR code")
            .RequireAuthorization()
            .Produces(200)
            .Produces(400)
            .Produces(401);

        group.MapPost("/setup/confirm", ConfirmSetup)
            .WithName("ConfirmTwoFactorSetup")
            .WithSummary("Confirm 2FA setup with TOTP code")
            .RequireAuthorization()
            .Produces(200)
            .Produces(400)
            .Produces(401);

        group.MapPost("/disable", Disable)
            .WithName("DisableTwoFactor")
            .WithSummary("Disable 2FA")
            .RequireAuthorization()
            .RequireRateLimiting("2fa-sensitive")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(429);

        group.MapPost("/recovery-codes/regenerate", RegenerateRecoveryCodes)
            .WithName("RegenerateRecoveryCodes")
            .WithSummary("Generate new recovery codes")
            .RequireAuthorization()
            .RequireRateLimiting("2fa-sensitive")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(429);

        // Verification endpoints (no authentication, use temporary token)
        group.MapPost("/verify", VerifyLogin)
            .WithName("VerifyTwoFactorLogin")
            .WithSummary("Complete login with TOTP code")
            .RequireRateLimiting("2fa-verify")
            .Produces(200)
            .Produces(400)
            .Produces(429);

        group.MapPost("/verify/recovery", UseRecoveryCode)
            .WithName("UseRecoveryCode")
            .WithSummary("Complete login with recovery code")
            .RequireRateLimiting("2fa-recovery")
            .Produces(200)
            .Produces(400)
            .Produces(429);

        // Status endpoint
        group.MapGet("/status", GetStatus)
            .WithName("GetTwoFactorStatus")
            .WithSummary("Get 2FA status for current user")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401);
    }

    private static async Task<IResult> BeginSetup(
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims(context);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        var command = new BeginTwoFactorSetupCommand(userId.Value);
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
            data = result.Value
        });
    }

    private static async Task<IResult> ConfirmSetup(
        ConfirmTwoFactorSetupCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
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
            data = result.Value
        });
    }

    private static async Task<IResult> Disable(
        DisableTwoFactorRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims(context);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        var command = new DisableTwoFactorCommand(userId.Value, request.Password, request.TotpCode);
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
            message = "Two-factor authentication disabled successfully"
        });
    }

    private static async Task<IResult> RegenerateRecoveryCodes(
        RegenerateRecoveryCodesRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims(context);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        var command = new RegenerateRecoveryCodesCommand(userId.Value, request.Password, request.TotpCode);
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
            data = result.Value
        });
    }

    private static async Task<IResult> VerifyLogin(
        VerifyTwoFactorLoginCommand command,
        IMediator mediator,
        ICookieManager cookieManager,
        HttpContext context,
        IOptions<JwtSettings> jwtSettings,
        CancellationToken cancellationToken)
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

        // Set tokens in cookies
        if (result.Value.AccessToken != null && result.Value.RefreshToken != null)
        {
            cookieManager.SetAccessTokenCookie(
                context,
                result.Value.AccessToken,
                jwtSettings.Value.ExpirationMinutes
            );

            cookieManager.SetRefreshTokenCookie(
                context,
                result.Value.RefreshToken,
                jwtSettings.Value.RefreshTokenExpirationDays
            );
        }

        return Results.Ok(new
        {
            success = true,
            data = result.Value
        });
    }

    private static async Task<IResult> UseRecoveryCode(
        UseRecoveryCodeCommand command,
        IMediator mediator,
        ICookieManager cookieManager,
        HttpContext context,
        IOptions<JwtSettings> jwtSettings,
        CancellationToken cancellationToken)
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

        // Set tokens in cookies
        if (result.Value.AccessToken != null && result.Value.RefreshToken != null)
        {
            cookieManager.SetAccessTokenCookie(
                context,
                result.Value.AccessToken,
                jwtSettings.Value.ExpirationMinutes
            );

            cookieManager.SetRefreshTokenCookie(
                context,
                result.Value.RefreshToken,
                jwtSettings.Value.RefreshTokenExpirationDays
            );
        }

        return Results.Ok(new
        {
            success = true,
            data = result.Value
        });
    }

    private static async Task<IResult> GetStatus(
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims(context);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        var query = new GetTwoFactorStatusQuery(userId.Value);
        var result = await mediator.Send(query, cancellationToken);

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
            data = result.Value
        });
    }

    private static Guid? GetUserIdFromClaims(HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
