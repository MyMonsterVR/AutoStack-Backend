using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Users;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Users.Queries.GetUser;

/// <summary>
/// Handles the get user query by retrieving user information
/// </summary>
public class GetUserQueryHandler(IUserRepository userRepository) : IQueryHandler<GetUserQuery, UserResponse>
{
    /// <summary>
    /// Processes the get user request by retrieving the user from the repository
    /// </summary>
    /// <param name="request">The get user query containing the user ID</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A result containing the user response on success, or an error message on failure</returns>
    public async Task<Result<UserResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.id, cancellationToken);
        if (user == null)
        {
            return Result<UserResponse>.Failure("User not found");
        }

        var response = new UserResponse(
            user.Id,
            user.Email,
            user.Username,
            user.AvatarUrl
        );

        return Result<UserResponse>.Success(response);
    }
}