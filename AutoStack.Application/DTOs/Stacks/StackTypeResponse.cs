using System.Text.Json.Serialization;

namespace AutoStack.Application.DTOs.Stacks;

/// <summary>
/// Enum representing the type of technology stack
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StackTypeResponse
{
    /// <summary>
    /// Frontend stack (e.g., UI frameworks and libraries)
    /// </summary>
    FRONTEND,

    /// <summary>
    /// Backend stack (e.g., server-side frameworks and databases)
    /// </summary>
    BACKEND,

    /// <summary>
    /// Full-stack combining both frontend and backend technologies
    /// </summary>
    FULLSTACK
}
