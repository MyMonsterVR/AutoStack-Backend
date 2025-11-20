using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Users.Queries.GetUser;

public class GetUserQueryHandler(IUserRepository userRepository) : IQueryHandler<GetUserQuery, UserRespones>
{
    public async Task<Result<UserRespones>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.id, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User with id {request.id} was not found");
        }

        var response = new UserRespones(
            user.Id,
            user.Email,
            user.Username,
            user.CreatedAt,
            user.UpdatedAt
        );

        return Result<UserRespones>.Success(response);
    }
}