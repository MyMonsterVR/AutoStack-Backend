using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Users.Queries.GetUser;

public class GetUserQueryHandler(IUserRepository userRepository) : IQueryHandler<GetUserQuery, GetUserQueryRespones>
{
    public async Task<Result<GetUserQueryRespones>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.id, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User with id {request.id} was not found");
        }

        var response = new GetUserQueryRespones(
            user.Id,
            user.Email,
            user.Username,
            user.CreatedAt,
            user.UpdatedAt
        );

        return Result<GetUserQueryRespones>.Success(response);
    }
}