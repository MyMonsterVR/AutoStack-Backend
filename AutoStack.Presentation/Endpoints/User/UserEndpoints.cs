using AutoStack.Application.Features.Users.Commands.CreateUser;
using AutoStack.Application.Features.Users.Queries.GetUser;
using MediatR;

namespace AutoStack.Presentation.Endpoints.User;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users");

        group.MapPost("/register", CreateUser)
            .WithName("CreateUser")
            .WithSummary("Create User")
            .Produces(200)
            .Produces(400);

        group.MapGet("/user/{id:guid}", GetUserById)
            .WithName("GetUserById")
            .WithSummary("Get User by Id");
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
            data    = result.Value
        });
    }

    private static async Task<IResult> GetUserById(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
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
}