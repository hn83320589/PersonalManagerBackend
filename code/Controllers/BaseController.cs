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
    }
}