using System.Text.Json.Serialization;

namespace AutoStack.Application.Common.Models;

/// <summary>
/// Represents a paginated response containing items and pagination metadata
/// </summary>
/// <typeparam name="T">The type of items in the response</typeparam>
public class PagedResponse<T>
{
    /// <summary>
    /// Gets or sets the items in the current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// Gets or sets the total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the current page number (1-based)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets the total number of pages
    /// </summary>
    [JsonInclude]
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Gets whether there is a previous page available
    /// </summary>
    [JsonInclude]
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Gets whether there is a next page available
    /// </summary>
    [JsonInclude]
    public bool HasNextPage => PageNumber < TotalPages;
}