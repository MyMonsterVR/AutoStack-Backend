using AutoStack.Application.Features.Users.Commands.CreateUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoStack.Presentation.Endpoints.User;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Add User";
                operation.Description = "Add User";
                return Task.CompletedTask;
            });

        group.MapPost("/", CreateUser)
            .WithName("CreateUser")
            .WithSummary("Create User")
            .Produces(200)
            .Produces(400);
    }

    private static async Task<IResult> CreateUser(
        CreateUserCommand command,
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
            message = "User created successfully"
        });
    }
}