using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;

namespace PersonalManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// 統一錯誤處理
        /// </summary>
        /// <param name="ex">例外</param>
        /// <param name="message">錯誤訊息</param>
        /// <returns>錯誤回應</returns>
        protected ActionResult<ApiResponse<T>> HandleError<T>(Exception ex, string message)
        {
            // 記錄詳細錯誤資訊
            Console.WriteLine($"[ERROR] {message}: {ex.Message}");
            Console.WriteLine($"[ERROR] Stack Trace: {ex.StackTrace}");

            return StatusCode(500, ApiResponse<T>.Failure($"{message}: {ex.Message}"));
        }

        /// <summary>
        /// 獲取模型驗證錯誤
        /// </summary>
        /// <returns>驗證錯誤列表</returns>
        protected List<string> GetModelErrors()
        {
            return ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();
        }

        /// <summary>
        /// 建立成功回應
        /// </summary>
        protected IActionResult CreateSuccessResponse<T>(T data, string message = "操作成功")
        {
            return Ok(ApiResponse<T>.Success(data, message));
        }

        /// <summary>
        /// 建立錯誤回應
        /// </summary>
        protected IActionResult CreateErrorResponse(string message, List<string>? errors = null)
        {
            return BadRequest(ApiResponse<object>.Failure(message, errors));
        }

        /// <summary>
        /// 建立錯誤回應（單一錯誤訊息）
        /// </summary>
        protected IActionResult CreateSingleErrorResponse(string message, string error)
        {
            return BadRequest(ApiResponse<object>.Failure(message, new List<string> { error }));
        }

        /// <summary>
        /// 建立服務層結果回應
        /// </summary>
        protected IActionResult CreateResponse<T>(Services.ServiceResult<T> result)
        {
            return result.IsSuccess
                ? Ok(ApiResponse<T>.Success(result.Data!, result.Message))
                : BadRequest(ApiResponse<T>.Failure(result.Message, result.Errors));
        }

        /// <summary>
        /// 獲取模型狀態錯誤
        /// </summary>
        protected List<string> GetModelStateErrors()
        {
            return GetModelErrors();
        }
    }
}