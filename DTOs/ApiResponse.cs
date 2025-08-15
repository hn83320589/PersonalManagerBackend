namespace PersonalManagerAPI.DTOs;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new List<string>();

    public static ApiResponse<T> Success(T data, string message = "操作成功")
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Failure(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    // 保留舊方法名稱以保持向後相容性
    public static ApiResponse<T> SuccessResult(T data, string message = "操作成功")
    {
        return Success(data, message);
    }

    public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
    {
        return Failure(message, errors);
    }
}

public class ApiResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new List<string>();

    public static ApiResponse Success(string message = "操作成功")
    {
        return new ApiResponse
        {
            IsSuccess = true,
            Message = message
        };
    }

    public static ApiResponse Failure(string message, List<string>? errors = null)
    {
        return new ApiResponse
        {
            IsSuccess = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    // 保留舊方法名稱以保持向後相容性
    public static ApiResponse SuccessResult(string message = "操作成功")
    {
        return Success(message);
    }

    public static ApiResponse ErrorResult(string message, List<string>? errors = null)
    {
        return Failure(message, errors);
    }
}