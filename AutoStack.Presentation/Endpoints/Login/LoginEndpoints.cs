using AutoStack.Application.Features.Users.Commands.CreateUser;
using AutoStack.Application.Features.Users.Commands.Login;
using AutoStack.Application.Features.Users.Commands.RefreshToken;
using AutoStack.Application.Features.Users.Queries.GetUser;
using MediatR;

namespace AutoStack.Presentation.Endpoints.Login;

public static class LoginEndpoints
{
    public static void MapLoginEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/login")
            .WithTags("Login");

        group.MapPost("/", Login)
            .WithName("Login")
            .WithSummary("User login")
            .Produces(200)
            .Produces(400);
        
        group.MapPost("/refresh", RefreshToken)
            .WithName("RefreshToken")
            .WithSummary("Refresh token")
            .Produces(200)
            .Produces(400);
    }

    private static async Task<IResult> Login(
        LoginCommand command,
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
            data = result.Value
        });
    }

    private static async Task<IResult> RefreshToken(
        RefreshTokenCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

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
}