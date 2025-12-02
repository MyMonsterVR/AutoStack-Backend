using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Stacks.Commands.DeleteStack;

public class DeleteStackCommandHandler : ICommandHandler<DeleteStackCommand, bool>
{
    private readonly IStackRepository _stackRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteStackCommandHandler(IStackRepository stackRepository, IUnitOfWork unitOfWork)
    {
        _stackRepository = stackRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result<bool>> Handle(DeleteStackCommand request, CancellationToken cancellationToken)
    {
        var stack = await _stackRepository.GetByIdAsync(request.StackId, cancellationToken);
        if (stack == null)
        {
            return Result<bool>.Failure("Stack not found");
        }
        
        await _stackRepository.DeleteAsync(stack, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result<bool>.Success(true);
    }
}