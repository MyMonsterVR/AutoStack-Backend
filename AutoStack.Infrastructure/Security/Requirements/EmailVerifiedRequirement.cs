using Microsoft.AspNetCore.Authorization;

namespace AutoStack.Infrastructure.Security.Requirements;

/// <summary>
/// Authorization requirement that checks if the user's email is verified
/// </summary>
public class EmailVerifiedRequirement : IAuthorizationRequirement
{
}
