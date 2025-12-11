using System.Security.Claims;
using AutoStack.Application.Features.Auth.Commands.ResendVerificationEmail;
using AutoStack.Application.Features.Auth.Commands.VerifyEmail;
using AutoStack.Application.Features.Auth.Queries.EmailVerificationStatus;
using MediatR;

namespace AutoStack.Presentation.Endpoints.EmailVerification;

public static class EmailVerificationEndpoints
{
    public static void MapEmailVerificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/email-verification")
            .WithTags("Email Verification");

        group.MapPost("/verify", VerifyEmail)
            .WithName("VerifyEmail")
            .WithSummary("Verify email with 6-digit code")
            .RequireAuthorization()
            .RequireRateLimiting("email-verify")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(429);

        group.MapPost("/resend", ResendVerificationEmail)
            .WithName("ResendVerificationEmail")
            .WithSummary("Resend verification email")
            .RequireAuthorization()
            .RequireRateLimiting("email-resend")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(429);

        group.MapGet("/status", GetVerificationStatus)
            .WithName("GetEmailVerificationStatus")
            .WithSummary("Get email verification status")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401);
    }

    private static async Task<IResult> VerifyEmail(
        VerifyEmailRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims(context);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        var command = new VerifyEmailCommand(userId.Value, request.Code);
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
            message = "Email verified successfully"
        });
    }

    private static async Task<IResult> ResendVerificationEmail(
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims(context);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        var command = new ResendVerificationEmailCommand(userId.Value);
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
            message = "Verification email sent"
        });
    }

    private static async Task<IResult> GetVerificationStatus(
        HttpContext context,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims(context);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        var query = new GetEmailVerificationStatusQuery(userId.Value);
        var result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new
            {
                success = false,
                message = result.Message
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

public record VerifyEmailRequest(string Code);
