using System.Text.Json.Serialization;

namespace AutoStack.Application.DTOs.Stacks;

/// <summary>
/// Enum representing the field to sort stacks by
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StackSortByResponse
{
    /// <summary>
    /// Sort by popularity (number of downloads)
    /// </summary>
    Popularity,

    /// <summary>
    /// Sort by rating
    /// </summary>
    Rating,

    /// <summary>
    /// Sort by created date
    /// </summary>
    CreatedDate,
}