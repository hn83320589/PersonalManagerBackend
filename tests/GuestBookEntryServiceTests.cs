using Moq;
using PersonalManager.Api.Models;
using PersonalManager.Api.Repositories;
using PersonalManager.Api.Services;

namespace PersonalManager.Tests;

public class GuestBookEntryServiceTests
{
    private readonly Mock<IRepository<GuestBookEntry>> _repo;
    private readonly GuestBookEntryService _sut;

    public GuestBookEntryServiceTests()
    {
        _repo = new Mock<IRepository<GuestBookEntry>>();
        _sut = new GuestBookEntryService(_repo.Object);
    }

    private static GuestBookEntry MakeEntry(int id, int targetUserId = 1, bool isApproved = true) => new()
    {
        Id = id,
        TargetUserId = targetUserId,
        Name = $"Guest {id}",
        Email = $"guest{id}@test.com",
        Message = $"Hello from guest {id}",
        IsApproved = isApproved,
        AdminReply = string.Empty,
        CreatedAt = DateTime.UtcNow.AddDays(-id),
        UpdatedAt = DateTime.UtcNow
    };

    // ===== GetApprovedByTargetUserIdAsync =====

    [Fact]
    public async Task GetApprovedByTargetUserIdAsync_ReturnsOnlyApprovedEntriesForTargetUser()
    {
        var entries = new List<GuestBookEntry>
        {
            MakeEntry(1, targetUserId: 1, isApproved: true),
            MakeEntry(2, targetUserId: 1, isApproved: false), // not approved
            MakeEntry(3, targetUserId: 2, isApproved: true),  // different user
        };
        _repo.Setup(r => r.FindAsync(It.IsAny<Func<GuestBookEntry, bool>>()))
            .ReturnsAsync((Func<GuestBookEntry, bool> pred) => entries.Where(pred).ToList());

        var result = await _sut.GetApprovedByTargetUserIdAsync(1);

        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public async Task GetApprovedByTargetUserIdAsync_NoEntries_ReturnsEmptyList()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Func<GuestBookEntry, bool>>()))
            .ReturnsAsync([]);

        var result = await _sut.GetApprovedByTargetUserIdAsync(1);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetApprovedByTargetUserIdAsync_OrdersByCreatedAtDescending()
    {
        var entries = new List<GuestBookEntry>
        {
            MakeEntry(1, targetUserId: 1, isApproved: true), // CreatedAt = now - 1 day
            MakeEntry(2, targetUserId: 1, isApproved: true), // CreatedAt = now - 2 days
            MakeEntry(3, targetUserId: 1, isApproved: true), // CreatedAt = now - 3 days
        };
        _repo.Setup(r => r.FindAsync(It.IsAny<Func<GuestBookEntry, bool>>()))
            .ReturnsAsync((Func<GuestBookEntry, bool> pred) => entries.Where(pred).ToList());

        var result = await _sut.GetApprovedByTargetUserIdAsync(1);

        Assert.Equal(1, result[0].Id); // most recent first
        Assert.Equal(3, result[2].Id); // oldest last
    }

    // ===== GetApprovedPagedAsync =====

    [Fact]
    public async Task GetApprovedPagedAsync_ReturnsPaginatedResult()
    {
        var entries = Enumerable.Range(1, 15)
            .Select(i => MakeEntry(i, targetUserId: 1, isApproved: true))
            .ToList();
        _repo.Setup(r => r.FindAsync(It.IsAny<Func<GuestBookEntry, bool>>()))
            .ReturnsAsync((Func<GuestBookEntry, bool> pred) => entries.Where(pred).ToList());

        var result = await _sut.GetApprovedPagedAsync(1, page: 1, pageSize: 10);

        Assert.Equal(10, result.Items.Count);
        Assert.Equal(15, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public async Task GetApprovedPagedAsync_LastPage_HasNoPreviousPage()
    {
        var entries = Enumerable.Range(1, 5)
            .Select(i => MakeEntry(i, targetUserId: 1, isApproved: true))
            .ToList();
        _repo.Setup(r => r.FindAsync(It.IsAny<Func<GuestBookEntry, bool>>()))
            .ReturnsAsync((Func<GuestBookEntry, bool> pred) => entries.Where(pred).ToList());

        var result = await _sut.GetApprovedPagedAsync(1, page: 2, pageSize: 3);

        Assert.Equal(2, result.Items.Count); // 5 total, page 2, pageSize 3 → 2 items
        Assert.True(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public async Task GetApprovedPagedAsync_ExcludesNonApprovedEntries()
    {
        var entries = new List<GuestBookEntry>
        {
            MakeEntry(1, targetUserId: 1, isApproved: true),
            MakeEntry(2, targetUserId: 1, isApproved: false),
            MakeEntry(3, targetUserId: 1, isApproved: false),
        };
        _repo.Setup(r => r.FindAsync(It.IsAny<Func<GuestBookEntry, bool>>()))
            .ReturnsAsync((Func<GuestBookEntry, bool> pred) => entries.Where(pred).ToList());

        var result = await _sut.GetApprovedPagedAsync(1, page: 1, pageSize: 10);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetApprovedPagedAsync_EmptyRepository_ReturnsEmptyPage()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Func<GuestBookEntry, bool>>()))
            .ReturnsAsync([]);

        var result = await _sut.GetApprovedPagedAsync(1, page: 1, pageSize: 10);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
    }
}
