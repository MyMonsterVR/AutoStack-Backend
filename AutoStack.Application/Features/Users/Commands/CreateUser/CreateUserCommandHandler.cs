using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Models;

namespace AutoStack.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, int>
{
    public async Task<Result<int>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        return Result<int>.Success(1);
    }
}