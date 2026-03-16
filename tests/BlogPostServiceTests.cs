using Moq;
using PersonalManager.Api.Models;
using PersonalManager.Api.Repositories;
using PersonalManager.Api.Services;

namespace PersonalManager.Tests;

public class BlogPostServiceTests
{
    private readonly Mock<IRepository<BlogPost>> _repo;
    private readonly BlogPostService _sut;

    public BlogPostServiceTests()
    {
        _repo = new Mock<IRepository<BlogPost>>();
        _sut = new BlogPostService(_repo.Object);
    }

    private static BlogPost MakePost(int id, int userId = 1, string status = "Published", bool isPublic = true,
        string title = "Test Post", string slug = "test-post", string tags = "", string category = "") => new()
    {
        Id = id,
        UserId = userId,
        Title = title,
        Slug = slug,
        Content = "Content here",
        Summary = "Summary",
        Tags = tags,
        Category = category,
        Status = status == "Published" ? BlogPostStatus.Published : BlogPostStatus.Draft,
        IsPublic = isPublic,
        ViewCount = 0,
        PublishedAt = status == "Published" ? DateTime.UtcNow.AddDays(-id) : null,
        CreatedAt = DateTime.UtcNow.AddDays(-id),
        UpdatedAt = DateTime.UtcNow
    };

    // ===== GetPublicByUserIdAsync =====

    [Fact]
    public async Task GetPublicByUserIdAsync_ReturnsOnlyPublishedAndPublicPosts()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Func<BlogPost, bool>>()))
            .ReturnsAsync((Func<BlogPost, bool> pred) =>
                new List<BlogPost>
                {
                    MakePost(1, userId: 1, status: "Published", isPublic: true),
                    MakePost(2, userId: 1, status: "Draft",     isPublic: true),
                    MakePost(3, userId: 1, status: "Published", isPublic: false),
                }.Where(pred).ToList());

        var result = await _sut.GetPublicByUserIdAsync(1);

        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public async Task GetPublicByUserIdAsync_EmptyRepository_ReturnsEmptyList()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Func<BlogPost, bool>>()))
            .ReturnsAsync([]);

        var result = await _sut.GetPublicByUserIdAsync(1);

        Assert.Empty(result);
    }

    // ===== GetBySlugAsync =====

    [Fact]
    public async Task GetBySlugAsync_ExistingSlug_ReturnsPost()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Func<BlogPost, bool>>()))
            .ReturnsAsync((Func<BlogPost, bool> pred) =>
                new List<BlogPost> { MakePost(5, slug: "my-post") }.Where(pred).ToList());

        var result = await _sut.GetBySlugAsync("my-post");

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
    }

    [Fact]
    public async Task GetBySlugAsync_NonExistingSlug_ReturnsNull()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Func<BlogPost, bool>>()))
            .ReturnsAsync([]);

        var result = await _sut.GetBySlugAsync("nonexistent-slug");

        Assert.Null(result);
    }

    // ===== GetPublicPagedAsync =====

    [Fact]
    public async Task GetPublicPagedAsync_ReturnsPaginatedResult()
    {
        var posts = Enumerable.Range(1, 12)
            .Select(i => MakePost(i, userId: 1, status: "Published", isPublic: true))
            .ToList();
        _repo.Setup(r => r.FindAsync(It.IsAny<Func<BlogPost, bool>>()))
            .ReturnsAsync((Func<BlogPost, bool> pred) => posts.Where(pred).ToList());

        var result = await _sut.GetPublicPagedAsync(1, page: 1, pageSize: 5, null, null, null);

        Assert.Equal(5, result.Items.Count);
        Assert.Equal(12, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public async Task GetPublicPagedAsync_Page2_SkipsFirstPage()
    {
        var posts = Enumerable.Range(1, 10)
            .Select(i => MakePost(i, userId: 1, status: "Published", isPublic: true))
            .ToList();
        _repo.Setup(r => r.FindAsync(It.IsAny<Func<BlogPost, bool>>()))
            .ReturnsAsync((Func<BlogPost, bool> pred) => posts.Where(pred).ToList());

        var result = await _sut.GetPublicPagedAsync(1, page: 2, pageSize: 4, null, null, null);

        Assert.Equal(4, result.Items.Count);
        Assert.Equal(10, result.TotalCount);
        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public async Task GetPublicPagedAsync_WithKeyword_FiltersResults()
    {
        var posts = new List<BlogPost>
        {
            MakePost(1, title: "Vue Tips",     status: "Published", isPublic: true),
            MakePost(2, title: "React Guide",  status: "Published", isPublic: true),
            MakePost(3, title: "Vue Advanced", status: "Published", isPublic: true),
        };
        _repo.Setup(r => r.FindAsync(It.IsAny<Func<BlogPost, bool>>()))
            .ReturnsAsync((Func<BlogPost, bool> pred) => posts.Where(pred).ToList());

        var result = await _sut.GetPublicPagedAsync(1, page: 1, pageSize: 10, keyword: "vue", null, null);

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, item => Assert.Contains("vue", item.Title.ToLower()));
    }

    [Fact]
    public async Task GetPublicPagedAsync_WithCategory_FiltersResults()
    {
        var posts = new List<BlogPost>
        {
            MakePost(1, category: "Tech",  status: "Published", isPublic: true),
            MakePost(2, category: "Life",  status: "Published", isPublic: true),
            MakePost(3, category: "Tech",  status: "Published", isPublic: true),
        };
        _repo.Setup(r => r.FindAsync(It.IsAny<Func<BlogPost, bool>>()))
            .ReturnsAsync((Func<BlogPost, bool> pred) => posts.Where(pred).ToList());

        var result = await _sut.GetPublicPagedAsync(1, page: 1, pageSize: 10, null, null, category: "Tech");

        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task GetPublicPagedAsync_WithTag_FiltersResults()
    {
        var posts = new List<BlogPost>
        {
            MakePost(1, tags: "vue,typescript", status: "Published", isPublic: true),
            MakePost(2, tags: "react,javascript", status: "Published", isPublic: true),
            MakePost(3, tags: "vue,css", status: "Published", isPublic: true),
        };
        _repo.Setup(r => r.FindAsync(It.IsAny<Func<BlogPost, bool>>()))
            .ReturnsAsync((Func<BlogPost, bool> pred) => posts.Where(pred).ToList());

        var result = await _sut.GetPublicPagedAsync(1, page: 1, pageSize: 10, null, tag: "vue", null);

        Assert.Equal(2, result.TotalCount);
    }

    // ===== IncrementViewCountAsync =====

    [Fact]
    public async Task IncrementViewCountAsync_ExistingPost_IncrementsCount()
    {
        var post = MakePost(1, status: "Published");
        post.ViewCount = 5;
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(post);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<BlogPost>()))
            .ReturnsAsync((BlogPost p) => p);

        await _sut.IncrementViewCountAsync(1);

        Assert.Equal(6, post.ViewCount);
        _repo.Verify(r => r.UpdateAsync(post), Times.Once);
    }

    [Fact]
    public async Task IncrementViewCountAsync_NonExistingPost_DoesNotThrow()
    {
        _repo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((BlogPost?)null);

        // Should not throw
        await _sut.IncrementViewCountAsync(999);

        _repo.Verify(r => r.UpdateAsync(It.IsAny<BlogPost>()), Times.Never);
    }
}
