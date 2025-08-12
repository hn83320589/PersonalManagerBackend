using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController : BaseController
{
    private readonly JsonDataService _dataService;

    public SkillsController(JsonDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<Skill>>>> GetSkills()
    {
        try
        {
            var skills = await _dataService.GetSkillsAsync();
            return Ok(ApiResponse<List<Skill>>.SuccessResult(skills, "成功取得技能列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<Skill>>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Skill>>> GetSkill(int id)
    {
        try
        {
            var skill = await _dataService.GetByIdAsync<Skill>("skills.json", id);
            
            if (skill == null)
            {
                return NotFound(ApiResponse<Skill>.ErrorResult("找不到指定的技能"));
            }

            return Ok(ApiResponse<Skill>.SuccessResult(skill, "成功取得技能資料"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Skill>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<List<Skill>>>> GetSkillsByUserId(int userId)
    {
        try
        {
            var skills = await _dataService.GetByUserIdAsync<Skill>("skills.json", userId);
            var sortedSkills = skills.OrderBy(s => s.SortOrder).ToList();
            
            return Ok(ApiResponse<List<Skill>>.SuccessResult(sortedSkills, "成功取得使用者技能列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<Skill>>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpGet("user/{userId}/public")]
    public async Task<ActionResult<ApiResponse<List<Skill>>>> GetPublicSkillsByUserId(int userId)
    {
        try
        {
            var skills = await _dataService.GetByUserIdAsync<Skill>("skills.json", userId);
            var publicSkills = skills.Where(s => s.IsPublic)
                                   .OrderBy(s => s.SortOrder)
                                   .ToList();
            
            return Ok(ApiResponse<List<Skill>>.SuccessResult(publicSkills, "成功取得公開技能列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<Skill>>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpGet("user/{userId}/category/{category}")]
    public async Task<ActionResult<ApiResponse<List<Skill>>>> GetSkillsByUserIdAndCategory(int userId, string category)
    {
        try
        {
            var skills = await _dataService.GetByUserIdAsync<Skill>("skills.json", userId);
            var categorySkills = skills.Where(s => s.Category != null && 
                                             s.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                                     .OrderBy(s => s.SortOrder)
                                     .ToList();
            
            return Ok(ApiResponse<List<Skill>>.SuccessResult(categorySkills, $"成功取得 {category} 類別技能列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<Skill>>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetSkillCategories()
    {
        try
        {
            var skills = await _dataService.GetSkillsAsync();
            var categories = skills.Where(s => !string.IsNullOrEmpty(s.Category))
                                 .Select(s => s.Category!)
                                 .Distinct()
                                 .OrderBy(c => c)
                                 .ToList();
            
            return Ok(ApiResponse<List<string>>.SuccessResult(categories, "成功取得技能分類列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<string>>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Skill>>> CreateSkill([FromBody] Skill skill)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();
                return BadRequest(ApiResponse<Skill>.ErrorResult("資料驗證失敗", errors));
            }

            var skills = await _dataService.GetSkillsAsync();
            
            // Generate new ID
            skill.Id = skills.Any() ? skills.Max(s => s.Id) + 1 : 1;
            skill.CreatedAt = DateTime.UtcNow;
            skill.UpdatedAt = DateTime.UtcNow;
            
            skills.Add(skill);
            await _dataService.SaveSkillsAsync(skills);

            return CreatedAtAction(nameof(GetSkill), new { id = skill.Id }, 
                ApiResponse<Skill>.SuccessResult(skill, "技能建立成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Skill>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Skill>>> UpdateSkill(int id, [FromBody] Skill skill)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();
                return BadRequest(ApiResponse<Skill>.ErrorResult("資料驗證失敗", errors));
            }

            var skills = await _dataService.GetSkillsAsync();
            var existingSkill = skills.FirstOrDefault(s => s.Id == id);
            
            if (existingSkill == null)
            {
                return NotFound(ApiResponse<Skill>.ErrorResult("找不到指定的技能"));
            }

            // Update skill properties
            existingSkill.Name = skill.Name;
            existingSkill.Category = skill.Category;
            existingSkill.Level = skill.Level;
            existingSkill.Description = skill.Description;
            existingSkill.IsPublic = skill.IsPublic;
            existingSkill.SortOrder = skill.SortOrder;
            existingSkill.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveSkillsAsync(skills);

            return Ok(ApiResponse<Skill>.SuccessResult(existingSkill, "技能更新成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Skill>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteSkill(int id)
    {
        try
        {
            var skills = await _dataService.GetSkillsAsync();
            var skill = skills.FirstOrDefault(s => s.Id == id);
            
            if (skill == null)
            {
                return NotFound(ApiResponse.ErrorResult("找不到指定的技能"));
            }

            skills.Remove(skill);
            await _dataService.SaveSkillsAsync(skills);

            return Ok(ApiResponse.SuccessResult("技能刪除成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }
}