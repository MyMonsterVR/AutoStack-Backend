using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Stacks;
using AutoStack.Domain.Entities;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Stacks.Commands.CreateStack;

public class CreateStackCommandHandler(
    IStackRepository stackRepository,
    IPackageRepository packageRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateStackCommand, StackResponse>
{
    public async Task<Result<StackResponse>> Handle(CreateStackCommand request, CancellationToken cancellationToken)
    {
        if (!request.UserId.HasValue)
        {
            return Result<StackResponse>.Failure("User ID is required");
        }

        // Create the stack
        var stack = Stack.Create(
            name: request.Name,
            description: request.Description,
            type: request.Type.ToString(),
            userId: request.UserId.Value
        );

        // Deduplicate packages by Link
        var uniquePackages = request.Packages
            .GroupBy(x => x.Link)
            .Select(g => g.First())
            .ToList();

        // Process each package: reuse existing or create new
        foreach (var packageInput in uniquePackages)
        {
            // Check if package already exists by link
            var existingPackage = await packageRepository.GetByLinkAsync(packageInput.Link, cancellationToken);

            Package package;
            if (existingPackage != null)
            {
                // Reuse existing package
                package = existingPackage;
            }
            else
            {
                // Create new custom package (unverified)
                package = Package.Create(packageInput.Name, packageInput.Link, isVerified: false);
                await packageRepository.AddAsync(package, cancellationToken);
            }

            // Link package to stack
            var stackInfo = StackInfo.Create(stack.Id, package.Id);
            stack.AddStackInfo(stackInfo);
        }

        await stackRepository.AddAsync(stack, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Load the stack with packages for response
        var loadedStack = await stackRepository.GetByIdWithInfoAsync(stack.Id, cancellationToken);
        if (loadedStack == null)
        {
            return Result<StackResponse>.Failure("Failed to load created stack");
        }

        var stackInfoResponse = loadedStack.StackInfo
            .Select(si => new StackInfoResponse(si.Package.Name, si.Package.Link, si.Package.IsVerified))
            .ToList();

        var response = new StackResponse
        {
            Name = loadedStack.Name,
            Description = loadedStack.Description,
            Type = request.Type,
            Downloads = loadedStack.Downloads,
            StackInfo = stackInfoResponse
        };

        return Result<StackResponse>.Success(response);
    }
}