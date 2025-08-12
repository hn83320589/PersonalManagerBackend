using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EducationsController : BaseController
{
    private readonly JsonDataService _dataService;

    public EducationsController(JsonDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<Education>>>> GetEducations()
    {
        try
        {
            var educations = await _dataService.GetEducationsAsync();
            return Ok(ApiResponse<List<Education>>.SuccessResult(educations, "成功取得學歷列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<Education>>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Education>>> GetEducation(int id)
    {
        try
        {
            var education = await _dataService.GetByIdAsync<Education>("educations.json", id);
            
            if (education == null)
            {
                return NotFound(ApiResponse<Education>.ErrorResult("找不到指定的學歷資料"));
            }

            return Ok(ApiResponse<Education>.SuccessResult(education, "成功取得學歷資料"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Education>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<List<Education>>>> GetEducationsByUserId(int userId)
    {
        try
        {
            var educations = await _dataService.GetByUserIdAsync<Education>("educations.json", userId);
            var sortedEducations = educations.OrderBy(e => e.SortOrder).ToList();
            
            return Ok(ApiResponse<List<Education>>.SuccessResult(sortedEducations, "成功取得使用者學歷列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<Education>>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpGet("user/{userId}/public")]
    public async Task<ActionResult<ApiResponse<List<Education>>>> GetPublicEducationsByUserId(int userId)
    {
        try
        {
            var educations = await _dataService.GetByUserIdAsync<Education>("educations.json", userId);
            var publicEducations = educations.Where(e => e.IsPublic)
                                           .OrderBy(e => e.SortOrder)
                                           .ToList();
            
            return Ok(ApiResponse<List<Education>>.SuccessResult(publicEducations, "成功取得公開學歷列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<Education>>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Education>>> CreateEducation([FromBody] Education education)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();
                return BadRequest(ApiResponse<Education>.ErrorResult("資料驗證失敗", errors));
            }

            var educations = await _dataService.GetEducationsAsync();
            
            // Generate new ID
            education.Id = educations.Any() ? educations.Max(e => e.Id) + 1 : 1;
            education.CreatedAt = DateTime.UtcNow;
            education.UpdatedAt = DateTime.UtcNow;
            
            educations.Add(education);
            await _dataService.SaveEducationsAsync(educations);

            return CreatedAtAction(nameof(GetEducation), new { id = education.Id }, 
                ApiResponse<Education>.SuccessResult(education, "學歷資料建立成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Education>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Education>>> UpdateEducation(int id, [FromBody] Education education)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();
                return BadRequest(ApiResponse<Education>.ErrorResult("資料驗證失敗", errors));
            }

            var educations = await _dataService.GetEducationsAsync();
            var existingEducation = educations.FirstOrDefault(e => e.Id == id);
            
            if (existingEducation == null)
            {
                return NotFound(ApiResponse<Education>.ErrorResult("找不到指定的學歷資料"));
            }

            // Update education properties
            existingEducation.School = education.School;
            existingEducation.Degree = education.Degree;
            existingEducation.FieldOfStudy = education.FieldOfStudy;
            existingEducation.StartDate = education.StartDate;
            existingEducation.EndDate = education.EndDate;
            existingEducation.Description = education.Description;
            existingEducation.IsPublic = education.IsPublic;
            existingEducation.SortOrder = education.SortOrder;
            existingEducation.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveEducationsAsync(educations);

            return Ok(ApiResponse<Education>.SuccessResult(existingEducation, "學歷資料更新成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Education>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteEducation(int id)
    {
        try
        {
            var educations = await _dataService.GetEducationsAsync();
            var education = educations.FirstOrDefault(e => e.Id == id);
            
            if (education == null)
            {
                return NotFound(ApiResponse.ErrorResult("找不到指定的學歷資料"));
            }

            educations.Remove(education);
            await _dataService.SaveEducationsAsync(educations);

            return Ok(ApiResponse.SuccessResult("學歷資料刪除成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }
}