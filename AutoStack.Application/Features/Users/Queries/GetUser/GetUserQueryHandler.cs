using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Users;
using AutoStack.Domain.Enums;
using AutoStack.Domain.Repositories;
using Microsoft.Extensions.Logging;
using LogLevel = AutoStack.Domain.Enums.LogLevel;

namespace AutoStack.Application.Features.Users.Queries.GetUser;

/// <summary>
/// Handles the get user query by retrieving user information
/// </summary>
public class GetUserQueryHandler : IQueryHandler<GetUserQuery, UserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<GetUserQueryHandler> _logger;

    public GetUserQueryHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IAuditLogService auditLogService,
        ILogger<GetUserQueryHandler> logger)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    /// <summary>
    /// Processes the get user request by retrieving the user from the repository
    /// </summary>
    /// <param name="request">The get user query containing the user ID</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A result containing the user response on success, or an error message on failure</returns>
    public async Task<Result<UserResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var authenticatedUserId = _currentUserService.UserId;

        if (!authenticatedUserId.HasValue)
        {
            return Result<UserResponse>.Failure("Unauthorized");
        }

        if (authenticatedUserId.Value != request.id)
        {
            try
            {
                await _auditLogService.LogAsync(new AuditLogRequest
                {
                    Level = LogLevel.Warning,
                    Category = LogCategory.Authorization,
                    Message = "Unauthorized access attempt. A user tried to access another user's profile",
                    UserIdOverride = authenticatedUserId.Value,
                    UsernameOverride = _currentUserService.Username ?? "Unknown",
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["RequestedUserId"] = request.id,
                        ["AuthenticatedUserId"] = authenticatedUserId.Value
                    }
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unauthorized access attempt. A user tried to access another user's profile");
            }

            return Result<UserResponse>.Failure("Forbidden: You can only access your own profile");
        }

        var user = await _userRepository.GetByIdAsync(request.id, cancellationToken);
        if (user == null)
        {
            return Result<UserResponse>.Failure("User not found");
        }

        var response = new UserResponse(
            user.Id,
            user.Email,
            user.Username,
            user.AvatarUrl,
            user.EmailVerified
        );

        return Result<UserResponse>.Success(response);
    }
}