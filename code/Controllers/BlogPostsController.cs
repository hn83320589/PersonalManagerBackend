using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogPostsController : BaseController
{
    private readonly JsonDataService _dataService;

    public BlogPostsController(JsonDataService dataService)
    {
        _dataService = dataService;
    }

    /// <summary>
    /// 取得所有公開文章
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetBlogPosts([FromQuery] string? category = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var blogPosts = await _dataService.GetBlogPostsAsync();
            var query = blogPosts.Where(b => b.IsPublished);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(b => b.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            var totalCount = query.Count();
            var posts = query
                .OrderByDescending(b => b.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new
            {
                Posts = posts,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
                
            return Ok(ApiResponse<object>.SuccessResult(result, "成功取得文章列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得指定文章
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBlogPost(int id)
    {
        try
        {
            var blogPosts = await _dataService.GetBlogPostsAsync();
            var blogPost = blogPosts.FirstOrDefault(b => b.Id == id);
            
            if (blogPost == null)
            {
                return NotFound(ApiResponse<BlogPost>.ErrorResult("找不到指定的文章"));
            }

            // 增加瀏覽次數
            blogPost.ViewCount++;
            await _dataService.SaveBlogPostsAsync(blogPosts);

            return Ok(ApiResponse<BlogPost>.SuccessResult(blogPost, "成功取得文章資料"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<BlogPost>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 依slug取得文章
    /// </summary>
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBlogPostBySlug(string slug)
    {
        try
        {
            var blogPosts = await _dataService.GetBlogPostsAsync();
            var blogPost = blogPosts.FirstOrDefault(b => b.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase) && b.IsPublished);
            
            if (blogPost == null)
            {
                return NotFound(ApiResponse<BlogPost>.ErrorResult("找不到指定的文章"));
            }

            // 增加瀏覽次數
            blogPost.ViewCount++;
            await _dataService.SaveBlogPostsAsync(blogPosts);

            return Ok(ApiResponse<BlogPost>.SuccessResult(blogPost, "成功取得文章資料"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<BlogPost>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得指定使用者的文章
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetBlogPostsByUser(int userId, [FromQuery] bool includeUnpublished = false)
    {
        try
        {
            var blogPosts = await _dataService.GetBlogPostsAsync();
            var query = blogPosts.Where(b => b.UserId == userId);

            if (!includeUnpublished)
            {
                query = query.Where(b => b.IsPublished);
            }

            var userPosts = query
                .OrderByDescending(b => b.CreatedAt)
                .ToList();
                
            return Ok(ApiResponse<List<BlogPost>>.SuccessResult(userPosts, "成功取得使用者文章列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<BlogPost>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 搜尋文章
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchBlogPosts([FromQuery] string keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var blogPosts = await _dataService.GetBlogPostsAsync();
            var query = blogPosts.Where(b => b.IsPublished && 
                (b.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                 b.Content.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                 (!string.IsNullOrEmpty(b.Tags) && b.Tags.Contains(keyword, StringComparison.OrdinalIgnoreCase))));

            var totalCount = query.Count();
            var posts = query
                .OrderByDescending(b => b.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new
            {
                Posts = posts,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Keyword = keyword
            };
                
            return Ok(ApiResponse<object>.SuccessResult(result, $"搜尋「{keyword}」找到 {totalCount} 篇文章"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得熱門文章
    /// </summary>
    [HttpGet("popular")]
    public async Task<IActionResult> GetPopularPosts([FromQuery] int limit = 5)
    {
        try
        {
            var blogPosts = await _dataService.GetBlogPostsAsync();
            var popularPosts = blogPosts
                .Where(b => b.IsPublished)
                .OrderByDescending(b => b.ViewCount)
                .Take(limit)
                .ToList();
                
            return Ok(ApiResponse<List<BlogPost>>.SuccessResult(popularPosts, "成功取得熱門文章"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<BlogPost>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 建立文章
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateBlogPost([FromBody] BlogPost blogPost)
    {
        try
        {
            var blogPosts = await _dataService.GetBlogPostsAsync();
            
            blogPost.Id = blogPosts.Count > 0 ? blogPosts.Max(b => b.Id) + 1 : 1;
            blogPost.CreatedAt = DateTime.UtcNow;
            blogPost.UpdatedAt = DateTime.UtcNow;
            blogPost.ViewCount = 0;

            if (blogPost.IsPublished && blogPost.PublishedAt == null)
            {
                blogPost.PublishedAt = DateTime.UtcNow;
            }

            // 產生slug
            if (string.IsNullOrEmpty(blogPost.Slug))
            {
                blogPost.Slug = GenerateSlug(blogPost.Title);
            }
            
            blogPosts.Add(blogPost);
            await _dataService.SaveBlogPostsAsync(blogPosts);

            return Ok(ApiResponse<BlogPost>.SuccessResult(blogPost, "文章建立成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<BlogPost>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 更新文章
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBlogPost(int id, [FromBody] BlogPost updatedPost)
    {
        try
        {
            var blogPosts = await _dataService.GetBlogPostsAsync();
            var blogPost = blogPosts.FirstOrDefault(b => b.Id == id);
            
            if (blogPost == null)
            {
                return NotFound(ApiResponse<BlogPost>.ErrorResult("找不到指定的文章"));
            }

            var wasUnpublished = !blogPost.IsPublished;

            blogPost.Title = updatedPost.Title;
            blogPost.Content = updatedPost.Content;
            blogPost.Summary = updatedPost.Summary;
            blogPost.Category = updatedPost.Category;
            blogPost.Tags = updatedPost.Tags;
            blogPost.FeaturedImageUrl = updatedPost.FeaturedImageUrl;
            blogPost.IsPublished = updatedPost.IsPublished;
            blogPost.UpdatedAt = DateTime.UtcNow;

            // 如果從未發布變為發布，設定發布時間
            if (wasUnpublished && updatedPost.IsPublished)
            {
                blogPost.PublishedAt = DateTime.UtcNow;
            }

            await _dataService.SaveBlogPostsAsync(blogPosts);

            return Ok(ApiResponse<BlogPost>.SuccessResult(blogPost, "文章更新成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<BlogPost>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 刪除文章
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBlogPost(int id)
    {
        try
        {
            var blogPosts = await _dataService.GetBlogPostsAsync();
            var blogPost = blogPosts.FirstOrDefault(b => b.Id == id);
            
            if (blogPost == null)
            {
                return NotFound(ApiResponse.ErrorResult("找不到指定的文章"));
            }

            blogPosts.Remove(blogPost);
            await _dataService.SaveBlogPostsAsync(blogPosts);

            return Ok(ApiResponse.SuccessResult("文章刪除成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    private string GenerateSlug(string title)
    {
        return title.ToLowerInvariant()
                    .Replace(" ", "-")
                    .Replace(".", "-")
                    .Replace(",", "")
                    .Replace("!", "")
                    .Replace("?", "");
    }
}