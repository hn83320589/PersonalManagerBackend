using Microsoft.EntityFrameworkCore;
using PersonalManager.Api.Data;
using PersonalManager.Api.Models;

namespace PersonalManager.Api.Repositories;

public class BlogPostRepository : EfRepository<BlogPost>
{
    private readonly ApplicationDbContext _db;

    public BlogPostRepository(ApplicationDbContext context) : base(context)
    {
        _db = context;
    }

    public override async Task<List<BlogPost>> GetAllAsync()
        => await _db.BlogPosts.Include(b => b.TagEntities).ToListAsync();

    public override async Task<BlogPost?> GetByIdAsync(int id)
        => await _db.BlogPosts.Include(b => b.TagEntities).FirstOrDefaultAsync(b => b.Id == id);

    public override Task<List<BlogPost>> FindAsync(Func<BlogPost, bool> predicate)
        => Task.FromResult(_db.BlogPosts.Include(b => b.TagEntities).AsEnumerable().Where(predicate).ToList());

    /// <summary>
    /// Sync tag associations for a blog post: find or create Tag entities and replace current associations.
    /// </summary>
    public async Task SyncTagsAsync(BlogPost post, List<string> tagNames)
    {
        var trimmedNames = tagNames
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim())
            .ToList();

        // Load the post with its current TagEntities
        var loaded = await _db.BlogPosts.Include(b => b.TagEntities).FirstOrDefaultAsync(b => b.Id == post.Id);
        if (loaded == null) return;

        // Find existing tags for this user
        var existingTags = await _db.Tags
            .Where(t => t.UserId == post.UserId && trimmedNames.Contains(t.Name))
            .ToListAsync();

        var existingNames = existingTags.Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Create new tags for names that don't exist yet
        foreach (var name in trimmedNames.Where(n => !existingNames.Contains(n)))
        {
            var tag = new Tag { UserId = post.UserId, Name = name };
            _db.Tags.Add(tag);
            existingTags.Add(tag);
        }

        // Replace all tag associations
        loaded.TagEntities.Clear();
        foreach (var tag in existingTags.Where(t => trimmedNames.Any(n => n.Equals(t.Name, StringComparison.OrdinalIgnoreCase))))
            loaded.TagEntities.Add(tag);

        await _db.SaveChangesAsync();
    }
}
