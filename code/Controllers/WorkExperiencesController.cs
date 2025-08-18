using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkExperiencesController : BaseController
{
    private readonly JsonDataService _dataService;

    public WorkExperiencesController(JsonDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<WorkExperience>>>> GetWorkExperiences()
    {
        try
        {
            var workExperiences = await _dataService.GetWorkExperiencesAsync();
            return Ok(ApiResponse<List<WorkExperience>>.SuccessResult(workExperiences, "成功取得工作經歷列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<WorkExperience>>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<WorkExperience>>> GetWorkExperience(int id)
    {
        try
        {
            var workExperience = await _dataService.GetByIdAsync<WorkExperience>("workExperiences.json", id);
            
            if (workExperience == null)
            {
                return NotFound(ApiResponse<WorkExperience>.ErrorResult("找不到指定的工作經歷"));
            }

            return Ok(ApiResponse<WorkExperience>.SuccessResult(workExperience, "成功取得工作經歷"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WorkExperience>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<List<WorkExperience>>>> GetWorkExperiencesByUserId(int userId)
    {
        try
        {
            var workExperiences = await _dataService.GetByUserIdAsync<WorkExperience>("workExperiences.json", userId);
            var sortedWorkExperiences = workExperiences.OrderBy(w => w.SortOrder).ToList();
            
            return Ok(ApiResponse<List<WorkExperience>>.SuccessResult(sortedWorkExperiences, "成功取得使用者工作經歷列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<WorkExperience>>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpGet("user/{userId}/public")]
    public async Task<ActionResult<ApiResponse<List<WorkExperience>>>> GetPublicWorkExperiencesByUserId(int userId)
    {
        try
        {
            var workExperiences = await _dataService.GetByUserIdAsync<WorkExperience>("workExperiences.json", userId);
            var publicWorkExperiences = workExperiences.Where(w => w.IsPublic)
                                                      .OrderBy(w => w.SortOrder)
                                                      .ToList();
            
            return Ok(ApiResponse<List<WorkExperience>>.SuccessResult(publicWorkExperiences, "成功取得公開工作經歷列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<WorkExperience>>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpGet("user/{userId}/current")]
    public async Task<ActionResult<ApiResponse<List<WorkExperience>>>> GetCurrentWorkExperiencesByUserId(int userId)
    {
        try
        {
            var workExperiences = await _dataService.GetByUserIdAsync<WorkExperience>("workExperiences.json", userId);
            var currentWorkExperiences = workExperiences.Where(w => w.IsCurrent)
                                                       .OrderBy(w => w.SortOrder)
                                                       .ToList();
            
            return Ok(ApiResponse<List<WorkExperience>>.SuccessResult(currentWorkExperiences, "成功取得目前工作經歷"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<WorkExperience>>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<WorkExperience>>> CreateWorkExperience([FromBody] WorkExperience workExperience)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();
                return BadRequest(ApiResponse<WorkExperience>.ErrorResult("資料驗證失敗", errors));
            }

            var workExperiences = await _dataService.GetWorkExperiencesAsync();
            
            // Generate new ID
            workExperience.Id = workExperiences.Any() ? workExperiences.Max(w => w.Id) + 1 : 1;
            workExperience.CreatedAt = DateTime.UtcNow;
            workExperience.UpdatedAt = DateTime.UtcNow;
            
            workExperiences.Add(workExperience);
            await _dataService.SaveWorkExperiencesAsync(workExperiences);

            return CreatedAtAction(nameof(GetWorkExperience), new { id = workExperience.Id }, 
                ApiResponse<WorkExperience>.SuccessResult(workExperience, "工作經歷建立成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WorkExperience>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<WorkExperience>>> UpdateWorkExperience(int id, [FromBody] WorkExperience workExperience)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();
                return BadRequest(ApiResponse<WorkExperience>.ErrorResult("資料驗證失敗", errors));
            }

            var workExperiences = await _dataService.GetWorkExperiencesAsync();
            var existingWorkExperience = workExperiences.FirstOrDefault(w => w.Id == id);
            
            if (existingWorkExperience == null)
            {
                return NotFound(ApiResponse<WorkExperience>.ErrorResult("找不到指定的工作經歷"));
            }

            // Update work experience properties
            existingWorkExperience.Company = workExperience.Company;
            existingWorkExperience.Position = workExperience.Position;
            existingWorkExperience.StartDate = workExperience.StartDate;
            existingWorkExperience.EndDate = workExperience.EndDate;
            existingWorkExperience.IsCurrent = workExperience.IsCurrent;
            existingWorkExperience.Description = workExperience.Description;
            existingWorkExperience.Achievements = workExperience.Achievements;
            existingWorkExperience.IsPublic = workExperience.IsPublic;
            existingWorkExperience.SortOrder = workExperience.SortOrder;
            existingWorkExperience.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveWorkExperiencesAsync(workExperiences);

            return Ok(ApiResponse<WorkExperience>.SuccessResult(existingWorkExperience, "工作經歷更新成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WorkExperience>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteWorkExperience(int id)
    {
        try
        {
            var workExperiences = await _dataService.GetWorkExperiencesAsync();
            var workExperience = workExperiences.FirstOrDefault(w => w.Id == id);
            
            if (workExperience == null)
            {
                return NotFound(ApiResponse.ErrorResult("找不到指定的工作經歷"));
            }

            workExperiences.Remove(workExperience);
            await _dataService.SaveWorkExperiencesAsync(workExperiences);

            return Ok(ApiResponse.SuccessResult("工作經歷刪除成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }
}