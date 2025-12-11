using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Auth.Queries.EmailVerificationStatus;

public class GetEmailVerificationStatusQueryHandler : IQueryHandler<GetEmailVerificationStatusQuery, EmailVerificationStatusResponse>
{
    private readonly IUserRepository _userRepository;

    public GetEmailVerificationStatusQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<EmailVerificationStatusResponse>> Handle(
        GetEmailVerificationStatusQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null)
        {
            return Result<EmailVerificationStatusResponse>.Failure("User not found");
        }

        var response = new EmailVerificationStatusResponse(
            IsVerified: user.EmailVerified,
            VerifiedAt: user.EmailVerifiedAt,
            HasPendingCode: !string.IsNullOrEmpty(user.EmailVerificationCode) &&
                           user.EmailVerificationCodeExpiry > DateTime.UtcNow,
            CodeExpiresAt: user.EmailVerificationCodeExpiry
        );

        return Result<EmailVerificationStatusResponse>.Success(response);
    }
}
