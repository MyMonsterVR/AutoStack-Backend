using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.TwoFactor;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Auth.Commands.TwoFactor.BeginSetup;

public class BeginTwoFactorSetupCommandHandler : ICommandHandler<BeginTwoFactorSetupCommand, BeginTwoFactorSetupResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ITotpService _totpService;
    private readonly IToken _token;

    public BeginTwoFactorSetupCommandHandler(
        IUserRepository userRepository,
        ITotpService totpService,
        IToken token)
    {
        _userRepository = userRepository;
        _totpService = totpService;
        _token = token;
    }

    public async Task<Result<BeginTwoFactorSetupResponse>> Handle(BeginTwoFactorSetupCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result<BeginTwoFactorSetupResponse>.Failure("User not found");
        }

        if (user.TwoFactorEnabled)
        {
            return Result<BeginTwoFactorSetupResponse>.Failure("Two-factor authentication is already enabled");
        }

        var secretKey = _totpService.GenerateSecretKey();
        var totpUri = _totpService.GenerateTotpUri(secretKey, user.Email, "AutoStack");
        var qrCode = _totpService.GenerateQrCodeBase64(totpUri);

        var setupToken = _token.GenerateSetupToken(user.Id, secretKey);

        var response = new BeginTwoFactorSetupResponse
        {
            SetupToken = setupToken,
            ManualEntryKey = secretKey,
            QrCode = qrCode
        };

        return Result<BeginTwoFactorSetupResponse>.Success(response);
    }
}
