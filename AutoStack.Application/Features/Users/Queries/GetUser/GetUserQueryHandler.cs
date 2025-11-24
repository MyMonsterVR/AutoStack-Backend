using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Users;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Users.Queries.GetUser;

public class GetUserQueryHandler(IUserRepository userRepository) : IQueryHandler<GetUserQuery, UserResponses>
{
    public async Task<Result<UserResponses>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.id, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User with id {request.id} was not found");
        }

        var response = new UserResponses(
            user.Id,
            user.Email,
            user.Username
        );

        return Result<UserResponses>.Success(response);
    }
}