using PersonalManagerAPI.DTOs.TodoItem;

namespace PersonalManagerAPI.Services.Interfaces;

/// <summary>
/// 待辦事項服務介面
/// </summary>
public interface ITodoItemService
{
    /// <summary>
    /// 取得所有待辦事項（分頁）
    /// </summary>
    Task<IEnumerable<TodoItemResponseDto>> GetAllTodoItemsAsync(int skip = 0, int take = 50);

    /// <summary>
    /// 根據ID取得待辦事項
    /// </summary>
    Task<TodoItemResponseDto?> GetTodoItemByIdAsync(int id);

    /// <summary>
    /// 取得指定使用者的待辦事項
    /// </summary>
    Task<IEnumerable<TodoItemResponseDto>> GetTodoItemsByUserIdAsync(int userId);

    /// <summary>
    /// 根據狀態篩選待辦事項
    /// </summary>
    Task<IEnumerable<TodoItemResponseDto>> GetTodoItemsByStatusAsync(string status, int userId);

    /// <summary>
    /// 根據優先級篩選待辦事項
    /// </summary>
    Task<IEnumerable<TodoItemResponseDto>> GetTodoItemsByPriorityAsync(string priority, int userId);

    /// <summary>
    /// 根據分類篩選待辦事項
    /// </summary>
    Task<IEnumerable<TodoItemResponseDto>> GetTodoItemsByCategoryAsync(string category, int userId);

    /// <summary>
    /// 取得逾期的待辦事項
    /// </summary>
    Task<IEnumerable<TodoItemResponseDto>> GetOverdueTodoItemsAsync(int userId);

    /// <summary>
    /// 取得即將到期的待辦事項
    /// </summary>
    Task<IEnumerable<TodoItemResponseDto>> GetDueSoonTodoItemsAsync(int userId, int days = 3);

    /// <summary>
    /// 取得今日的待辦事項
    /// </summary>
    Task<IEnumerable<TodoItemResponseDto>> GetTodayTodoItemsAsync(int userId);

    /// <summary>
    /// 搜尋待辦事項
    /// </summary>
    Task<IEnumerable<TodoItemResponseDto>> SearchTodoItemsAsync(string keyword, int userId);

    /// <summary>
    /// 建立新的待辦事項
    /// </summary>
    Task<TodoItemResponseDto> CreateTodoItemAsync(CreateTodoItemDto createTodoItemDto);

    /// <summary>
    /// 更新待辦事項
    /// </summary>
    Task<TodoItemResponseDto?> UpdateTodoItemAsync(int id, UpdateTodoItemDto updateTodoItemDto);

    /// <summary>
    /// 標記待辦事項為完成
    /// </summary>
    Task<bool> MarkTodoItemAsCompletedAsync(int id);

    /// <summary>
    /// 標記待辦事項為未完成
    /// </summary>
    Task<bool> MarkTodoItemAsIncompleteAsync(int id);

    /// <summary>
    /// 刪除待辦事項
    /// </summary>
    Task<bool> DeleteTodoItemAsync(int id);

    /// <summary>
    /// 批量更新待辦事項狀態
    /// </summary>
    Task<int> BatchUpdateStatusAsync(List<int> todoItemIds, string status);

    /// <summary>
    /// 批量刪除待辦事項
    /// </summary>
    Task<int> BatchDeleteTodoItemsAsync(List<int> todoItemIds);

    /// <summary>
    /// 取得待辦事項統計資訊
    /// </summary>
    Task<object> GetTodoItemStatsAsync(int userId);

    /// <summary>
    /// 取得所有分類列表
    /// </summary>
    Task<IEnumerable<string>> GetCategoriesAsync(int userId);

    /// <summary>
    /// 驗證到期日期
    /// </summary>
    bool ValidateDueDate(DateTime? dueDate);
}