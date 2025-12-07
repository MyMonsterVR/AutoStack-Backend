using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.DTOs.TwoFactor;

namespace AutoStack.Application.Features.Auth.Queries.TwoFactor.GetStatus;

public record GetTwoFactorStatusQuery(Guid UserId) : IQuery<TwoFactorStatusResponse>;
