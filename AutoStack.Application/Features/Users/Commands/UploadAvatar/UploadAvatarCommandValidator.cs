using FluentValidation;

namespace AutoStack.Application.Features.Users.Commands.UploadAvatar;

public class UploadAvatarCommandValidator : AbstractValidator<UploadAvatarCommand>
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long MaxFileSizeBytes = 5242880; // 5MB

    public UploadAvatarCommandValidator()
    {
        RuleFor(c => c.FileName)
            .NotEmpty().WithMessage("File name is required.")
            .Must(HaveValidExtension).WithMessage($"File must be one of the following types: {string.Join(", ", AllowedExtensions)}");

        RuleFor(c => c.FileSize)
            .GreaterThan(0).WithMessage("File cannot be empty.")
            .LessThanOrEqualTo(MaxFileSizeBytes).WithMessage($"File size cannot exceed {MaxFileSizeBytes / 1024 / 1024}MB.");

        RuleFor(c => c.FileStream)
            .NotNull().WithMessage("File stream is required.");

        RuleFor(c => c.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }

    private static bool HaveValidExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }
}
