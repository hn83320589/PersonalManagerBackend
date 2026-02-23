using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalManager.Api.DTOs;
using PersonalManager.Api.Services;

namespace PersonalManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogPostsController : ControllerBase
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
    public async Task<IActionResult> GetByUserId(int userId)
        => Ok(ApiResponse<List<BlogPostResponse>>.Ok(await _service.GetByUserIdAsync(userId)));

    [Authorize] [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBlogPostDto dto)
        => Ok(ApiResponse<BlogPostResponse>.Ok(await _service.CreateAsync(dto), "Blog post created"));

    [Authorize] [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBlogPostDto dto)
    {
        var item = await _service.UpdateAsync(id, dto);
        return item != null ? Ok(ApiResponse<BlogPostResponse>.Ok(item, "Blog post updated")) : NotFound(ApiResponse.Fail("Blog post not found"));
    }

    [Authorize] [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteAsync(id) ? Ok(ApiResponse.Ok("Blog post deleted")) : NotFound(ApiResponse.Fail("Blog post not found"));
}
