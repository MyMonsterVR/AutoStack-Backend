using AutoStack.Application.Common.Interfaces.Commands;

namespace AutoStack.Application.Features.Users.Commands.DeleteAccount;

public record DeleteAccountCommand(Guid? UserId) : ICommand<bool>;