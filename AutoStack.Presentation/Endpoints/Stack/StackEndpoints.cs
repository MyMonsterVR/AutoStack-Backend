using AutoStack.Application.Features.Stacks.Commands.CreateStack;
using AutoStack.Application.Features.Stacks.Queries.GetStacks;
using MediatR;

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
        
        group.MapGet("/getstacks", GetStacks)
            .WithName("GetStacks")
            .WithSummary("Get paginated stacks")
            .WithTags("Stack");
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
}