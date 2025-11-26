using AutoStack.Application.Common.Models;
using FluentAssertions;
using Xunit;

namespace AutoStack.Application.Tests.Common.Models;

public class PagedResponseTests
{
    [Fact]
    public void TotalPages_ShouldCalculateCorrectly()
    {
        // Arrange
        var pagedResponse = new PagedResponse<string>
        {
            Items = new List<string> { "item1", "item2", "item3" },
            TotalCount = 30,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var totalPages = pagedResponse.TotalPages;

        // Assert
        totalPages.Should().Be(3);
    }

    [Fact]
    public void TotalPages_WithPartialPage_ShouldRoundUp()
    {
        // Arrange
        var pagedResponse = new PagedResponse<string>
        {
            Items = new List<string>(),
            TotalCount = 25,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var totalPages = pagedResponse.TotalPages;

        // Assert
        totalPages.Should().Be(3); // 25 items with page size 10 = 3 pages
    }

    [Fact]
    public void TotalPages_WithExactDivision_ShouldNotRoundUp()
    {
        // Arrange
        var pagedResponse = new PagedResponse<string>
        {
            Items = new List<string>(),
            TotalCount = 20,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var totalPages = pagedResponse.TotalPages;

        // Assert
        totalPages.Should().Be(2);
    }

    [Fact]
    public void HasPreviousPage_OnFirstPage_ShouldBeFalse()
    {
        // Arrange
        var pagedResponse = new PagedResponse<string>
        {
            Items = new List<string>(),
            TotalCount = 30,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var hasPreviousPage = pagedResponse.HasPreviousPage;

        // Assert
        hasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_OnSecondPage_ShouldBeTrue()
    {
        // Arrange
        var pagedResponse = new PagedResponse<string>
        {
            Items = new List<string>(),
            TotalCount = 30,
            PageNumber = 2,
            PageSize = 10
        };

        // Act
        var hasPreviousPage = pagedResponse.HasPreviousPage;

        // Assert
        hasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_OnLastPage_ShouldBeFalse()
    {
        // Arrange
        var pagedResponse = new PagedResponse<string>
        {
            Items = new List<string>(),
            TotalCount = 30,
            PageNumber = 3,
            PageSize = 10
        };

        // Act
        var hasNextPage = pagedResponse.HasNextPage;

        // Assert
        hasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_OnFirstPage_ShouldBeTrue()
    {
        // Arrange
        var pagedResponse = new PagedResponse<string>
        {
            Items = new List<string>(),
            TotalCount = 30,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var hasNextPage = pagedResponse.HasNextPage;

        // Assert
        hasNextPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_OnMiddlePage_ShouldBeTrue()
    {
        // Arrange
        var pagedResponse = new PagedResponse<string>
        {
            Items = new List<string>(),
            TotalCount = 30,
            PageNumber = 2,
            PageSize = 10
        };

        // Act
        var hasNextPage = pagedResponse.HasNextPage;

        // Assert
        hasNextPage.Should().BeTrue();
    }

    [Fact]
    public void TotalPages_WithZeroItems_ShouldReturnZero()
    {
        // Arrange
        var pagedResponse = new PagedResponse<string>
        {
            Items = new List<string>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var totalPages = pagedResponse.TotalPages;

        // Assert
        totalPages.Should().Be(0);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };

        // Act
        var pagedResponse = new PagedResponse<int>
        {
            Items = items,
            TotalCount = 100,
            PageNumber = 5,
            PageSize = 20
        };

        // Assert
        pagedResponse.Items.Should().BeEquivalentTo(items);
        pagedResponse.TotalCount.Should().Be(100);
        pagedResponse.PageNumber.Should().Be(5);
        pagedResponse.PageSize.Should().Be(20);
    }
}
