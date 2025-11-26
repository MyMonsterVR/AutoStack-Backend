using System.Text.Json.Serialization;

namespace AutoStack.Application.DTOs.Stacks;

/// <summary>
/// Enum representing the order direction for sorting
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SortingOrderResponse
{
    /// <summary>
    /// Sort in ascending order (lowest to highest)
    /// </summary>
    Ascending,

    /// <summary>
    /// Sort in descending order (highest to lowest)
    /// </summary>
    Descending
}