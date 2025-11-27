using AutoStack.Domain.Common;

namespace AutoStack.Domain.Entities;

/// <summary>
/// Represents the CLI version configuration
/// </summary>
public class CliVersion : Entity<int>
{
    /// <summary>
    /// Gets the version string
    /// </summary>
    public string Version { get; private set; } = string.Empty;

    public CliVersion()
    {
    }

    private CliVersion(int id, string version) : base(id)
    {
        Version = version;
    }

    /// <summary>
    /// Creates a new CLI version configuration
    /// </summary>
    /// <param name="version">The version string (e.g., "1.0.0")</param>
    /// <returns>A new CliVersion instance</returns>
    /// <exception cref="ArgumentException">Thrown when version is null or empty</exception>
    public static CliVersion Create(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException("Version cannot be null or empty", nameof(version));
        }

        // Always use ID 1 for the single version record
        return new CliVersion(1, version);
    }

    /// <summary>
    /// Updates the version string
    /// </summary>
    /// <param name="version">The new version string</param>
    /// <exception cref="ArgumentException">Thrown when version is null or empty</exception>
    public void UpdateVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException("Version cannot be null or empty", nameof(version));
        }

        Version = version;
        UpdateTimestamp();
    }
}
