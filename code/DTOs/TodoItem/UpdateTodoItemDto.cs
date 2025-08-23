using System.ComponentModel.DataAnnotations;

namespace PersonalManagerAPI.DTOs.TodoItem;

public class UpdateTodoItemDto
{
    [StringLength(200, MinimumLength = 1, ErrorMessage = "標題長度必須在1-200字元之間")]
    public string? Title { get; set; }

    [StringLength(2000, ErrorMessage = "描述不可超過2000字元")]
    public string? Description { get; set; }

    [StringLength(20, ErrorMessage = "狀態長度不可超過20字元")]
    public string? Status { get; set; }

    [StringLength(20, ErrorMessage = "優先級長度不可超過20字元")]
    public string? Priority { get; set; }

    public DateTime? DueDate { get; set; }

    [StringLength(50, ErrorMessage = "分類長度不可超過50字元")]
    public string? Category { get; set; }

    [Range(0, 999999, ErrorMessage = "排序值必須在0-999999之間")]
    public int? SortOrder { get; set; }

    public bool? IsCompleted { get; set; }
}