namespace PersonalManagerAPI.Services;

/// <summary>
/// 服務層統一回應結果
/// </summary>
public class ServiceResult<T>
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 回應訊息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 回應資料
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 錯誤訊息列表
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// 成功結果
    /// </summary>
    public static ServiceResult<T> Success(T data, string message = "操作成功")
    {
        return new ServiceResult<T>
        {
            IsSuccess = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// 失敗結果
    /// </summary>
    public static ServiceResult<T> Failure(string message, List<string>? errors = null)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    /// <summary>
    /// 失敗結果 (單一錯誤訊息)
    /// </summary>
    public static ServiceResult<T> Failure(string message, string error)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            Message = message,
            Errors = new List<string> { error }
        };
    }
}
