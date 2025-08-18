using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkTasksController : BaseController
{
    private readonly JsonDataService _dataService;

    public WorkTasksController(JsonDataService dataService)
    {
        _dataService = dataService;
    }

    /// <summary>
    /// 取得所有工作追蹤項目
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWorkTasks([FromQuery] Models.TaskStatus? status = null, [FromQuery] TaskPriority? priority = null)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var query = workTasks.AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            if (priority.HasValue)
            {
                query = query.Where(t => t.Priority == priority.Value);
            }

            var filteredTasks = query
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .ToList();
                
            return Ok(ApiResponse<List<WorkTask>>.SuccessResult(filteredTasks, "成功取得工作追蹤列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<WorkTask>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得指定工作追蹤項目
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorkTask(int id)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var workTask = workTasks.FirstOrDefault(t => t.Id == id);
            
            if (workTask == null)
            {
                return NotFound(ApiResponse<WorkTask>.ErrorResult("找不到指定的工作追蹤項目"));
            }

            return Ok(ApiResponse<WorkTask>.SuccessResult(workTask, "成功取得工作追蹤資料"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WorkTask>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得指定使用者的工作追蹤項目
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetWorkTasksByUser(int userId, [FromQuery] Models.TaskStatus? status = null)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var query = workTasks.Where(t => t.UserId == userId);

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            var userWorkTasks = query
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .ToList();
                
            return Ok(ApiResponse<List<WorkTask>>.SuccessResult(userWorkTasks, "成功取得使用者工作追蹤列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<WorkTask>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得專案相關的工作追蹤項目
    /// </summary>
    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetWorkTasksByProject(string projectId)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var projectTasks = workTasks
                .Where(t => !string.IsNullOrEmpty(t.ProjectId) && 
                           t.ProjectId.Equals(projectId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .ToList();
                
            return Ok(ApiResponse<List<WorkTask>>.SuccessResult(projectTasks, "成功取得專案工作追蹤列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<WorkTask>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得進行中的工作追蹤項目
    /// </summary>
    [HttpGet("in-progress")]
    public async Task<IActionResult> GetInProgressTasks()
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var inProgressTasks = workTasks
                .Where(t => t.Status == Models.TaskStatus.InProgress)
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .ToList();
                
            return Ok(ApiResponse<List<WorkTask>>.SuccessResult(inProgressTasks, "成功取得進行中工作追蹤"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<WorkTask>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 建立工作追蹤項目
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateWorkTask([FromBody] WorkTask workTask)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            
            workTask.Id = workTasks.Count > 0 ? workTasks.Max(t => t.Id) + 1 : 1;
            workTask.Status = Models.TaskStatus.Pending;
            workTask.CreatedAt = DateTime.UtcNow;
            workTask.UpdatedAt = DateTime.UtcNow;
            
            workTasks.Add(workTask);
            await _dataService.SaveWorkTasksAsync(workTasks);

            return Ok(ApiResponse<WorkTask>.SuccessResult(workTask, "工作追蹤項目建立成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WorkTask>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 更新工作追蹤項目
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorkTask(int id, [FromBody] WorkTask updatedTask)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var workTask = workTasks.FirstOrDefault(t => t.Id == id);
            
            if (workTask == null)
            {
                return NotFound(ApiResponse<WorkTask>.ErrorResult("找不到指定的工作追蹤項目"));
            }

            workTask.Title = updatedTask.Title;
            workTask.Description = updatedTask.Description;
            workTask.Priority = updatedTask.Priority;
            workTask.Status = updatedTask.Status;
            workTask.DueDate = updatedTask.DueDate;
            workTask.ProjectId = updatedTask.ProjectId;
            workTask.EstimatedHours = updatedTask.EstimatedHours;
            workTask.ActualHours = updatedTask.ActualHours;
            workTask.UpdatedAt = DateTime.UtcNow;

            if (updatedTask.Status == Models.TaskStatus.Completed && workTask.CompletedAt == null)
            {
                workTask.CompletedAt = DateTime.UtcNow;
            }

            await _dataService.SaveWorkTasksAsync(workTasks);

            return Ok(ApiResponse<WorkTask>.SuccessResult(workTask, "工作追蹤項目更新成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WorkTask>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 記錄工作時間
    /// </summary>
    [HttpPut("{id}/time")]
    public async Task<IActionResult> UpdateWorkTime(int id, [FromBody] decimal actualHours)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var workTask = workTasks.FirstOrDefault(t => t.Id == id);
            
            if (workTask == null)
            {
                return NotFound(ApiResponse<WorkTask>.ErrorResult("找不到指定的工作追蹤項目"));
            }

            workTask.ActualHours = actualHours;
            workTask.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveWorkTasksAsync(workTasks);

            return Ok(ApiResponse<WorkTask>.SuccessResult(workTask, "工作時間記錄更新成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<WorkTask>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 刪除工作追蹤項目
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkTask(int id)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var workTask = workTasks.FirstOrDefault(t => t.Id == id);
            
            if (workTask == null)
            {
                return NotFound(ApiResponse.ErrorResult("找不到指定的工作追蹤項目"));
            }

            workTasks.Remove(workTask);
            await _dataService.SaveWorkTasksAsync(workTasks);

            return Ok(ApiResponse.SuccessResult("工作追蹤項目刪除成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }
}