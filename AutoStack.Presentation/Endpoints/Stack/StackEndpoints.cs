using AutoStack.Application.Common.Models;
using AutoStack.Application.Features.Stacks.Commands.CreateStack;
using AutoStack.Application.Features.Stacks.Commands.DeleteStack;
using AutoStack.Application.Features.Stacks.Queries.GetStack;
using AutoStack.Application.Features.Stacks.Queries.GetStacks;
using AutoStack.Application.Features.Stacks.Queries.MyStacks;
using AutoStack.Application.Features.Stacks.Queries.VerifiedPackages;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoStack.Presentation.Endpoints.Stack;

public static class StackEndpoints
{
    public static void MapStackEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stack")
            .WithTags("Stack");

        group.MapPost("/create", CreateStack)
            .WithName("CreateStack")
            .WithSummary("Create a new Stack")
            .RequireAuthorization();

        group.MapDelete("/deletestack", DeleteStack)
            .WithName("DeleteStack")
            .WithTags("Stack")
            .RequireAuthorization();
        
        group.MapGet("/getstacks", GetStacks)
            .WithName("GetStacks")
            .WithSummary("Get paginated stacks");
        
        group.MapGet("/getstack", GetStack)
            .WithName("GetStack")
            .WithSummary("Get specific stack");
        
        group.MapGet("/verifiedpackages", GetVerifiedPackages)
            .WithName("Packages")
            .WithSummary("Get packages")
            .RequireAuthorization();
        
        group.MapGet("/mystacks", MyStacks)
            .WithName("MyStacks")
            .WithSummary("Get all stacks")
            .RequireAuthorization();
    }
    
    private static async Task<IResult> CreateStack(
        CreateStackCommand command,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var authenticatedUserIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(authenticatedUserIdClaim, out var authenticatedUserId))
        {
            return Results.Unauthorized();
        }

        var commandWithUser = command with { UserId = authenticatedUserId };
        var result = await mediator.Send(commandWithUser, cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new
            {
                success = false,
                message = result.Message,
                errors  = result.ValidationErrors
            });
        }

        return Results.Ok(new
        {
            success = true,
            data    = result.Value
        });
    }

    private static async Task<IResult> GetStacks(
        [AsParameters] GetStacksQuery query,
        IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        var result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new
            {
                success = false,
                message = result.Message,
                errors  = result.ValidationErrors
            });
        }

        return Results.Ok(new
        {
            success = true,
            data    = result.Value
        });
    }

    private static async Task<IResult> DeleteStack(
        [FromBody] DeleteStackCommand command,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authenticatedUserIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(authenticatedUserIdClaim, out var authenticatedUserId))
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

        return Results.Ok(new
        {
            success = true,
            data = result.Value
        });
    }

    private static async Task<IResult> GetStack(
        [AsParameters] GetStackQuery query,
        IMediator mediator,
        CancellationToken cancellationToken
    )
    {
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

    private static async Task<IResult> GetVerifiedPackages(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetVerifiedPackagesQuery();
        var result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.BadRequest();
        }

        return Results.Ok(new
        {
            success = true,
            data = result.Value
        });
    }

    private static async Task<IResult> MyStacks(
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authenticatedUserIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(authenticatedUserIdClaim, out var authenticatedUserId))
        {
            return Results.Unauthorized();
        }
        
        var query = new MyStacksQuery(authenticatedUserId);
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
}