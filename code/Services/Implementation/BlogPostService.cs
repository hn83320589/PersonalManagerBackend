using AutoMapper;
using PersonalManagerAPI.DTOs.BlogPost;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services.Interfaces;
using System.Text.RegularExpressions;
using System.Globalization;

namespace PersonalManagerAPI.Services.Implementation;

/// <summary>
/// 部落格文章服務實作
/// </summary>
public class BlogPostService : IBlogPostService
{
    private readonly JsonDataService _jsonDataService;
    private readonly IMapper _mapper;

    public BlogPostService(JsonDataService jsonDataService, IMapper mapper)
    {
        _jsonDataService = jsonDataService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BlogPostResponseDto>> GetAllBlogPostsAsync(int skip = 0, int take = 50)
    {
        var blogPosts = await _jsonDataService.GetAllAsync<BlogPost>();
        return blogPosts
            .OrderByDescending(b => b.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(b => _mapper.Map<BlogPostResponseDto>(b));
    }

    public async Task<BlogPostResponseDto?> GetBlogPostByIdAsync(int id)
    {
        var blogPost = await _jsonDataService.GetByIdAsync<BlogPost>(id);
        return blogPost != null ? _mapper.Map<BlogPostResponseDto>(blogPost) : null;
    }

    public async Task<BlogPostResponseDto?> GetBlogPostBySlugAsync(string slug)
    {
        var blogPosts = await _jsonDataService.GetAllAsync<BlogPost>();
        var blogPost = blogPosts.FirstOrDefault(b => b.Slug == slug);
        return blogPost != null ? _mapper.Map<BlogPostResponseDto>(blogPost) : null;
    }

    public async Task<IEnumerable<BlogPostResponseDto>> GetBlogPostsByUserIdAsync(int userId)
    {
        var blogPosts = await _jsonDataService.GetAllAsync<BlogPost>();
        return blogPosts
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => _mapper.Map<BlogPostResponseDto>(b));
    }

    public async Task<IEnumerable<BlogPostResponseDto>> GetPublishedBlogPostsAsync(int skip = 0, int take = 50)
    {
        var blogPosts = await _jsonDataService.GetAllAsync<BlogPost>();
        return blogPosts
            .Where(b => b.IsPublished && b.IsPublic)
            .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(b => _mapper.Map<BlogPostResponseDto>(b));
    }

    public async Task<IEnumerable<BlogPostResponseDto>> GetDraftBlogPostsAsync(int userId)
    {
        var blogPosts = await _jsonDataService.GetAllAsync<BlogPost>();
        return blogPosts
            .Where(b => b.UserId == userId && !b.IsPublished)
            .OrderByDescending(b => b.UpdatedAt)
            .Select(b => _mapper.Map<BlogPostResponseDto>(b));
    }

    public async Task<IEnumerable<BlogPostResponseDto>> GetBlogPostsByCategoryAsync(string category, bool publicOnly = true)
    {
        var blogPosts = await _jsonDataService.GetAllAsync<BlogPost>();
        var filtered = blogPosts.Where(b => 
            !string.IsNullOrEmpty(b.Category) && 
            b.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

        if (publicOnly)
        {
            filtered = filtered.Where(b => b.IsPublished && b.IsPublic);
        }

        return filtered
            .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
            .Select(b => _mapper.Map<BlogPostResponseDto>(b));
    }

    public async Task<IEnumerable<BlogPostResponseDto>> SearchBlogPostsByTagsAsync(string tags, bool publicOnly = true)
    {
        var blogPosts = await _jsonDataService.GetAllAsync<BlogPost>();
        var searchTags = tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim().ToLower())
                            .ToList();

        var filtered = blogPosts.Where(b =>
        {
            if (string.IsNullOrEmpty(b.Tags)) return false;
            
            var blogTags = b.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(t => t.Trim().ToLower());
            
            return searchTags.Any(searchTag => blogTags.Contains(searchTag));
        });

        if (publicOnly)
        {
            filtered = filtered.Where(b => b.IsPublished && b.IsPublic);
        }

        return filtered
            .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
            .Select(b => _mapper.Map<BlogPostResponseDto>(b));
    }

    public async Task<IEnumerable<BlogPostResponseDto>> SearchBlogPostsAsync(string keyword, bool publicOnly = true)
    {
        var blogPosts = await _jsonDataService.GetAllAsync<BlogPost>();
        var lowerKeyword = keyword.ToLower();

        var filtered = blogPosts.Where(b =>
            b.Title.ToLower().Contains(lowerKeyword) ||
            b.Content.ToLower().Contains(lowerKeyword) ||
            (!string.IsNullOrEmpty(b.Summary) && b.Summary.ToLower().Contains(lowerKeyword)) ||
            (!string.IsNullOrEmpty(b.Tags) && b.Tags.ToLower().Contains(lowerKeyword)));

        if (publicOnly)
        {
            filtered = filtered.Where(b => b.IsPublished && b.IsPublic);
        }

        return filtered
            .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
            .Select(b => _mapper.Map<BlogPostResponseDto>(b));
    }

    public async Task<IEnumerable<BlogPostResponseDto>> GetPopularBlogPostsAsync(int count = 10, bool publicOnly = true)
    {
        var blogPosts = await _jsonDataService.GetAllAsync<BlogPost>();
        var filtered = blogPosts.AsEnumerable();

        if (publicOnly)
        {
            filtered = filtered.Where(b => b.IsPublished && b.IsPublic);
        }

        return filtered
            .OrderByDescending(b => b.ViewCount)
            .ThenByDescending(b => b.PublishedAt ?? b.CreatedAt)
            .Take(count)
            .Select(b => _mapper.Map<BlogPostResponseDto>(b));
    }

    public async Task<IEnumerable<BlogPostResponseDto>> GetLatestBlogPostsAsync(int count = 10, bool publicOnly = true)
    {
        var blogPosts = await _jsonDataService.GetAllAsync<BlogPost>();
        var filtered = blogPosts.AsEnumerable();

        if (publicOnly)
        {
            filtered = filtered.Where(b => b.IsPublished && b.IsPublic);
        }

        return filtered
            .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
            .Take(count)
            .Select(b => _mapper.Map<BlogPostResponseDto>(b));
    }

    public async Task<IEnumerable<BlogPostResponseDto>> GetRelatedBlogPostsAsync(int blogPostId, int count = 5)
    {
        var blogPosts = await _jsonDataService.GetAllAsync<BlogPost>();
        var targetPost = blogPosts.FirstOrDefault(b => b.Id == blogPostId);
        
        if (targetPost == null) return Enumerable.Empty<BlogPostResponseDto>();

        // 根據分類和標籤尋找相關文章
        var relatedPosts = blogPosts
            .Where(b => b.Id != blogPostId && b.IsPublished && b.IsPublic)
            .Where(b => 
                (!string.IsNullOrEmpty(targetPost.Category) && b.Category == targetPost.Category) ||
                (!string.IsNullOrEmpty(targetPost.Tags) && !string.IsNullOrEmpty(b.Tags) && 
                 HasCommonTags(targetPost.Tags, b.Tags)))
            .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
            .Take(count);

        return relatedPosts.Select(b => _mapper.Map<BlogPostResponseDto>(b));
    }

    public async Task<IEnumerable<BlogPostResponseDto>> GetBlogPostsByDateRangeAsync(DateTime startDate, DateTime endDate, bool publicOnly = true)
    {
        var blogPosts = await _jsonDataService.GetAllAsync<BlogPost>();
        var filtered = blogPosts.Where(b =>
        {
            var postDate = b.PublishedAt ?? b.CreatedAt;
            return postDate >= startDate && postDate <= endDate;
        });

        if (publicOnly)
        {
            filtered = filtered.Where(b => b.IsPublished && b.IsPublic);
        }

        return filtered
            .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
            .Select(b => _mapper.Map<BlogPostResponseDto>(b));
    }

    public async Task<BlogPostResponseDto> CreateBlogPostAsync(CreateBlogPostDto createBlogPostDto)
    {
        var blogPost = _mapper.Map<BlogPost>(createBlogPostDto);
        blogPost.CreatedAt = DateTime.UtcNow;
        blogPost.UpdatedAt = DateTime.UtcNow;

        // 自動生成 Slug 如果沒有提供
        if (string.IsNullOrEmpty(blogPost.Slug))
        {
            blogPost.Slug = await GenerateSlugAsync(blogPost.Title);
        }
        else
        {
            // 確保提供的 Slug 是唯一的
            if (!await IsSlugUniqueAsync(blogPost.Slug))
            {
                blogPost.Slug = await GenerateSlugAsync(blogPost.Title);
            }
        }

        // 如果設為發布，設定發布時間
        if (blogPost.IsPublished && blogPost.PublishedAt == null)
        {
            blogPost.PublishedAt = DateTime.UtcNow;
            blogPost.PublishedDate = blogPost.PublishedAt;
        }

        var createdBlogPost = await _jsonDataService.CreateAsync(blogPost);
        return _mapper.Map<BlogPostResponseDto>(createdBlogPost);
    }

    public async Task<BlogPostResponseDto?> UpdateBlogPostAsync(int id, UpdateBlogPostDto updateBlogPostDto)
    {
        var existingBlogPost = await _jsonDataService.GetByIdAsync<BlogPost>(id);
        if (existingBlogPost == null) return null;

        // 儲存原始發布狀態
        var wasPublished = existingBlogPost.IsPublished;

        _mapper.Map(updateBlogPostDto, existingBlogPost);
        existingBlogPost.UpdatedAt = DateTime.UtcNow;

        // 處理 Slug 變更
        if (!string.IsNullOrEmpty(updateBlogPostDto.Slug) && 
            updateBlogPostDto.Slug != existingBlogPost.Slug)
        {
            if (!await IsSlugUniqueAsync(updateBlogPostDto.Slug, id))
            {
                existingBlogPost.Slug = await GenerateSlugAsync(existingBlogPost.Title, id);
            }
        }

        // 處理發布狀態變更
        if (!wasPublished && existingBlogPost.IsPublished && existingBlogPost.PublishedAt == null)
        {
            existingBlogPost.PublishedAt = DateTime.UtcNow;
            existingBlogPost.PublishedDate = existingBlogPost.PublishedAt;
        }

        var updatedBlogPost = await _jsonDataService.UpdateAsync(existingBlogPost);
        return _mapper.Map<BlogPostResponseDto>(updatedBlogPost);
    }

    public async Task<bool> PublishBlogPostAsync(int id)
    {
        var blogPost = await _jsonDataService.GetByIdAsync<BlogPost>(id);
        if (blogPost == null) return false;

        blogPost.IsPublished = true;
        if (blogPost.PublishedAt == null)
        {
            blogPost.PublishedAt = DateTime.UtcNow;
            blogPost.PublishedDate = blogPost.PublishedAt;
        }
        blogPost.UpdatedAt = DateTime.UtcNow;

        await _jsonDataService.UpdateAsync(blogPost);
        return true;
    }

    public async Task<bool> UnpublishBlogPostAsync(int id)
    {
        var blogPost = await _jsonDataService.GetByIdAsync<BlogPost>(id);
        if (blogPost == null) return false;

        blogPost.IsPublished = false;
        blogPost.UpdatedAt = DateTime.UtcNow;

        await _jsonDataService.UpdateAsync(blogPost);
        return true;
    }

    public async Task<bool> IncrementViewCountAsync(int id)
    {
        var blogPost = await _jsonDataService.GetByIdAsync<BlogPost>(id);
        if (blogPost == null) return false;

        blogPost.ViewCount++;
        blogPost.UpdatedAt = DateTime.UtcNow;

        await _jsonDataService.UpdateAsync(blogPost);
        return true;
    }

    public async Task<bool> DeleteBlogPostAsync(int id)
    {
        return await _jsonDataService.DeleteAsync<BlogPost>(id);
    }

    public async Task<int> BatchUpdateStatusAsync(List<int> blogPostIds, bool isPublished)
    {
        var blogPosts = await _jsonDataService.GetAllAsync<BlogPost>();
        var updatedCount = 0;

        foreach (var id in blogPostIds)
        {
            var blogPost = blogPosts.FirstOrDefault(b => b.Id == id);
            if (blogPost != null)
            {
                blogPost.IsPublished = isPublished;
                if (isPublished && blogPost.PublishedAt == null)
                {
                    blogPost.PublishedAt = DateTime.UtcNow;
                    blogPost.PublishedDate = blogPost.PublishedAt;
                }
                blogPost.UpdatedAt = DateTime.UtcNow;

                await _jsonDataService.UpdateAsync(blogPost);
                updatedCount++;
            }
        }

        return updatedCount;
    }

    public async Task<object> GetBlogPostStatsAsync(int userId)
    {
        var blogPosts = await _jsonDataService.GetAllAsync<BlogPost>();
        var userPosts = blogPosts.Where(b => b.UserId == userId).ToList();

        var totalViews = userPosts.Sum(b => b.ViewCount);
        var publishedCount = userPosts.Count(b => b.IsPublished);
        var draftCount = userPosts.Count(b => !b.IsPublished);
        var publicCount = userPosts.Count(b => b.IsPublic);

        // 分類統計
        var categoryStats = userPosts
            .Where(b => !string.IsNullOrEmpty(b.Category))
            .GroupBy(b => b.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        // 月度發布統計
        var monthlyStats = userPosts
            .Where(b => b.IsPublished && b.PublishedAt.HasValue)
            .GroupBy(b => new { b.PublishedAt!.Value.Year, b.PublishedAt!.Value.Month })
            .Select(g => new { 
                Month = $"{g.Key.Year}-{g.Key.Month:D2}", 
                Count = g.Count(),
                Views = g.Sum(b => b.ViewCount)
            })
            .OrderBy(x => x.Month)
            .ToList();

        return new
        {
            TotalPosts = userPosts.Count,
            PublishedPosts = publishedCount,
            DraftPosts = draftCount,
            PublicPosts = publicCount,
            TotalViews = totalViews,
            AverageViews = userPosts.Count > 0 ? (double)totalViews / userPosts.Count : 0,
            CategoryStats = categoryStats,
            MonthlyStats = monthlyStats
        };
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync(bool publicOnly = true)
    {
        var blogPosts = await _jsonDataService.GetAllAsync<BlogPost>();
        var filtered = blogPosts.AsEnumerable();

        if (publicOnly)
        {
            filtered = filtered.Where(b => b.IsPublished && b.IsPublic);
        }

        return filtered
            .Where(b => !string.IsNullOrEmpty(b.Category))
            .Select(b => b.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    public async Task<IEnumerable<string>> GetTagsAsync(bool publicOnly = true)
    {
        var blogPosts = await _jsonDataService.GetAllAsync<BlogPost>();
        var filtered = blogPosts.AsEnumerable();

        if (publicOnly)
        {
            filtered = filtered.Where(b => b.IsPublished && b.IsPublic);
        }

        var allTags = filtered
            .Where(b => !string.IsNullOrEmpty(b.Tags))
            .SelectMany(b => b.Tags!.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        return allTags;
    }

    public async Task<string> GenerateSlugAsync(string title, int? excludeId = null)
    {
        // 移除特殊字元，保留字母數字和空格
        var slug = Regex.Replace(title, @"[^\w\s\u4e00-\u9fff]", "");
        
        // 將空格替換為連字符，並轉為小寫
        slug = Regex.Replace(slug, @"\s+", "-").ToLower();
        
        // 移除開頭和結尾的連字符
        slug = slug.Trim('-');
        
        // 限制長度
        if (slug.Length > 100)
        {
            slug = slug.Substring(0, 100).TrimEnd('-');
        }

        // 如果 slug 為空，使用預設值
        if (string.IsNullOrEmpty(slug))
        {
            slug = "blog-post";
        }

        // 確保唯一性
        var originalSlug = slug;
        var counter = 1;
        
        while (!await IsSlugUniqueAsync(slug, excludeId))
        {
            slug = $"{originalSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    public async Task<bool> IsSlugUniqueAsync(string slug, int? excludeId = null)
    {
        var blogPosts = await _jsonDataService.GetAllAsync<BlogPost>();
        return !blogPosts.Any(b => b.Slug == slug && b.Id != excludeId);
    }

    public async Task<object> GetBlogPostArchiveAsync(bool publicOnly = true)
    {
        var blogPosts = await _jsonDataService.GetAllAsync<BlogPost>();
        var filtered = blogPosts.AsEnumerable();

        if (publicOnly)
        {
            filtered = filtered.Where(b => b.IsPublished && b.IsPublic);
        }

        var archive = filtered
            .Where(b => b.PublishedAt.HasValue)
            .GroupBy(b => new { b.PublishedAt!.Value.Year, b.PublishedAt!.Value.Month })
            .Select(g => new {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                Count = g.Count(),
                Posts = g.OrderByDescending(b => b.PublishedAt).Select(b => new {
                    b.Id,
                    b.Title,
                    b.Slug,
                    b.PublishedAt,
                    b.ViewCount
                }).ToList()
            })
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ToList();

        return archive;
    }

    private static bool HasCommonTags(string tags1, string tags2)
    {
        var tags1List = tags1.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim().ToLower())
                            .ToList();
        
        var tags2List = tags2.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim().ToLower())
                            .ToList();

        return tags1List.Any(tag => tags2List.Contains(tag));
    }
}