using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.DTOs.WorkExperience;
using PersonalManagerAPI.Services.Interfaces;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkExperiencesController : BaseController
{
    private readonly IWorkExperienceService _workExperienceService;

    public WorkExperiencesController(IWorkExperienceService workExperienceService)
    {
        _workExperienceService = workExperienceService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkExperienceResponseDto>>>> GetWorkExperiences([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var workExperiences = await _workExperienceService.GetAllWorkExperiencesAsync(skip, take);
        return Ok(ApiResponse<IEnumerable<WorkExperienceResponseDto>>.SuccessResult(workExperiences, "成功取得工作經歷列表"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<WorkExperienceResponseDto>>> GetWorkExperience(int id)
    {
        var workExperience = await _workExperienceService.GetWorkExperienceByIdAsync(id);
        
        if (workExperience == null)
        {
            return NotFound(ApiResponse<WorkExperienceResponseDto>.ErrorResult("找不到指定的工作經歷"));
        }

        return Ok(ApiResponse<WorkExperienceResponseDto>.SuccessResult(workExperience, "成功取得工作經歷資料"));
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkExperienceResponseDto>>>> GetWorkExperiencesByUserId(int userId)
    {
        var workExperiences = await _workExperienceService.GetWorkExperiencesByUserIdAsync(userId);
        return Ok(ApiResponse<IEnumerable<WorkExperienceResponseDto>>.SuccessResult(workExperiences, "成功取得使用者工作經歷列表"));
    }

    [HttpGet("public")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkExperienceResponseDto>>>> GetPublicWorkExperiences([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var workExperiences = await _workExperienceService.GetPublicWorkExperiencesAsync(skip, take);
        return Ok(ApiResponse<IEnumerable<WorkExperienceResponseDto>>.SuccessResult(workExperiences, "成功取得公開工作經歷列表"));
    }

    [HttpGet("current")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkExperienceResponseDto>>>> GetCurrentWorkExperiences([FromQuery] bool publicOnly = true)
    {
        var workExperiences = await _workExperienceService.GetCurrentWorkExperiencesAsync(publicOnly);
        return Ok(ApiResponse<IEnumerable<WorkExperienceResponseDto>>.SuccessResult(workExperiences, "成功取得目前工作經歷"));
    }

    [HttpGet("search/company")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkExperienceResponseDto>>>> SearchByCompany([FromQuery] string company, [FromQuery] bool publicOnly = true)
    {
        if (string.IsNullOrWhiteSpace(company))
        {
            return BadRequest(ApiResponse<IEnumerable<WorkExperienceResponseDto>>.ErrorResult("公司名稱不能為空"));
        }

        var workExperiences = await _workExperienceService.SearchByCompanyAsync(company, publicOnly);
        return Ok(ApiResponse<IEnumerable<WorkExperienceResponseDto>>.SuccessResult(workExperiences, "搜尋完成"));
    }

    [HttpGet("search/position")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkExperienceResponseDto>>>> SearchByPosition([FromQuery] string position, [FromQuery] bool publicOnly = true)
    {
        if (string.IsNullOrWhiteSpace(position))
        {
            return BadRequest(ApiResponse<IEnumerable<WorkExperienceResponseDto>>.ErrorResult("職位名稱不能為空"));
        }

        var workExperiences = await _workExperienceService.SearchByPositionAsync(position, publicOnly);
        return Ok(ApiResponse<IEnumerable<WorkExperienceResponseDto>>.SuccessResult(workExperiences, "搜尋完成"));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkExperienceResponseDto>>>> SearchWorkExperiences([FromQuery] string keyword, [FromQuery] bool publicOnly = true)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest(ApiResponse<IEnumerable<WorkExperienceResponseDto>>.ErrorResult("搜尋關鍵字不能為空"));
        }

        var workExperiences = await _workExperienceService.SearchWorkExperiencesAsync(keyword, publicOnly);
        return Ok(ApiResponse<IEnumerable<WorkExperienceResponseDto>>.SuccessResult(workExperiences, "搜尋完成"));
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkExperienceResponseDto>>>> GetWorkExperiencesByDateRange(
        [FromQuery] DateTime? startDate, 
        [FromQuery] DateTime? endDate, 
        [FromQuery] bool publicOnly = true)
    {
        var workExperiences = await _workExperienceService.GetWorkExperiencesByDateRangeAsync(startDate, endDate, publicOnly);
        return Ok(ApiResponse<IEnumerable<WorkExperienceResponseDto>>.SuccessResult(workExperiences, "成功取得指定時期的工作經歷"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<WorkExperienceResponseDto>>> CreateWorkExperience([FromBody] CreateWorkExperienceDto createWorkExperienceDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
            return BadRequest(ApiResponse<WorkExperienceResponseDto>.ErrorResult("資料驗證失敗", errors));
        }

        var workExperience = await _workExperienceService.CreateWorkExperienceAsync(createWorkExperienceDto);

        return CreatedAtAction(nameof(GetWorkExperience), new { id = workExperience.Id }, 
            ApiResponse<WorkExperienceResponseDto>.SuccessResult(workExperience, "工作經歷建立成功"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<WorkExperienceResponseDto>>> UpdateWorkExperience(int id, [FromBody] UpdateWorkExperienceDto updateWorkExperienceDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
            return BadRequest(ApiResponse<WorkExperienceResponseDto>.ErrorResult("資料驗證失敗", errors));
        }

        var workExperience = await _workExperienceService.UpdateWorkExperienceAsync(id, updateWorkExperienceDto);
        
        if (workExperience == null)
        {
            return NotFound(ApiResponse<WorkExperienceResponseDto>.ErrorResult("找不到指定的工作經歷"));
        }

        return Ok(ApiResponse<WorkExperienceResponseDto>.SuccessResult(workExperience, "工作經歷更新成功"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteWorkExperience(int id)
    {
        var result = await _workExperienceService.DeleteWorkExperienceAsync(id);
        
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResult("找不到指定的工作經歷"));
        }

        return Ok(ApiResponse.SuccessResult("工作經歷刪除成功"));
    }

    [HttpPut("{id}/order")]
    public async Task<ActionResult<ApiResponse>> UpdateWorkExperienceOrder(int id, [FromBody] int newSortOrder)
    {
        var result = await _workExperienceService.UpdateWorkExperienceOrderAsync(id, newSortOrder);
        
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResult("找不到指定的工作經歷"));
        }

        return Ok(ApiResponse.SuccessResult("工作經歷排序更新成功"));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<object>>> GetWorkExperienceStats()
    {
        var stats = await _workExperienceService.GetWorkExperienceStatsAsync();
        return Ok(ApiResponse<object>.SuccessResult(stats, "成功取得工作經歷統計"));
    }
}