using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.DTOs.Stacks;

namespace AutoStack.Application.Features.Stacks.Commands.TrackDownload;

/// <summary>
/// Command to track a stack download (increment download counter)
/// </summary>
/// <param name="StackId">The ID of the stack being downloaded</param>
public record TrackDownloadCommand(Guid StackId) : ICommand<StackResponse>;
