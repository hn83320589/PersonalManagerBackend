using System.Diagnostics;
using System.Text;

namespace PersonalManagerAPI.Middleware
{
    /// <summary>
    /// 請求日誌中介軟體，記錄所有HTTP請求和回應的詳細資訊
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString("N")[..8];
            
            // 記錄請求開始
            await LogRequestAsync(context, requestId);

            // 替換回應流以捕獲回應內容
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                
                // 記錄回應結束
                await LogResponseAsync(context, requestId, stopwatch.ElapsedMilliseconds);
                
                // 將回應內容複製回原始流
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task LogRequestAsync(HttpContext context, string requestId)
        {
            var request = context.Request;
            
            var logMessage = new StringBuilder();
            logMessage.AppendLine($"[{requestId}] HTTP Request Information:");
            logMessage.AppendLine($"Method: {request.Method}");
            logMessage.AppendLine($"Path: {request.Path}");
            logMessage.AppendLine($"QueryString: {request.QueryString}");
            logMessage.AppendLine($"Headers: {string.Join(", ", request.Headers.Select(h => $"{h.Key}={h.Value}"))}");
            
            // 記錄請求體 (只對POST、PUT、PATCH請求且內容類型為JSON)
            if (ShouldLogRequestBody(request))
            {
                request.EnableBuffering();
                var body = await ReadStreamAsync(request.Body);
                request.Body.Position = 0;
                
                if (!string.IsNullOrWhiteSpace(body))
                {
                    logMessage.AppendLine($"Body: {body}");
                }
            }

            _logger.LogInformation(logMessage.ToString());
        }

        private async Task LogResponseAsync(HttpContext context, string requestId, long elapsedMs)
        {
            var response = context.Response;
            
            var logMessage = new StringBuilder();
            logMessage.AppendLine($"[{requestId}] HTTP Response Information:");
            logMessage.AppendLine($"StatusCode: {response.StatusCode}");
            logMessage.AppendLine($"ElapsedTime: {elapsedMs}ms");
            logMessage.AppendLine($"Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}={h.Value}"))}");
            
            // 記錄回應體 (只對錯誤狀態碼或開發環境)
            if (ShouldLogResponseBody(response))
            {
                response.Body.Seek(0, SeekOrigin.Begin);
                var body = await ReadStreamAsync(response.Body);
                response.Body.Seek(0, SeekOrigin.Begin);
                
                if (!string.IsNullOrWhiteSpace(body))
                {
                    logMessage.AppendLine($"Body: {body}");
                }
            }

            var logLevel = response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
            _logger.Log(logLevel, logMessage.ToString());
        }

        private static bool ShouldLogRequestBody(HttpRequest request)
        {
            return (request.Method == "POST" || request.Method == "PUT" || request.Method == "PATCH") &&
                   request.ContentType?.Contains("application/json") == true &&
                   request.ContentLength > 0;
        }

        private static bool ShouldLogResponseBody(HttpResponse response)
        {
            // 記錄錯誤回應或開發環境的所有回應
            return response.StatusCode >= 400;
        }

        private static async Task<string> ReadStreamAsync(Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            return await reader.ReadToEndAsync();
        }
    }
}