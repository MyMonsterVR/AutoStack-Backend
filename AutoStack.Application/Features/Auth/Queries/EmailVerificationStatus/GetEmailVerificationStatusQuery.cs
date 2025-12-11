using AutoStack.Application.Common.Interfaces.Queries;

namespace AutoStack.Application.Features.Auth.Queries.EmailVerificationStatus;

public record GetEmailVerificationStatusQuery(Guid UserId) : IQuery<EmailVerificationStatusResponse>;

public record EmailVerificationStatusResponse(
    bool IsVerified,
    DateTime? VerifiedAt,
    bool HasPendingCode,
    DateTime? CodeExpiresAt
);
