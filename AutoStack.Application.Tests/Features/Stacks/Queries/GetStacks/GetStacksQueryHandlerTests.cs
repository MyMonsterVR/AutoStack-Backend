using AutoStack.Application.DTOs.Stacks;
using AutoStack.Application.Features.Stacks.Queries.GetStacks;
using AutoStack.Application.Tests.Builders;
using AutoStack.Application.Tests.Common;
using AutoStack.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoStack.Application.Tests.Features.Stacks.Queries.GetStacks;

public class GetStacksQueryHandlerTests : QueryHandlerTestBase
{
    private readonly GetStacksQueryHandler _handler;

    public GetStacksQueryHandlerTests()
    {
        _handler = new GetStacksQueryHandler(MockStackRepository.Object);
    }

    [Fact]
    public async Task Handle_WithNoFilter_ShouldReturnAllStacks()
    {
        var stacks = new List<Stack>
        {
            new StackBuilder().WithName("Stack 1").Build(),
            new StackBuilder().WithName("Stack 2").Build(),
            new StackBuilder().WithName("Stack 3").Build()
        };

        MockStackRepository.Setup(r => r.GetStacksPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((stacks, stacks.Count));

        var query = new GetStacksQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(3);
        result.Value.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithStackTypeFilter_ShouldFilterByType()
    {
        var frontendStacks = new List<Stack>
        {
            new StackBuilder().WithName("React Stack").WithType("FRONTEND").Build(),
            new StackBuilder().WithName("Vue Stack").WithType("FRONTEND").Build()
        };

        MockStackRepository.Setup(r => r.GetStacksPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                "FRONTEND",
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((frontendStacks, 2));

        var query = new GetStacksQuery(StackType: StackTypeResponse.FRONTEND);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.Should().AllSatisfy(s => s.Type.Should().Be(StackTypeResponse.FRONTEND));
    }

    [Fact]
    public async Task Handle_WithPopularitySortAscending_ShouldSortCorrectly()
    {
        var stack1 = new StackBuilder().WithName("Stack 1").Build();
        stack1.IncrementDownloads();

        var stack2 = new StackBuilder().WithName("Stack 2").Build();
        stack2.IncrementDownloads();
        stack2.IncrementDownloads();
        stack2.IncrementDownloads();

        var stack3 = new StackBuilder().WithName("Stack 3").Build();
        stack3.IncrementDownloads();
        stack3.IncrementDownloads();

        // Repository returns sorted ascending by downloads
        var sortedStacks = new List<Stack> { stack1, stack3, stack2 };

        MockStackRepository.Setup(r => r.GetStacksPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                "Popularity",
                false, // ascending
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((sortedStacks, 3));

        var query = new GetStacksQuery(
            StackSortByResponse: StackSortByResponse.Popularity,
            SortingOrderResponse: SortingOrderResponse.Ascending
        );

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(3);
        result.Value.Items.First().Downloads.Should().Be(1);
        result.Value.Items.Last().Downloads.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithPopularitySortDescending_ShouldSortCorrectly()
    {
        var stack1 = new StackBuilder().WithName("Stack 1").Build();
        stack1.IncrementDownloads();

        var stack2 = new StackBuilder().WithName("Stack 2").Build();
        stack2.IncrementDownloads();
        stack2.IncrementDownloads();
        stack2.IncrementDownloads();

        var stack3 = new StackBuilder().WithName("Stack 3").Build();
        stack3.IncrementDownloads();
        stack3.IncrementDownloads();

        // Repository returns sorted descending by downloads
        var sortedStacks = new List<Stack> { stack2, stack3, stack1 };

        MockStackRepository.Setup(r => r.GetStacksPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                "Popularity",
                true, // descending
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((sortedStacks, 3));

        var query = new GetStacksQuery(
            StackSortByResponse: StackSortByResponse.Popularity,
            SortingOrderResponse: SortingOrderResponse.Descending
        );

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(3);
        result.Value.Items.First().Downloads.Should().Be(3);
        result.Value.Items.Last().Downloads.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        var page2Stacks = new List<Stack>();
        for (int i = 11; i <= 20; i++)
        {
            page2Stacks.Add(new StackBuilder().WithName($"Stack {i}").Build());
        }

        MockStackRepository.Setup(r => r.GetStacksPagedAsync(
                2,
                10,
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((page2Stacks, 25));

        var query = new GetStacksQuery(PageNumber: 2, PageSize: 10);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(10);
        result.Value.TotalCount.Should().Be(25);
        result.Value.PageNumber.Should().Be(2);
        result.Value.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldCalculateTotalPages()
    {
        var page1Stacks = new List<Stack>();
        for (int i = 1; i <= 10; i++)
        {
            page1Stacks.Add(new StackBuilder().WithName($"Stack {i}").Build());
        }

        MockStackRepository.Setup(r => r.GetStacksPagedAsync(
                1,
                10,
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((page1Stacks, 25));

        var query = new GetStacksQuery(PageNumber: 1, PageSize: 10);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithLastPage_ShouldReturnRemainingItems()
    {
        var page3Stacks = new List<Stack>();
        for (int i = 21; i <= 25; i++)
        {
            page3Stacks.Add(new StackBuilder().WithName($"Stack {i}").Build());
        }

        MockStackRepository.Setup(r => r.GetStacksPagedAsync(
                3,
                10,
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((page3Stacks, 25));

        var query = new GetStacksQuery(PageNumber: 3, PageSize: 10);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(5);
        result.Value.TotalCount.Should().Be(25);
    }

    [Fact]
    public async Task Handle_ShouldMapToStackResponse()
    {
        var stack = new StackBuilder()
            .WithName("React Stack")
            .WithDescription("A React stack")
            .WithType("FRONTEND")
            .Build();

        MockStackRepository.Setup(r => r.GetStacksPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Stack> { stack }, 1));

        var query = new GetStacksQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var stackResponse = result.Value.Items.First();
        stackResponse.Id.Should().Be(stack.Id);
        stackResponse.Name.Should().Be(stack.Name);
        stackResponse.Description.Should().Be(stack.Description);
        stackResponse.Type.Should().Be(StackTypeResponse.FRONTEND);
        stackResponse.Downloads.Should().Be(stack.Downloads);
    }

    [Fact]
    public async Task Handle_WithEmptyResult_ShouldReturnEmptyList()
    {
        MockStackRepository.Setup(r => r.GetStacksPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Stack>(), 0));

        var query = new GetStacksQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithFilterAndPagination_ShouldApplyBoth()
    {
        var page2FrontendStacks = new List<Stack>();
        for (int i = 6; i <= 10; i++)
        {
            page2FrontendStacks.Add(new StackBuilder().WithName($"Frontend {i}").WithType("FRONTEND").Build());
        }

        MockStackRepository.Setup(r => r.GetStacksPagedAsync(
                2,
                5,
                "FRONTEND",
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((page2FrontendStacks, 15));

        var query = new GetStacksQuery(
            StackType: StackTypeResponse.FRONTEND,
            PageNumber: 2,
            PageSize: 5
        );

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(5);
        result.Value.TotalCount.Should().Be(15);
        result.Value.Items.Should().AllSatisfy(s => s.Type.Should().Be(StackTypeResponse.FRONTEND));
    }
}
