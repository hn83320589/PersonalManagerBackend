using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.DTOs.BlogPost;
using PersonalManagerAPI.Services.Interfaces;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogPostsController : BaseController
{
    private readonly IBlogPostService _blogPostService;

    public BlogPostsController(IBlogPostService blogPostService)
    {
        _blogPostService = blogPostService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<BlogPostResponseDto>>>> GetBlogPosts([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var blogPosts = await _blogPostService.GetPublishedBlogPostsAsync(skip, take);
        return Ok(ApiResponse<IEnumerable<BlogPostResponseDto>>.SuccessResult(blogPosts, "成功取得公開文章列表"));
    }

    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<IEnumerable<BlogPostResponseDto>>>> GetAllBlogPosts([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var blogPosts = await _blogPostService.GetAllBlogPostsAsync(skip, take);
        return Ok(ApiResponse<IEnumerable<BlogPostResponseDto>>.SuccessResult(blogPosts, "成功取得所有文章列表"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<BlogPostResponseDto>>> GetBlogPost(int id)
    {
        var blogPost = await _blogPostService.GetBlogPostByIdAsync(id);
        
        if (blogPost == null)
        {
            return NotFound(ApiResponse<BlogPostResponseDto>.ErrorResult("找不到指定的部落格文章"));
        }

        // 增加瀏覽次數
        await _blogPostService.IncrementViewCountAsync(id);

        return Ok(ApiResponse<BlogPostResponseDto>.SuccessResult(blogPost, "成功取得部落格文章資料"));
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ApiResponse<BlogPostResponseDto>>> GetBlogPostBySlug(string slug)
    {
        var blogPost = await _blogPostService.GetBlogPostBySlugAsync(slug);
        
        if (blogPost == null)
        {
            return NotFound(ApiResponse<BlogPostResponseDto>.ErrorResult("找不到指定的部落格文章"));
        }

        // 增加瀏覽次數
        await _blogPostService.IncrementViewCountAsync(blogPost.Id);

        return Ok(ApiResponse<BlogPostResponseDto>.SuccessResult(blogPost, "成功取得部落格文章資料"));
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<BlogPostResponseDto>>>> GetBlogPostsByUserId(int userId)
    {
        var blogPosts = await _blogPostService.GetBlogPostsByUserIdAsync(userId);
        return Ok(ApiResponse<IEnumerable<BlogPostResponseDto>>.SuccessResult(blogPosts, "成功取得使用者部落格文章列表"));
    }

    [HttpGet("published")]
    public async Task<ActionResult<ApiResponse<IEnumerable<BlogPostResponseDto>>>> GetPublishedBlogPosts([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var blogPosts = await _blogPostService.GetPublishedBlogPostsAsync(skip, take);
        return Ok(ApiResponse<IEnumerable<BlogPostResponseDto>>.SuccessResult(blogPosts, "成功取得已發布部落格文章"));
    }

    [HttpGet("drafts")]
    public async Task<ActionResult<ApiResponse<IEnumerable<BlogPostResponseDto>>>> GetDraftBlogPosts([FromQuery] int userId)
    {
        var blogPosts = await _blogPostService.GetDraftBlogPostsAsync(userId);
        return Ok(ApiResponse<IEnumerable<BlogPostResponseDto>>.SuccessResult(blogPosts, "成功取得草稿文章"));
    }

    [HttpGet("category/{category}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<BlogPostResponseDto>>>> GetBlogPostsByCategory(string category, [FromQuery] bool publicOnly = true)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return BadRequest(ApiResponse<IEnumerable<BlogPostResponseDto>>.ErrorResult("分類不能為空"));
        }

        var blogPosts = await _blogPostService.GetBlogPostsByCategoryAsync(category, publicOnly);
        return Ok(ApiResponse<IEnumerable<BlogPostResponseDto>>.SuccessResult(blogPosts, $"成功取得{category}分類的文章"));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<BlogPostResponseDto>>>> SearchBlogPosts([FromQuery] string keyword, [FromQuery] bool publicOnly = true)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest(ApiResponse<IEnumerable<BlogPostResponseDto>>.ErrorResult("搜尋關鍵字不能為空"));
        }

        var blogPosts = await _blogPostService.SearchBlogPostsAsync(keyword, publicOnly);
        return Ok(ApiResponse<IEnumerable<BlogPostResponseDto>>.SuccessResult(blogPosts, "搜尋完成"));
    }

    [HttpGet("search/tags")]
    public async Task<ActionResult<ApiResponse<IEnumerable<BlogPostResponseDto>>>> SearchBlogPostsByTags([FromQuery] string tags, [FromQuery] bool publicOnly = true)
    {
        if (string.IsNullOrWhiteSpace(tags))
        {
            return BadRequest(ApiResponse<IEnumerable<BlogPostResponseDto>>.ErrorResult("標籤不能為空"));
        }

        var blogPosts = await _blogPostService.SearchBlogPostsByTagsAsync(tags, publicOnly);
        return Ok(ApiResponse<IEnumerable<BlogPostResponseDto>>.SuccessResult(blogPosts, "標籤搜尋完成"));
    }

    [HttpGet("popular")]
    public async Task<ActionResult<ApiResponse<IEnumerable<BlogPostResponseDto>>>> GetPopularBlogPosts([FromQuery] int count = 10, [FromQuery] bool publicOnly = true)
    {
        var blogPosts = await _blogPostService.GetPopularBlogPostsAsync(count, publicOnly);
        return Ok(ApiResponse<IEnumerable<BlogPostResponseDto>>.SuccessResult(blogPosts, "成功取得熱門文章"));
    }

    [HttpGet("latest")]
    public async Task<ActionResult<ApiResponse<IEnumerable<BlogPostResponseDto>>>> GetLatestBlogPosts([FromQuery] int count = 10, [FromQuery] bool publicOnly = true)
    {
        var blogPosts = await _blogPostService.GetLatestBlogPostsAsync(count, publicOnly);
        return Ok(ApiResponse<IEnumerable<BlogPostResponseDto>>.SuccessResult(blogPosts, "成功取得最新文章"));
    }

    [HttpGet("{id}/related")]
    public async Task<ActionResult<ApiResponse<IEnumerable<BlogPostResponseDto>>>> GetRelatedBlogPosts(int id, [FromQuery] int count = 5)
    {
        var blogPosts = await _blogPostService.GetRelatedBlogPostsAsync(id, count);
        return Ok(ApiResponse<IEnumerable<BlogPostResponseDto>>.SuccessResult(blogPosts, "成功取得相關文章"));
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<ApiResponse<IEnumerable<BlogPostResponseDto>>>> GetBlogPostsByDateRange(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate, 
        [FromQuery] bool publicOnly = true)
    {
        var blogPosts = await _blogPostService.GetBlogPostsByDateRangeAsync(startDate, endDate, publicOnly);
        return Ok(ApiResponse<IEnumerable<BlogPostResponseDto>>.SuccessResult(blogPosts, "成功取得指定日期範圍的文章"));
    }

    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetCategories([FromQuery] bool publicOnly = true)
    {
        var categories = await _blogPostService.GetCategoriesAsync(publicOnly);
        return Ok(ApiResponse<IEnumerable<string>>.SuccessResult(categories, "成功取得分類列表"));
    }

    [HttpGet("tags")]
    public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetTags([FromQuery] bool publicOnly = true)
    {
        var tags = await _blogPostService.GetTagsAsync(publicOnly);
        return Ok(ApiResponse<IEnumerable<string>>.SuccessResult(tags, "成功取得標籤列表"));
    }

    [HttpGet("archive")]
    public async Task<ActionResult<ApiResponse<object>>> GetBlogPostArchive([FromQuery] bool publicOnly = true)
    {
        var archive = await _blogPostService.GetBlogPostArchiveAsync(publicOnly);
        return Ok(ApiResponse<object>.SuccessResult(archive, "成功取得文章存檔"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BlogPostResponseDto>>> CreateBlogPost([FromBody] CreateBlogPostDto createBlogPostDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
            return BadRequest(ApiResponse<BlogPostResponseDto>.ErrorResult("資料驗證失敗", errors));
        }

        var blogPost = await _blogPostService.CreateBlogPostAsync(createBlogPostDto);

        return CreatedAtAction(nameof(GetBlogPost), new { id = blogPost.Id }, 
            ApiResponse<BlogPostResponseDto>.SuccessResult(blogPost, "部落格文章建立成功"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<BlogPostResponseDto>>> UpdateBlogPost(int id, [FromBody] UpdateBlogPostDto updateBlogPostDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
            return BadRequest(ApiResponse<BlogPostResponseDto>.ErrorResult("資料驗證失敗", errors));
        }

        var blogPost = await _blogPostService.UpdateBlogPostAsync(id, updateBlogPostDto);
        
        if (blogPost == null)
        {
            return NotFound(ApiResponse<BlogPostResponseDto>.ErrorResult("找不到指定的部落格文章"));
        }

        return Ok(ApiResponse<BlogPostResponseDto>.SuccessResult(blogPost, "部落格文章更新成功"));
    }

    [HttpPatch("{id}/publish")]
    public async Task<ActionResult<ApiResponse>> PublishBlogPost(int id)
    {
        var result = await _blogPostService.PublishBlogPostAsync(id);
        
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResult("找不到指定的部落格文章"));
        }

        return Ok(ApiResponse.SuccessResult("文章已發布"));
    }

    [HttpPatch("{id}/unpublish")]
    public async Task<ActionResult<ApiResponse>> UnpublishBlogPost(int id)
    {
        var result = await _blogPostService.UnpublishBlogPostAsync(id);
        
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResult("找不到指定的部落格文章"));
        }

        return Ok(ApiResponse.SuccessResult("文章已取消發布"));
    }

    [HttpPatch("{id}/view")]
    public async Task<ActionResult<ApiResponse>> IncrementViewCount(int id)
    {
        var result = await _blogPostService.IncrementViewCountAsync(id);
        
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResult("找不到指定的部落格文章"));
        }

        return Ok(ApiResponse.SuccessResult("瀏覽次數已更新"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteBlogPost(int id)
    {
        var result = await _blogPostService.DeleteBlogPostAsync(id);
        
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResult("找不到指定的部落格文章"));
        }

        return Ok(ApiResponse.SuccessResult("部落格文章刪除成功"));
    }

    [HttpPatch("batch/status")]
    public async Task<ActionResult<ApiResponse<int>>> BatchUpdateStatus([FromBody] BatchUpdateBlogPostStatusRequest request)
    {
        if (request.BlogPostIds == null || !request.BlogPostIds.Any())
        {
            return BadRequest(ApiResponse<int>.ErrorResult("部落格文章ID列表不能為空"));
        }

        var updatedCount = await _blogPostService.BatchUpdateStatusAsync(request.BlogPostIds, request.IsPublished);
        return Ok(ApiResponse<int>.SuccessResult(updatedCount, $"批量更新了{updatedCount}個文章的發布狀態"));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<object>>> GetBlogPostStats([FromQuery] int userId)
    {
        var stats = await _blogPostService.GetBlogPostStatsAsync(userId);
        return Ok(ApiResponse<object>.SuccessResult(stats, "成功取得部落格文章統計"));
    }

    [HttpPost("generate-slug")]
    public async Task<ActionResult<ApiResponse<string>>> GenerateSlug([FromBody] GenerateSlugRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(ApiResponse<string>.ErrorResult("標題不能為空"));
        }

        var slug = await _blogPostService.GenerateSlugAsync(request.Title, request.ExcludeId);
        return Ok(ApiResponse<string>.SuccessResult(slug, "Slug產生成功"));
    }

    [HttpPost("validate-slug")]
    public async Task<ActionResult<ApiResponse<bool>>> ValidateSlug([FromBody] ValidateSlugRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Slug))
        {
            return BadRequest(ApiResponse<bool>.ErrorResult("Slug不能為空"));
        }

        var isUnique = await _blogPostService.IsSlugUniqueAsync(request.Slug, request.ExcludeId);
        return Ok(ApiResponse<bool>.SuccessResult(isUnique, isUnique ? "Slug可用" : "Slug已被使用"));
    }
}

// 批量操作的請求模型
public class BatchUpdateBlogPostStatusRequest
{
    public List<int> BlogPostIds { get; set; } = new();
    public bool IsPublished { get; set; }
}

// Slug產生請求模型
public class GenerateSlugRequest
{
    public string Title { get; set; } = string.Empty;
    public int? ExcludeId { get; set; }
}

// Slug驗證請求模型
public class ValidateSlugRequest
{
    public string Slug { get; set; } = string.Empty;
    public int? ExcludeId { get; set; }
}