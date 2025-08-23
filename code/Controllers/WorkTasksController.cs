using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.DTOs.WorkTask;
using PersonalManagerAPI.Services.Interfaces;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkTasksController : BaseController
{
    private readonly IWorkTaskService _workTaskService;

    public WorkTasksController(IWorkTaskService workTaskService)
    {
        _workTaskService = workTaskService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkTaskResponseDto>>>> GetWorkTasks([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var workTasks = await _workTaskService.GetAllWorkTasksAsync(skip, take);
        return Ok(ApiResponse<IEnumerable<WorkTaskResponseDto>>.SuccessResult(workTasks, "成功取得工作任務列表"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<WorkTaskResponseDto>>> GetWorkTask(int id)
    {
        var workTask = await _workTaskService.GetWorkTaskByIdAsync(id);
        
        if (workTask == null)
        {
            return NotFound(ApiResponse<WorkTaskResponseDto>.ErrorResult("找不到指定的工作任務"));
        }

        return Ok(ApiResponse<WorkTaskResponseDto>.SuccessResult(workTask, "成功取得工作任務資料"));
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkTaskResponseDto>>>> GetWorkTasksByUserId(int userId)
    {
        var workTasks = await _workTaskService.GetWorkTasksByUserIdAsync(userId);
        return Ok(ApiResponse<IEnumerable<WorkTaskResponseDto>>.SuccessResult(workTasks, "成功取得使用者工作任務列表"));
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkTaskResponseDto>>>> GetWorkTasksByStatus(string status, [FromQuery] int userId)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return BadRequest(ApiResponse<IEnumerable<WorkTaskResponseDto>>.ErrorResult("狀態不能為空"));
        }

        var workTasks = await _workTaskService.GetWorkTasksByStatusAsync(status, userId);
        return Ok(ApiResponse<IEnumerable<WorkTaskResponseDto>>.SuccessResult(workTasks, $"成功取得{status}狀態的工作任務"));
    }

    [HttpGet("priority/{priority}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkTaskResponseDto>>>> GetWorkTasksByPriority(string priority, [FromQuery] int userId)
    {
        if (string.IsNullOrWhiteSpace(priority))
        {
            return BadRequest(ApiResponse<IEnumerable<WorkTaskResponseDto>>.ErrorResult("優先級不能為空"));
        }

        var workTasks = await _workTaskService.GetWorkTasksByPriorityAsync(priority, userId);
        return Ok(ApiResponse<IEnumerable<WorkTaskResponseDto>>.SuccessResult(workTasks, $"成功取得{priority}優先級的工作任務"));
    }

    [HttpGet("project/{project}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkTaskResponseDto>>>> GetWorkTasksByProject(string project, [FromQuery] int userId)
    {
        if (string.IsNullOrWhiteSpace(project))
        {
            return BadRequest(ApiResponse<IEnumerable<WorkTaskResponseDto>>.ErrorResult("專案名稱不能為空"));
        }

        var workTasks = await _workTaskService.GetWorkTasksByProjectAsync(project, userId);
        return Ok(ApiResponse<IEnumerable<WorkTaskResponseDto>>.SuccessResult(workTasks, $"成功取得{project}專案的工作任務"));
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkTaskResponseDto>>>> GetOverdueWorkTasks([FromQuery] int userId)
    {
        var workTasks = await _workTaskService.GetOverdueWorkTasksAsync(userId);
        return Ok(ApiResponse<IEnumerable<WorkTaskResponseDto>>.SuccessResult(workTasks, "成功取得逾期工作任務"));
    }

    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkTaskResponseDto>>>> GetActiveWorkTasks([FromQuery] int userId)
    {
        var workTasks = await _workTaskService.GetActiveWorkTasksAsync(userId);
        return Ok(ApiResponse<IEnumerable<WorkTaskResponseDto>>.SuccessResult(workTasks, "成功取得進行中的工作任務"));
    }

    [HttpGet("completed")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkTaskResponseDto>>>> GetCompletedWorkTasks([FromQuery] int userId)
    {
        var workTasks = await _workTaskService.GetCompletedWorkTasksAsync(userId);
        return Ok(ApiResponse<IEnumerable<WorkTaskResponseDto>>.SuccessResult(workTasks, "成功取得已完成的工作任務"));
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkTaskResponseDto>>>> GetWorkTasksByDateRange(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate, 
        [FromQuery] int userId)
    {
        var workTasks = await _workTaskService.GetWorkTasksByDateRangeAsync(startDate, endDate, userId);
        return Ok(ApiResponse<IEnumerable<WorkTaskResponseDto>>.SuccessResult(workTasks, "成功取得指定日期範圍的工作任務"));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkTaskResponseDto>>>> SearchWorkTasks([FromQuery] string keyword, [FromQuery] int userId)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest(ApiResponse<IEnumerable<WorkTaskResponseDto>>.ErrorResult("搜尋關鍵字不能為空"));
        }

        var workTasks = await _workTaskService.SearchWorkTasksAsync(keyword, userId);
        return Ok(ApiResponse<IEnumerable<WorkTaskResponseDto>>.SuccessResult(workTasks, "搜尋完成"));
    }

    [HttpGet("search/tags")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkTaskResponseDto>>>> SearchWorkTasksByTags([FromQuery] string tags, [FromQuery] int userId)
    {
        if (string.IsNullOrWhiteSpace(tags))
        {
            return BadRequest(ApiResponse<IEnumerable<WorkTaskResponseDto>>.ErrorResult("標籤不能為空"));
        }

        var workTasks = await _workTaskService.SearchWorkTasksByTagsAsync(tags, userId);
        return Ok(ApiResponse<IEnumerable<WorkTaskResponseDto>>.SuccessResult(workTasks, "標籤搜尋完成"));
    }

    [HttpGet("projects")]
    public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetProjects([FromQuery] int userId)
    {
        var projects = await _workTaskService.GetProjectsAsync(userId);
        return Ok(ApiResponse<IEnumerable<string>>.SuccessResult(projects, "成功取得專案列表"));
    }

    [HttpGet("tags")]
    public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetTags([FromQuery] int userId)
    {
        var tags = await _workTaskService.GetTagsAsync(userId);
        return Ok(ApiResponse<IEnumerable<string>>.SuccessResult(tags, "成功取得標籤列表"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<WorkTaskResponseDto>>> CreateWorkTask([FromBody] CreateWorkTaskDto createWorkTaskDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
            return BadRequest(ApiResponse<WorkTaskResponseDto>.ErrorResult("資料驗證失敗", errors));
        }

        var workTask = await _workTaskService.CreateWorkTaskAsync(createWorkTaskDto);

        return CreatedAtAction(nameof(GetWorkTask), new { id = workTask.Id }, 
            ApiResponse<WorkTaskResponseDto>.SuccessResult(workTask, "工作任務建立成功"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<WorkTaskResponseDto>>> UpdateWorkTask(int id, [FromBody] UpdateWorkTaskDto updateWorkTaskDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
            return BadRequest(ApiResponse<WorkTaskResponseDto>.ErrorResult("資料驗證失敗", errors));
        }

        var workTask = await _workTaskService.UpdateWorkTaskAsync(id, updateWorkTaskDto);
        
        if (workTask == null)
        {
            return NotFound(ApiResponse<WorkTaskResponseDto>.ErrorResult("找不到指定的工作任務"));
        }

        return Ok(ApiResponse<WorkTaskResponseDto>.SuccessResult(workTask, "工作任務更新成功"));
    }

    [HttpPatch("{id}/start")]
    public async Task<ActionResult<ApiResponse>> StartWorkTask(int id)
    {
        var result = await _workTaskService.StartWorkTaskAsync(id);
        
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResult("找不到指定的工作任務"));
        }

        return Ok(ApiResponse.SuccessResult("工作任務已開始"));
    }

    [HttpPatch("{id}/pause")]
    public async Task<ActionResult<ApiResponse>> PauseWorkTask(int id)
    {
        var result = await _workTaskService.PauseWorkTaskAsync(id);
        
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResult("找不到指定的工作任務"));
        }

        return Ok(ApiResponse.SuccessResult("工作任務已暫停"));
    }

    [HttpPatch("{id}/complete")]
    public async Task<ActionResult<ApiResponse>> CompleteWorkTask(int id, [FromQuery] decimal? actualHours = null)
    {
        var result = await _workTaskService.CompleteWorkTaskAsync(id, actualHours);
        
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResult("找不到指定的工作任務"));
        }

        return Ok(ApiResponse.SuccessResult("工作任務已完成"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteWorkTask(int id)
    {
        var result = await _workTaskService.DeleteWorkTaskAsync(id);
        
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResult("找不到指定的工作任務"));
        }

        return Ok(ApiResponse.SuccessResult("工作任務刪除成功"));
    }

    [HttpPatch("batch/status")]
    public async Task<ActionResult<ApiResponse<int>>> BatchUpdateStatus([FromBody] BatchUpdateWorkTaskStatusRequest request)
    {
        if (request.WorkTaskIds == null || !request.WorkTaskIds.Any())
        {
            return BadRequest(ApiResponse<int>.ErrorResult("工作任務ID列表不能為空"));
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return BadRequest(ApiResponse<int>.ErrorResult("狀態不能為空"));
        }

        var updatedCount = await _workTaskService.BatchUpdateStatusAsync(request.WorkTaskIds, request.Status);
        return Ok(ApiResponse<int>.SuccessResult(updatedCount, $"批量更新了{updatedCount}個工作任務的狀態"));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<object>>> GetWorkTaskStats([FromQuery] int userId)
    {
        var stats = await _workTaskService.GetWorkTaskStatsAsync(userId);
        return Ok(ApiResponse<object>.SuccessResult(stats, "成功取得工作任務統計"));
    }

    [HttpGet("workload")]
    public async Task<ActionResult<ApiResponse<object>>> GetWorkloadStats(
        [FromQuery] int userId, 
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null)
    {
        var stats = await _workTaskService.GetWorkloadStatsAsync(userId, startDate, endDate);
        return Ok(ApiResponse<object>.SuccessResult(stats, "成功取得工作量統計"));
    }
}

// 批量操作的請求模型
public class BatchUpdateWorkTaskStatusRequest
{
    public List<int> WorkTaskIds { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}