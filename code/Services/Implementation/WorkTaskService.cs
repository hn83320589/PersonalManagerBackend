using PersonalManagerAPI.DTOs.WorkTask;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;
using PersonalManagerAPI.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace PersonalManagerAPI.Services.Implementation;

/// <summary>
/// 工作任務服務實作
/// </summary>
public class WorkTaskService : IWorkTaskService
{
    private readonly JsonDataService _dataService;
    private readonly IMapper _mapper;
    private readonly ILogger<WorkTaskService> _logger;

    // 預定義的狀態列表
    private readonly HashSet<string> _validStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Pending", "Planning", "InProgress", "Testing", "Completed", "OnHold", "Cancelled"
    };

    // 預定義的優先級列表
    private readonly HashSet<string> _validPriorities = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Low", "Medium", "High", "Urgent"
    };

    public WorkTaskService(JsonDataService dataService, IMapper mapper, ILogger<WorkTaskService> logger)
    {
        _dataService = dataService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<WorkTaskResponseDto>> GetAllWorkTasksAsync(int skip = 0, int take = 50)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var pagedTasks = workTasks
                .OrderByDescending(t => t.CreatedAt)
                .Skip(skip)
                .Take(take);
            
            return _mapper.Map<IEnumerable<WorkTaskResponseDto>>(pagedTasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all work tasks");
            throw;
        }
    }

    public async Task<WorkTaskResponseDto?> GetWorkTaskByIdAsync(int id)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var workTask = workTasks.FirstOrDefault(t => t.Id == id);
            return _mapper.Map<WorkTaskResponseDto?>(workTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting work task with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<WorkTaskResponseDto>> GetWorkTasksByUserIdAsync(int userId)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var userTasks = workTasks
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt);
            
            return _mapper.Map<IEnumerable<WorkTaskResponseDto>>(userTasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting work tasks for user ID {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<WorkTaskResponseDto>> GetWorkTasksByStatusAsync(string status, int userId)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var filteredTasks = workTasks
                .Where(t => t.UserId == userId && 
                           t.Status.ToString().Equals(status, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(t => t.CreatedAt);
            
            return _mapper.Map<IEnumerable<WorkTaskResponseDto>>(filteredTasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting work tasks by status {Status} for user {UserId}", status, userId);
            throw;
        }
    }

    public async Task<IEnumerable<WorkTaskResponseDto>> GetWorkTasksByPriorityAsync(string priority, int userId)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var filteredTasks = workTasks
                .Where(t => t.UserId == userId && 
                           t.Priority.ToString().Equals(priority, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(t => t.CreatedAt);
            
            return _mapper.Map<IEnumerable<WorkTaskResponseDto>>(filteredTasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting work tasks by priority {Priority} for user {UserId}", priority, userId);
            throw;
        }
    }

    public async Task<IEnumerable<WorkTaskResponseDto>> GetWorkTasksByProjectAsync(string project, int userId)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var filteredTasks = workTasks
                .Where(t => t.UserId == userId && 
                           !string.IsNullOrEmpty(t.Project) &&
                           t.Project.Equals(project, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(t => t.CreatedAt);
            
            return _mapper.Map<IEnumerable<WorkTaskResponseDto>>(filteredTasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting work tasks by project {Project} for user {UserId}", project, userId);
            throw;
        }
    }

    public async Task<IEnumerable<WorkTaskResponseDto>> GetOverdueWorkTasksAsync(int userId)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var now = DateTime.Now;
            var overdueTasks = workTasks
                .Where(t => t.UserId == userId && 
                           t.DueDate.HasValue && 
                           t.DueDate.Value < now && 
                           t.Status != Models.TaskStatus.Completed)
                .OrderBy(t => t.DueDate);
            
            return _mapper.Map<IEnumerable<WorkTaskResponseDto>>(overdueTasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting overdue work tasks for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<WorkTaskResponseDto>> GetActiveWorkTasksAsync(int userId)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var activeTasks = workTasks
                .Where(t => t.UserId == userId && 
                           (t.Status == Models.TaskStatus.InProgress || 
                            t.Status == Models.TaskStatus.Planning))
                .OrderByDescending(t => t.CreatedAt);
            
            return _mapper.Map<IEnumerable<WorkTaskResponseDto>>(activeTasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting active work tasks for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<WorkTaskResponseDto>> GetCompletedWorkTasksAsync(int userId)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var completedTasks = workTasks
                .Where(t => t.UserId == userId && t.Status == Models.TaskStatus.Completed)
                .OrderByDescending(t => t.CompletedAt ?? t.UpdatedAt);
            
            return _mapper.Map<IEnumerable<WorkTaskResponseDto>>(completedTasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting completed work tasks for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<WorkTaskResponseDto>> GetWorkTasksByDateRangeAsync(DateTime startDate, DateTime endDate, int userId)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var filteredTasks = workTasks.Where(t =>
                t.UserId == userId &&
                ((t.StartDate.HasValue && t.StartDate.Value >= startDate && t.StartDate.Value <= endDate) ||
                 (t.DueDate.HasValue && t.DueDate.Value >= startDate && t.DueDate.Value <= endDate) ||
                 (t.CreatedAt >= startDate && t.CreatedAt <= endDate))
            ).OrderBy(t => t.StartDate ?? t.CreatedAt);
            
            return _mapper.Map<IEnumerable<WorkTaskResponseDto>>(filteredTasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting work tasks by date range for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<WorkTaskResponseDto>> SearchWorkTasksAsync(string keyword, int userId)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var searchResults = workTasks.Where(t =>
                t.UserId == userId &&
                (t.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                 (!string.IsNullOrEmpty(t.Description) && t.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                 (!string.IsNullOrEmpty(t.Project) && t.Project.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                 (!string.IsNullOrEmpty(t.Tags) && t.Tags.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            ).OrderByDescending(t => t.CreatedAt);
            
            return _mapper.Map<IEnumerable<WorkTaskResponseDto>>(searchResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching work tasks with keyword {Keyword} for user {UserId}", keyword, userId);
            throw;
        }
    }

    public async Task<IEnumerable<WorkTaskResponseDto>> SearchWorkTasksByTagsAsync(string tags, int userId)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                             .Select(t => t.Trim())
                             .Where(t => !string.IsNullOrEmpty(t));
            
            var searchResults = workTasks.Where(t =>
                t.UserId == userId &&
                !string.IsNullOrEmpty(t.Tags) &&
                tagList.Any(tag => t.Tags.Contains(tag, StringComparison.OrdinalIgnoreCase))
            ).OrderByDescending(t => t.CreatedAt);
            
            return _mapper.Map<IEnumerable<WorkTaskResponseDto>>(searchResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching work tasks by tags {Tags} for user {UserId}", tags, userId);
            throw;
        }
    }

    public async Task<WorkTaskResponseDto> CreateWorkTaskAsync(CreateWorkTaskDto createWorkTaskDto)
    {
        try
        {
            // 驗證日期
            if (!ValidateTaskDates(createWorkTaskDto.StartDate, createWorkTaskDto.DueDate))
            {
                throw new InvalidOperationException("到期日期不能早於開始日期");
            }

            // 驗證狀態
            if (!string.IsNullOrEmpty(createWorkTaskDto.Status) && 
                !_validStatuses.Contains(createWorkTaskDto.Status))
            {
                _logger.LogWarning("Invalid status provided: {Status}", createWorkTaskDto.Status);
            }

            // 驗證優先級
            if (!string.IsNullOrEmpty(createWorkTaskDto.Priority) && 
                !_validPriorities.Contains(createWorkTaskDto.Priority))
            {
                _logger.LogWarning("Invalid priority provided: {Priority}", createWorkTaskDto.Priority);
            }

            var workTasks = await _dataService.GetWorkTasksAsync();
            var workTask = _mapper.Map<Models.WorkTask>(createWorkTaskDto);
            
            // 生成新ID
            workTask.Id = workTasks.Any() ? workTasks.Max(t => t.Id) + 1 : 1;
            
            // 設定時間戳記
            workTask.CreatedAt = DateTime.UtcNow;
            workTask.UpdatedAt = DateTime.UtcNow;

            workTasks.Add(workTask);
            await _dataService.SaveWorkTasksAsync(workTasks);

            _logger.LogInformation("Work task created with ID {Id} for user {UserId}", workTask.Id, workTask.UserId);
            return _mapper.Map<WorkTaskResponseDto>(workTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating work task");
            throw;
        }
    }

    public async Task<WorkTaskResponseDto?> UpdateWorkTaskAsync(int id, UpdateWorkTaskDto updateWorkTaskDto)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var existingTask = workTasks.FirstOrDefault(t => t.Id == id);
            
            if (existingTask == null)
            {
                _logger.LogWarning("Work task with ID {Id} not found for update", id);
                return null;
            }

            // 驗證日期（如果有提供）
            var startDate = updateWorkTaskDto.StartDate ?? existingTask.StartDate;
            var dueDate = updateWorkTaskDto.DueDate ?? existingTask.DueDate;
            
            if (!ValidateTaskDates(startDate, dueDate))
            {
                throw new InvalidOperationException("到期日期不能早於開始日期");
            }

            // 驗證狀態（如果有提供）
            if (!string.IsNullOrEmpty(updateWorkTaskDto.Status) && 
                !_validStatuses.Contains(updateWorkTaskDto.Status))
            {
                _logger.LogWarning("Invalid status provided for update: {Status}", updateWorkTaskDto.Status);
            }

            // 驗證優先級（如果有提供）
            if (!string.IsNullOrEmpty(updateWorkTaskDto.Priority) && 
                !_validPriorities.Contains(updateWorkTaskDto.Priority))
            {
                _logger.LogWarning("Invalid priority provided for update: {Priority}", updateWorkTaskDto.Priority);
            }

            _mapper.Map(updateWorkTaskDto, existingTask);
            existingTask.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveWorkTasksAsync(workTasks);
            _logger.LogInformation("Work task with ID {Id} updated", id);
            
            return _mapper.Map<WorkTaskResponseDto>(existingTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating work task with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> StartWorkTaskAsync(int id)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var workTask = workTasks.FirstOrDefault(t => t.Id == id);
            
            if (workTask == null)
            {
                _logger.LogWarning("Work task with ID {Id} not found for starting", id);
                return false;
            }

            if (workTask.Status != Models.TaskStatus.InProgress)
            {
                workTask.Status = Models.TaskStatus.InProgress;
                if (!workTask.StartDate.HasValue)
                {
                    workTask.StartDate = DateTime.UtcNow;
                }
                workTask.UpdatedAt = DateTime.UtcNow;

                await _dataService.SaveWorkTasksAsync(workTasks);
                _logger.LogInformation("Work task with ID {Id} started", id);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while starting work task {Id}", id);
            throw;
        }
    }

    public async Task<bool> PauseWorkTaskAsync(int id)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var workTask = workTasks.FirstOrDefault(t => t.Id == id);
            
            if (workTask == null)
            {
                _logger.LogWarning("Work task with ID {Id} not found for pausing", id);
                return false;
            }

            if (workTask.Status == Models.TaskStatus.InProgress)
            {
                workTask.Status = Models.TaskStatus.OnHold;
                workTask.UpdatedAt = DateTime.UtcNow;

                await _dataService.SaveWorkTasksAsync(workTasks);
                _logger.LogInformation("Work task with ID {Id} paused", id);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while pausing work task {Id}", id);
            throw;
        }
    }

    public async Task<bool> CompleteWorkTaskAsync(int id, decimal? actualHours = null)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var workTask = workTasks.FirstOrDefault(t => t.Id == id);
            
            if (workTask == null)
            {
                _logger.LogWarning("Work task with ID {Id} not found for completion", id);
                return false;
            }

            if (workTask.Status != Models.TaskStatus.Completed)
            {
                workTask.Status = Models.TaskStatus.Completed;
                workTask.CompletedAt = DateTime.UtcNow;
                workTask.CompletedDate = DateTime.UtcNow;
                
                if (actualHours.HasValue)
                {
                    workTask.ActualHours = actualHours.Value;
                }
                
                workTask.UpdatedAt = DateTime.UtcNow;

                await _dataService.SaveWorkTasksAsync(workTasks);
                _logger.LogInformation("Work task with ID {Id} completed", id);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing work task {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteWorkTaskAsync(int id)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var workTask = workTasks.FirstOrDefault(t => t.Id == id);
            
            if (workTask == null)
            {
                _logger.LogWarning("Work task with ID {Id} not found for deletion", id);
                return false;
            }

            workTasks.Remove(workTask);
            await _dataService.SaveWorkTasksAsync(workTasks);
            _logger.LogInformation("Work task with ID {Id} deleted", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting work task with ID {Id}", id);
            throw;
        }
    }

    public async Task<int> BatchUpdateStatusAsync(List<int> workTaskIds, string status)
    {
        try
        {
            if (!_validStatuses.Contains(status))
            {
                throw new InvalidOperationException($"無效的狀態: {status}");
            }

            var workTasks = await _dataService.GetWorkTasksAsync();
            var tasksToUpdate = workTasks.Where(t => workTaskIds.Contains(t.Id)).ToList();
            
            var updatedCount = 0;
            foreach (var task in tasksToUpdate)
            {
                if (Enum.TryParse<Models.TaskStatus>(status, true, out var taskStatus))
                {
                    task.Status = taskStatus;
                    task.UpdatedAt = DateTime.UtcNow;
                    
                    // 處理完成狀態
                    if (taskStatus == Models.TaskStatus.Completed && !task.CompletedAt.HasValue)
                    {
                        task.CompletedAt = DateTime.UtcNow;
                        task.CompletedDate = DateTime.UtcNow;
                    }
                    
                    updatedCount++;
                }
            }

            if (updatedCount > 0)
            {
                await _dataService.SaveWorkTasksAsync(workTasks);
                _logger.LogInformation("Batch updated {Count} work tasks to status {Status}", updatedCount, status);
            }
            
            return updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while batch updating work tasks status");
            throw;
        }
    }

    public async Task<object> GetWorkTaskStatsAsync(int userId)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var userTasks = workTasks.Where(t => t.UserId == userId);
            var now = DateTime.Now;
            var today = DateTime.Today;
            var thisWeek = today.AddDays(-(int)today.DayOfWeek);
            var thisMonth = new DateTime(now.Year, now.Month, 1);

            var stats = new
            {
                TotalTasks = userTasks.Count(),
                CompletedTasks = userTasks.Count(t => t.Status == Models.TaskStatus.Completed),
                InProgressTasks = userTasks.Count(t => t.Status == Models.TaskStatus.InProgress),
                PendingTasks = userTasks.Count(t => t.Status == Models.TaskStatus.Pending),
                
                OverdueTasks = userTasks.Count(t => 
                    t.DueDate.HasValue && t.DueDate.Value < now && t.Status != Models.TaskStatus.Completed),
                
                CompletionRate = userTasks.Any() ? 
                    Math.Round((double)userTasks.Count(t => t.Status == Models.TaskStatus.Completed) / userTasks.Count() * 100, 2) : 0,
                
                TasksByStatus = userTasks
                    .GroupBy(t => t.Status.ToString())
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count),
                
                TasksByPriority = userTasks
                    .GroupBy(t => t.Priority.ToString())
                    .Select(g => new { Priority = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count),
                
                TasksByProject = userTasks
                    .Where(t => !string.IsNullOrEmpty(t.Project))
                    .GroupBy(t => t.Project)
                    .Select(g => new { Project = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10),
                
                TotalEstimatedHours = userTasks
                    .Where(t => t.EstimatedHours.HasValue)
                    .Sum(t => t.EstimatedHours!.Value),
                
                TotalActualHours = userTasks
                    .Where(t => t.ActualHours.HasValue)
                    .Sum(t => t.ActualHours!.Value),
                
                AverageTaskDuration = userTasks
                    .Where(t => t.Status == Models.TaskStatus.Completed && t.StartDate.HasValue && t.CompletedAt.HasValue)
                    .Select(t => (t.CompletedAt!.Value - t.StartDate!.Value).TotalDays)
                    .DefaultIfEmpty(0)
                    .Average(),
                
                TasksCreatedThisWeek = userTasks.Count(t => t.CreatedAt >= thisWeek),
                TasksCreatedThisMonth = userTasks.Count(t => t.CreatedAt >= thisMonth),
                TasksCompletedThisWeek = userTasks.Count(t => 
                    t.CompletedAt.HasValue && t.CompletedAt.Value >= thisWeek),
                TasksCompletedThisMonth = userTasks.Count(t => 
                    t.CompletedAt.HasValue && t.CompletedAt.Value >= thisMonth),
                
                ProductivityScore = CalculateProductivityScore(userTasks),
                
                UniqueProjects = userTasks
                    .Where(t => !string.IsNullOrEmpty(t.Project))
                    .Select(t => t.Project)
                    .Distinct()
                    .Count()
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting work task statistics for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetProjectsAsync(int userId)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var projects = workTasks
                .Where(t => t.UserId == userId && !string.IsNullOrEmpty(t.Project))
                .Select(t => t.Project!)
                .Distinct()
                .OrderBy(p => p);
            
            return projects;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting projects for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetTagsAsync(int userId)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var allTags = workTasks
                .Where(t => t.UserId == userId && !string.IsNullOrEmpty(t.Tags))
                .SelectMany(t => t.Tags!.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(tag => tag.Trim())
                .Where(tag => !string.IsNullOrEmpty(tag))
                .Distinct()
                .OrderBy(tag => tag);
            
            return allTags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting tags for user {UserId}", userId);
            throw;
        }
    }

    public async Task<object> GetWorkloadStatsAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var workTasks = await _dataService.GetWorkTasksAsync();
            var userTasks = workTasks.Where(t => t.UserId == userId);

            // 如果沒有指定日期範圍，預設為本月
            if (!startDate.HasValue || !endDate.HasValue)
            {
                var now = DateTime.Now;
                startDate = new DateTime(now.Year, now.Month, 1);
                endDate = startDate.Value.AddMonths(1).AddDays(-1);
            }

            var tasksInRange = userTasks.Where(t =>
                (t.StartDate.HasValue && t.StartDate.Value >= startDate && t.StartDate.Value <= endDate) ||
                (t.DueDate.HasValue && t.DueDate.Value >= startDate && t.DueDate.Value <= endDate) ||
                (t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            );

            var workloadStats = new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                
                TotalTasks = tasksInRange.Count(),
                TotalEstimatedHours = tasksInRange
                    .Where(t => t.EstimatedHours.HasValue)
                    .Sum(t => t.EstimatedHours!.Value),
                TotalActualHours = tasksInRange
                    .Where(t => t.ActualHours.HasValue)
                    .Sum(t => t.ActualHours!.Value),
                
                WorkloadByWeek = tasksInRange
                    .GroupBy(t => GetWeekOfYear(t.StartDate ?? t.CreatedAt))
                    .Select(g => new {
                        Week = g.Key,
                        TaskCount = g.Count(),
                        EstimatedHours = g.Where(t => t.EstimatedHours.HasValue).Sum(t => t.EstimatedHours!.Value),
                        ActualHours = g.Where(t => t.ActualHours.HasValue).Sum(t => t.ActualHours!.Value)
                    })
                    .OrderBy(x => x.Week),
                
                WorkloadByProject = tasksInRange
                    .Where(t => !string.IsNullOrEmpty(t.Project))
                    .GroupBy(t => t.Project)
                    .Select(g => new {
                        Project = g.Key,
                        TaskCount = g.Count(),
                        EstimatedHours = g.Where(t => t.EstimatedHours.HasValue).Sum(t => t.EstimatedHours!.Value),
                        ActualHours = g.Where(t => t.ActualHours.HasValue).Sum(t => t.ActualHours!.Value)
                    })
                    .OrderByDescending(x => x.EstimatedHours),
                
                EfficiencyRatio = CalculateEfficiencyRatio(tasksInRange)
            };

            return workloadStats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting workload statistics for user {UserId}", userId);
            throw;
        }
    }

    public bool ValidateTaskDates(DateTime? startDate, DateTime? dueDate)
    {
        try
        {
            // 允許空值
            if (!startDate.HasValue || !dueDate.HasValue)
                return true;

            // 到期日期不能早於開始日期
            if (dueDate.Value < startDate.Value)
                return false;

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while validating task dates");
            return false;
        }
    }

    private static double CalculateProductivityScore(IEnumerable<Models.WorkTask> tasks)
    {
        var completedTasks = tasks.Where(t => t.Status == Models.TaskStatus.Completed).ToList();
        if (!completedTasks.Any()) return 0;

        var onTimeCompletions = completedTasks.Count(t => 
            !t.DueDate.HasValue || (t.CompletedAt.HasValue && t.CompletedAt.Value <= t.DueDate.Value));
        
        var timeEfficiency = completedTasks
            .Where(t => t.EstimatedHours.HasValue && t.ActualHours.HasValue && t.EstimatedHours.Value > 0)
            .Select(t => Math.Min(1.0, (double)t.EstimatedHours!.Value / (double)t.ActualHours!.Value))
            .DefaultIfEmpty(1.0)
            .Average();

        var completionRate = (double)completedTasks.Count / tasks.Count();
        var onTimeRate = (double)onTimeCompletions / completedTasks.Count;

        return Math.Round((completionRate * 0.4 + onTimeRate * 0.3 + timeEfficiency * 0.3) * 100, 2);
    }

    private static double CalculateEfficiencyRatio(IEnumerable<Models.WorkTask> tasks)
    {
        var tasksWithHours = tasks
            .Where(t => t.EstimatedHours.HasValue && t.ActualHours.HasValue && t.EstimatedHours.Value > 0)
            .ToList();

        if (!tasksWithHours.Any()) return 1.0;

        var totalEstimated = tasksWithHours.Sum(t => (double)t.EstimatedHours!.Value);
        var totalActual = tasksWithHours.Sum(t => (double)t.ActualHours!.Value);

        return totalActual > 0 ? Math.Round(totalEstimated / totalActual, 2) : 1.0;
    }

    private static string GetWeekOfYear(DateTime date)
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        var weekOfYear = culture.Calendar.GetWeekOfYear(date, 
            System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        return $"{date.Year}-W{weekOfYear:D2}";
    }
}