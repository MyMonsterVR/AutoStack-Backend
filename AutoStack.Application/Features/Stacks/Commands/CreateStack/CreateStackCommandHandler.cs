using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Stacks;
using AutoStack.Domain.Entities;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Stacks.Commands.CreateStack;

/// <summary>
/// Handles the create stack command by creating a new stack with its associated packages
/// </summary>
public class CreateStackCommandHandler : ICommandHandler<CreateStackCommand, StackResponse>
{
    private readonly IStackRepository _stackRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateStackCommandHandler(
        IStackRepository stackRepository,
        IUserRepository userRepository,
        IPackageRepository packageRepository,
        IUnitOfWork unitOfWork)
    {
        _stackRepository = stackRepository;
        _userRepository = userRepository;
        _packageRepository = packageRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Processes the create stack request by creating or reusing packages and linking them to the new stack
    /// </summary>
    /// <param name="request">The create stack command containing stack and package details</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A result containing the created stack response on success, or an error message on failure</returns>
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
            type: request.TypeResponse.ToString(),
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
            var existingPackage = await _packageRepository.GetByLinkAsync(packageInput.Link, cancellationToken);

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
                await _packageRepository.AddAsync(package, cancellationToken);
            }

            // Link package to stack
            var stackInfo = StackInfo.Create(stack.Id, package.Id);
            stack.AddStackInfo(stackInfo);
        }

        await _stackRepository.AddAsync(stack, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Load the stack with packages for response
        var loadedStack = await _stackRepository.GetByIdWithInfoAsync(stack.Id, cancellationToken);
        if (loadedStack == null)
        {
            return Result<StackResponse>.Failure("Failed to load created stack");
        }

        var stackInfoResponse = loadedStack.Packages
            .Select(si => new PackagesResponse(si.Package.Name, si.Package.Link, si.Package.IsVerified))
            .ToList();

        var user = await _userRepository.GetByIdAsync(stack.UserId, cancellationToken);
        if (user == null)
        {
            return Result<StackResponse>.Failure("No user found");
        }
        
        var response = new StackResponse
        {
            Id = stack.Id,
            Name = loadedStack.Name,
            Description = loadedStack.Description,
            TypeResponse = request.TypeResponse,
            Downloads = loadedStack.Downloads,
            Packages = stackInfoResponse,
            UserId =  loadedStack.UserId,
            Username = user.Username,
            UserAvatarUrl = user.AvatarUrl,
        };

        return Result<StackResponse>.Success(response);
    }
}