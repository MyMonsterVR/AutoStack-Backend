using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.TwoFactor;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Auth.Queries.TwoFactor.GetStatus;

public class GetTwoFactorStatusQueryHandler : IQueryHandler<GetTwoFactorStatusQuery, TwoFactorStatusResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRecoveryCodeRepository _recoveryCodeRepository;

    public GetTwoFactorStatusQueryHandler(
        IUserRepository userRepository,
        IRecoveryCodeRepository recoveryCodeRepository)
    {
        _userRepository = userRepository;
        _recoveryCodeRepository = recoveryCodeRepository;
    }

    public async Task<Result<TwoFactorStatusResponse>> Handle(GetTwoFactorStatusQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result<TwoFactorStatusResponse>.Failure("User not found");
        }

        // Count unused recovery codes
        var recoveryCodes = await _recoveryCodeRepository.GetUnusedByUserIdAsync(user.Id, cancellationToken);

        var response = new TwoFactorStatusResponse
        {
            IsEnabled = user.TwoFactorEnabled,
            EnabledAt = user.TwoFactorEnabledAt,
            RecoveryCodesRemaining = recoveryCodes.Count
        };

        return Result<TwoFactorStatusResponse>.Success(response);
    }
}
