using System.Net;
using System.Text.Json;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Middleware.Exceptions;

namespace PersonalManagerAPI.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = new ApiResponse<object>();
            
            switch (exception)
            {
                case ValidationException ex:
                    response.IsSuccess = false;
                    response.Message = "資料驗證失敗";
                    response.Errors = ex.ValidationErrors.SelectMany(kvp => 
                        kvp.Value.Select(error => $"{kvp.Key}: {error}")).ToList();
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case BusinessLogicException ex:
                    response.IsSuccess = false;
                    response.Message = ex.Message;
                    response.Errors = ex.ErrorCode != null 
                        ? new List<string> { $"錯誤代碼: {ex.ErrorCode}" }
                        : new List<string> { "業務邏輯錯誤" };
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case ResourceNotFoundException ex:
                    response.IsSuccess = false;
                    response.Message = ex.Message;
                    response.Errors = new List<string> { $"資源類型: {ex.ResourceType}" };
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case ArgumentNullException ex:
                    response.IsSuccess = false;
                    response.Message = "請求參數不能為空";
                    response.Errors = new List<string> { ex.ParamName ?? "未知參數" };
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case ArgumentException ex:
                    response.IsSuccess = false;
                    response.Message = "請求參數無效";
                    response.Errors = new List<string> { ex.Message };
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case InvalidOperationException ex:
                    response.IsSuccess = false;
                    response.Message = "操作無效";
                    response.Errors = new List<string> { ex.Message };
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case UnauthorizedAccessException:
                    response.IsSuccess = false;
                    response.Message = "未授權的訪問";
                    response.Errors = new List<string> { "請確認您有執行此操作的權限" };
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;

                case FileNotFoundException ex:
                    response.IsSuccess = false;
                    response.Message = "找不到指定的檔案";
                    response.Errors = new List<string> { ex.FileName ?? "未知檔案" };
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case TimeoutException:
                    response.IsSuccess = false;
                    response.Message = "請求超時";
                    response.Errors = new List<string> { "請稍後再試" };
                    context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    break;

                case HttpRequestException ex:
                    response.IsSuccess = false;
                    response.Message = "外部服務請求失敗";
                    response.Errors = new List<string> { ex.Message };
                    context.Response.StatusCode = (int)HttpStatusCode.BadGateway;
                    break;

                default:
                    response.IsSuccess = false;
                    response.Message = "伺服器內部錯誤";
                    response.Errors = new List<string> { "請聯繫系統管理員" };
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var jsonResponse = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}