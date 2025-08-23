using PersonalManagerAPI.DTOs.WorkTask;

namespace PersonalManagerAPI.Services.Interfaces;

/// <summary>
/// 工作任務服務介面
/// </summary>
public interface IWorkTaskService
{
    /// <summary>
    /// 取得所有工作任務（分頁）
    /// </summary>
    Task<IEnumerable<WorkTaskResponseDto>> GetAllWorkTasksAsync(int skip = 0, int take = 50);

    /// <summary>
    /// 根據ID取得工作任務
    /// </summary>
    Task<WorkTaskResponseDto?> GetWorkTaskByIdAsync(int id);

    /// <summary>
    /// 取得指定使用者的工作任務
    /// </summary>
    Task<IEnumerable<WorkTaskResponseDto>> GetWorkTasksByUserIdAsync(int userId);

    /// <summary>
    /// 根據狀態篩選工作任務
    /// </summary>
    Task<IEnumerable<WorkTaskResponseDto>> GetWorkTasksByStatusAsync(string status, int userId);

    /// <summary>
    /// 根據優先級篩選工作任務
    /// </summary>
    Task<IEnumerable<WorkTaskResponseDto>> GetWorkTasksByPriorityAsync(string priority, int userId);

    /// <summary>
    /// 根據專案篩選工作任務
    /// </summary>
    Task<IEnumerable<WorkTaskResponseDto>> GetWorkTasksByProjectAsync(string project, int userId);

    /// <summary>
    /// 取得逾期的工作任務
    /// </summary>
    Task<IEnumerable<WorkTaskResponseDto>> GetOverdueWorkTasksAsync(int userId);

    /// <summary>
    /// 取得進行中的工作任務
    /// </summary>
    Task<IEnumerable<WorkTaskResponseDto>> GetActiveWorkTasksAsync(int userId);

    /// <summary>
    /// 取得已完成的工作任務
    /// </summary>
    Task<IEnumerable<WorkTaskResponseDto>> GetCompletedWorkTasksAsync(int userId);

    /// <summary>
    /// 根據日期範圍搜尋工作任務
    /// </summary>
    Task<IEnumerable<WorkTaskResponseDto>> GetWorkTasksByDateRangeAsync(DateTime startDate, DateTime endDate, int userId);

    /// <summary>
    /// 搜尋工作任務
    /// </summary>
    Task<IEnumerable<WorkTaskResponseDto>> SearchWorkTasksAsync(string keyword, int userId);

    /// <summary>
    /// 根據標籤搜尋工作任務
    /// </summary>
    Task<IEnumerable<WorkTaskResponseDto>> SearchWorkTasksByTagsAsync(string tags, int userId);

    /// <summary>
    /// 建立新的工作任務
    /// </summary>
    Task<WorkTaskResponseDto> CreateWorkTaskAsync(CreateWorkTaskDto createWorkTaskDto);

    /// <summary>
    /// 更新工作任務
    /// </summary>
    Task<WorkTaskResponseDto?> UpdateWorkTaskAsync(int id, UpdateWorkTaskDto updateWorkTaskDto);

    /// <summary>
    /// 開始工作任務
    /// </summary>
    Task<bool> StartWorkTaskAsync(int id);

    /// <summary>
    /// 暫停工作任務
    /// </summary>
    Task<bool> PauseWorkTaskAsync(int id);

    /// <summary>
    /// 完成工作任務
    /// </summary>
    Task<bool> CompleteWorkTaskAsync(int id, decimal? actualHours = null);

    /// <summary>
    /// 刪除工作任務
    /// </summary>
    Task<bool> DeleteWorkTaskAsync(int id);

    /// <summary>
    /// 批量更新工作任務狀態
    /// </summary>
    Task<int> BatchUpdateStatusAsync(List<int> workTaskIds, string status);

    /// <summary>
    /// 取得工作任務統計資訊
    /// </summary>
    Task<object> GetWorkTaskStatsAsync(int userId);

    /// <summary>
    /// 取得專案列表
    /// </summary>
    Task<IEnumerable<string>> GetProjectsAsync(int userId);

    /// <summary>
    /// 取得標籤列表
    /// </summary>
    Task<IEnumerable<string>> GetTagsAsync(int userId);

    /// <summary>
    /// 計算工作量統計
    /// </summary>
    Task<object> GetWorkloadStatsAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// 驗證任務日期
    /// </summary>
    bool ValidateTaskDates(DateTime? startDate, DateTime? dueDate);
}