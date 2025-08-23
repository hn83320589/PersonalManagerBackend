using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.DTOs.Skill;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services.Interfaces;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController : BaseController
{
    private readonly ISkillService _skillService;

    public SkillsController(ISkillService skillService)
    {
        _skillService = skillService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<SkillResponseDto>>>> GetSkills([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var skills = await _skillService.GetAllSkillsAsync(skip, take);
        return Ok(ApiResponse<IEnumerable<SkillResponseDto>>.SuccessResult(skills, "成功取得技能列表"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SkillResponseDto>>> GetSkill(int id)
    {
        var skill = await _skillService.GetSkillByIdAsync(id);
        
        if (skill == null)
        {
            return NotFound(ApiResponse<SkillResponseDto>.ErrorResult("找不到指定的技能"));
        }

        return Ok(ApiResponse<SkillResponseDto>.SuccessResult(skill, "成功取得技能資料"));
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SkillResponseDto>>>> GetSkillsByUserId(int userId)
    {
        var skills = await _skillService.GetSkillsByUserIdAsync(userId);
        return Ok(ApiResponse<IEnumerable<SkillResponseDto>>.SuccessResult(skills, "成功取得使用者技能列表"));
    }

    [HttpGet("public")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SkillResponseDto>>>> GetPublicSkills([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var skills = await _skillService.GetPublicSkillsAsync(skip, take);
        return Ok(ApiResponse<IEnumerable<SkillResponseDto>>.SuccessResult(skills, "成功取得公開技能列表"));
    }

    [HttpGet("level/{level}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SkillResponseDto>>>> GetSkillsByLevel(SkillLevel level, [FromQuery] bool publicOnly = true)
    {
        var skills = await _skillService.GetSkillsByLevelAsync(level, publicOnly);
        return Ok(ApiResponse<IEnumerable<SkillResponseDto>>.SuccessResult(skills, $"成功取得{level}等級技能"));
    }

    [HttpGet("category/{category}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SkillResponseDto>>>> GetSkillsByCategory(string category, [FromQuery] bool publicOnly = true)
    {
        var skills = await _skillService.GetSkillsByCategoryAsync(category, publicOnly);
        return Ok(ApiResponse<IEnumerable<SkillResponseDto>>.SuccessResult(skills, $"成功取得 {category} 類別技能列表"));
    }

    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetSkillCategories([FromQuery] bool publicOnly = true)
    {
        var categories = await _skillService.GetSkillCategoriesAsync(publicOnly);
        return Ok(ApiResponse<IEnumerable<string>>.SuccessResult(categories, "成功取得技能分類列表"));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SkillResponseDto>>>> SearchSkills([FromQuery] string keyword, [FromQuery] bool publicOnly = true)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest(ApiResponse<IEnumerable<SkillResponseDto>>.ErrorResult("搜尋關鍵字不能為空"));
        }

        var skills = await _skillService.SearchSkillsAsync(keyword, publicOnly);
        return Ok(ApiResponse<IEnumerable<SkillResponseDto>>.SuccessResult(skills, "搜尋完成"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SkillResponseDto>>> CreateSkill([FromBody] CreateSkillDto createSkillDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
            return BadRequest(ApiResponse<SkillResponseDto>.ErrorResult("資料驗證失敗", errors));
        }

        var skill = await _skillService.CreateSkillAsync(createSkillDto);

        return CreatedAtAction(nameof(GetSkill), new { id = skill.Id }, 
            ApiResponse<SkillResponseDto>.SuccessResult(skill, "技能建立成功"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<SkillResponseDto>>> UpdateSkill(int id, [FromBody] UpdateSkillDto updateSkillDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
            return BadRequest(ApiResponse<SkillResponseDto>.ErrorResult("資料驗證失敗", errors));
        }

        var skill = await _skillService.UpdateSkillAsync(id, updateSkillDto);
        
        if (skill == null)
        {
            return NotFound(ApiResponse<SkillResponseDto>.ErrorResult("找不到指定的技能"));
        }

        return Ok(ApiResponse<SkillResponseDto>.SuccessResult(skill, "技能更新成功"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteSkill(int id)
    {
        var result = await _skillService.DeleteSkillAsync(id);
        
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResult("找不到指定的技能"));
        }

        return Ok(ApiResponse.SuccessResult("技能刪除成功"));
    }

    [HttpPut("{id}/order")]
    public async Task<ActionResult<ApiResponse>> UpdateSkillOrder(int id, [FromBody] int newSortOrder)
    {
        var result = await _skillService.UpdateSkillOrderAsync(id, newSortOrder);
        
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResult("找不到指定的技能"));
        }

        return Ok(ApiResponse.SuccessResult("技能排序更新成功"));
    }

    [HttpPut("batch-order")]
    public async Task<ActionResult<ApiResponse>> BatchUpdateSkillOrder([FromBody] Dictionary<int, int> skillOrders)
    {
        if (skillOrders == null || !skillOrders.Any())
        {
            return BadRequest(ApiResponse.ErrorResult("排序資料不能為空"));
        }

        var result = await _skillService.BatchUpdateSkillOrderAsync(skillOrders);
        
        if (!result)
        {
            return BadRequest(ApiResponse.ErrorResult("批量更新排序失敗"));
        }

        return Ok(ApiResponse.SuccessResult("批量更新技能排序成功"));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<object>>> GetSkillStats()
    {
        var stats = await _skillService.GetSkillStatsAsync();
        return Ok(ApiResponse<object>.SuccessResult(stats, "成功取得技能統計"));
    }
}