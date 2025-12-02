using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Models;
using AutoStack.Application.Features.Users.Commands.EditUser;
using AutoStack.Application.Features.Users.Queries.GetUser;
using AutoStack.Domain.Enums;
using MediatR;

namespace AutoStack.Presentation.Endpoints.User;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/user")
            .WithTags("Users");

        group.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithSummary("Get current authenticated user")
            .RequireAuthorization();

        group.MapGet("/{id:guid}", GetUserById)
            .WithName("GetUserById")
            .WithSummary("Get User by Id")
            .RequireAuthorization();
        
        group.MapPatch("/edit", EditUser)
            .WithName("EditUser")
            .WithSummary("Edit User")
            .RequireAuthorization();
    }

    private static async Task<IResult> GetCurrentUser(
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        // Extract authenticated user's ID from JWT token
        var authenticatedUserIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(authenticatedUserIdClaim) || !Guid.TryParse(authenticatedUserIdClaim, out var authenticatedUserId))
        {
            return Results.Unauthorized();
        }

        var query = new GetUserQuery(authenticatedUserId);
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

    private static async Task<IResult> GetUserById(
        Guid id,
        IMediator mediator,
        HttpContext httpContext,
        IAuditLogService auditLogService,
        CancellationToken cancellationToken)
    {
        // Extract authenticated user's ID from JWT token
        var authenticatedUserIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var authenticatedUsernameClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(authenticatedUserIdClaim) || !Guid.TryParse(authenticatedUserIdClaim, out var authenticatedUserId))
        {
            return Results.Unauthorized();
        }

        // Check if authenticated user is accessing their own data
        if (authenticatedUserId != id)
        {
            // Log unauthorized access attempt
            try
            {
                await auditLogService.LogAsync(new AuditLogRequest
                {
                    Level = Domain.Enums.LogLevel.Warning,
                    Category = LogCategory.Authorization,
                    Message = "Unauthorized access attempt - user tried to access another user's profile",
                    UserIdOverride = authenticatedUserId,
                    UsernameOverride = authenticatedUsernameClaim ?? "Unknown",
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["RequestedUserId"] = id,
                        ["AuthenticatedUserId"] = authenticatedUserId
                    }
                }, cancellationToken);
            }
            catch
            {
                // Ignore logging failures
            }

            return Results.Forbid();
        }

        var query = new GetUserQuery(id);
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

    private static async Task<IResult> EditUser(
        EditUserCommand command,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authenticatedUserIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(authenticatedUserIdClaim) || !Guid.TryParse(authenticatedUserIdClaim, out var authenticatedUserId))
        {
            return Results.Unauthorized();
        }
        
        var result = await mediator.Send(command with { UserId = authenticatedUserId }, cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new
            {
                success = false,
                message = result.Message,
                errors = result.ValidationErrors
            });
        }
        
        return Results.Ok(result.Value);
    }
}