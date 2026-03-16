using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Services;

namespace PersonalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogPostsController : BaseApiController
{
    private readonly IBlogPostService _service;
    public BlogPostsController(IBlogPostService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(ApiResponse<List<BlogPostResponse>>.Ok(await _service.GetAllAsync()));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item != null ? Ok(ApiResponse<BlogPostResponse>.Ok(item)) : NotFound(ApiResponse.Fail("Blog post not found"));
    }

    [HttpGet("published")]
    public async Task<IActionResult> GetPublished()
        => Ok(ApiResponse<List<BlogPostResponse>>.Ok(await _service.GetPublishedAsync()));

    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var item = await _service.GetBySlugAsync(slug);
        return item != null ? Ok(ApiResponse<BlogPostResponse>.Ok(item)) : NotFound(ApiResponse.Fail("Blog post not found"));
    }

    [HttpGet("user/{userId}/public")]
    public async Task<IActionResult> GetPublicByUserId(int userId)
        => Ok(ApiResponse<List<BlogPostResponse>>.Ok(await _service.GetPublicByUserIdAsync(userId)));

    [Authorize]
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId, [FromQuery] string? tag = null)
    {
        var posts = await _service.GetByUserIdAsync(userId);
        if (!string.IsNullOrEmpty(tag))
        {
            posts = posts.Where(p => p.Tags != null &&
                p.Tags.Split(',').Select(t => t.Trim()).Contains(tag, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }
        return Ok(ApiResponse<List<BlogPostResponse>>.Ok(posts));
    }

    [HttpPost("{id}/view")]
    public async Task<IActionResult> IncrementView(int id)
    {
        await _service.IncrementViewCountAsync(id);
        return Ok(ApiResponse.Ok("View counted"));
    }

    [HttpGet("user/{userId}/tags")]
    public async Task<IActionResult> GetTagsByUserId(int userId)
    {
        var posts = await _service.GetPublicByUserIdAsync(userId);
        var tags = posts
            .Where(p => !string.IsNullOrEmpty(p.Tags))
            .SelectMany(p => p.Tags.Split(',').Select(t => t.Trim()))
            .Where(t => !string.IsNullOrEmpty(t))
            .Distinct()
            .OrderBy(t => t)
            .ToList();
        return Ok(ApiResponse<List<string>>.Ok(tags));
    }

    [HttpGet("user/{userId}/categories")]
    public async Task<IActionResult> GetCategoriesByUserId(int userId)
    {
        var posts = await _service.GetPublicByUserIdAsync(userId);
        var categories = posts
            .Where(p => !string.IsNullOrEmpty(p.Category))
            .Select(p => p.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
        return Ok(ApiResponse<List<string>>.Ok(categories));
    }

    [HttpGet("user/{userId}/public/paged")]
    public async Task<IActionResult> GetPublicByUserIdPaged(
        int userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 8,
        [FromQuery] string? keyword = null,
        [FromQuery] string? tag = null,
        [FromQuery] string? category = null)
    {
        var result = await _service.GetPublicPagedAsync(userId, Math.Max(1, page), Math.Clamp(pageSize, 1, 50), keyword, tag, category);
        return Ok(ApiResponse<PagedResult<BlogPostResponse>>.Ok(result));
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBlogPostDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        dto.UserId = currentUserId.Value;
        return Ok(ApiResponse<BlogPostResponse>.Ok(await _service.CreateAsync(dto), "Blog post created"));
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBlogPostDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound(ApiResponse.Fail("Blog post not found"));
        if (existing.UserId != currentUserId.Value) return StatusCode(403, ApiResponse.Fail("Forbidden"));
        var item = await _service.UpdateAsync(id, dto);
        return item != null ? Ok(ApiResponse<BlogPostResponse>.Ok(item, "Blog post updated")) : NotFound(ApiResponse.Fail("Blog post not found"));
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null) return Unauthorized(ApiResponse.Fail("Unauthorized"));
        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound(ApiResponse.Fail("Blog post not found"));
        if (existing.UserId != currentUserId.Value) return StatusCode(403, ApiResponse.Fail("Forbidden"));
        return await _service.DeleteAsync(id) ? Ok(ApiResponse.Ok("Blog post deleted")) : NotFound(ApiResponse.Fail("Blog post not found"));
    }
}
