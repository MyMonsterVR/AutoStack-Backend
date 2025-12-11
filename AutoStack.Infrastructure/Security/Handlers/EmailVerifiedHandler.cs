using System.Security.Claims;
using AutoStack.Domain.Repositories;
using AutoStack.Infrastructure.Security.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace AutoStack.Infrastructure.Security.Handlers;

/// <summary>
/// Handles authorization for the EmailVerifiedRequirement
/// </summary>
public class EmailVerifiedHandler : AuthorizationHandler<EmailVerifiedRequirement>
{
    private readonly IUserRepository _userRepository;

    public EmailVerifiedHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EmailVerifiedRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            context.Fail();
            return;
        }

        var user = await _userRepository.GetByIdAsync(userId);

        if (user != null && user.EmailVerified)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
