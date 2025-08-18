using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactMethodsController : BaseController
{
    private readonly JsonDataService _dataService;

    public ContactMethodsController(JsonDataService dataService)
    {
        _dataService = dataService;
    }

    /// <summary>
    /// 取得所有公開聯絡方式
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetContactMethods()
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var publicMethods = contactMethods
                .Where(c => c.IsPublic)
                .OrderBy(c => c.SortOrder)
                .ToList();
                
            return Ok(ApiResponse<List<ContactMethod>>.SuccessResult(publicMethods, "成功取得聯絡方式列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<ContactMethod>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得指定聯絡方式
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetContactMethod(int id)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var contactMethod = contactMethods.FirstOrDefault(c => c.Id == id);
            
            if (contactMethod == null)
            {
                return NotFound(ApiResponse<ContactMethod>.ErrorResult("找不到指定的聯絡方式"));
            }

            return Ok(ApiResponse<ContactMethod>.SuccessResult(contactMethod, "成功取得聯絡方式資料"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ContactMethod>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得指定使用者的聯絡方式
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetContactMethodsByUser(int userId, [FromQuery] bool includePrivate = false)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var query = contactMethods.Where(c => c.UserId == userId);

            if (!includePrivate)
            {
                query = query.Where(c => c.IsPublic);
            }

            var userContactMethods = query
                .OrderBy(c => c.SortOrder)
                .ToList();
                
            return Ok(ApiResponse<List<ContactMethod>>.SuccessResult(userContactMethods, "成功取得使用者聯絡方式列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<ContactMethod>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 依聯絡方式類型篩選
    /// </summary>
    [HttpGet("type/{type}")]
    public async Task<IActionResult> GetContactMethodsByType(string type)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var filteredMethods = contactMethods
                .Where(c => c.IsPublic && 
                           c.Type.ToString().Equals(type, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => c.SortOrder)
                .ToList();
                
            return Ok(ApiResponse<List<ContactMethod>>.SuccessResult(filteredMethods, $"成功取得 {type} 類型聯絡方式"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<ContactMethod>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得社群媒體聯絡方式
    /// </summary>
    [HttpGet("social")]
    public async Task<IActionResult> GetSocialMediaContacts()
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var socialTypes = new[] { "Facebook", "Twitter", "Instagram", "LinkedIn", "GitHub", "YouTube" };
            
            var socialMethods = contactMethods
                .Where(c => c.IsPublic && 
                           socialTypes.Contains(c.Type.ToString(), StringComparer.OrdinalIgnoreCase))
                .OrderBy(c => c.SortOrder)
                .ToList();
                
            return Ok(ApiResponse<List<ContactMethod>>.SuccessResult(socialMethods, "成功取得社群媒體聯絡方式"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<ContactMethod>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得基本聯絡資訊
    /// </summary>
    [HttpGet("basic")]
    public async Task<IActionResult> GetBasicContactInfo()
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var basicTypes = new[] { "Email", "Phone", "Address" };
            
            var basicMethods = contactMethods
                .Where(c => c.IsPublic && 
                           basicTypes.Contains(c.Type.ToString(), StringComparer.OrdinalIgnoreCase))
                .OrderBy(c => c.SortOrder)
                .ToList();
                
            return Ok(ApiResponse<List<ContactMethod>>.SuccessResult(basicMethods, "成功取得基本聯絡資訊"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<ContactMethod>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 建立聯絡方式
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateContactMethod([FromBody] ContactMethod contactMethod)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();

            // 驗證必要欄位
            if (string.IsNullOrEmpty(contactMethod.Value))
            {
                return BadRequest(ApiResponse<ContactMethod>.ErrorResult("類型和內容為必填項目"));
            }

            // 驗證Email格式
            if (contactMethod.Type == ContactType.Email && 
                !IsValidEmail(contactMethod.Value))
            {
                return BadRequest(ApiResponse<ContactMethod>.ErrorResult("Email格式不正確"));
            }
            
            contactMethod.Id = contactMethods.Count > 0 ? contactMethods.Max(c => c.Id) + 1 : 1;
            contactMethod.CreatedAt = DateTime.UtcNow;
            contactMethod.UpdatedAt = DateTime.UtcNow;
            
            contactMethods.Add(contactMethod);
            await _dataService.SaveContactMethodsAsync(contactMethods);

            return Ok(ApiResponse<ContactMethod>.SuccessResult(contactMethod, "聯絡方式建立成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ContactMethod>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 更新聯絡方式
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContactMethod(int id, [FromBody] ContactMethod updatedMethod)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var contactMethod = contactMethods.FirstOrDefault(c => c.Id == id);
            
            if (contactMethod == null)
            {
                return NotFound(ApiResponse<ContactMethod>.ErrorResult("找不到指定的聯絡方式"));
            }

            // 驗證Email格式
            if (updatedMethod.Type == ContactType.Email && 
                !IsValidEmail(updatedMethod.Value))
            {
                return BadRequest(ApiResponse<ContactMethod>.ErrorResult("Email格式不正確"));
            }

            contactMethod.Type = updatedMethod.Type;
            contactMethod.Label = updatedMethod.Label;
            contactMethod.Value = updatedMethod.Value;
            contactMethod.Icon = updatedMethod.Icon;
            contactMethod.IsPublic = updatedMethod.IsPublic;
            contactMethod.SortOrder = updatedMethod.SortOrder;
            contactMethod.UpdatedAt = DateTime.UtcNow;

            await _dataService.SaveContactMethodsAsync(contactMethods);

            return Ok(ApiResponse<ContactMethod>.SuccessResult(contactMethod, "聯絡方式更新成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ContactMethod>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 刪除聯絡方式
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContactMethod(int id)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            var contactMethod = contactMethods.FirstOrDefault(c => c.Id == id);
            
            if (contactMethod == null)
            {
                return NotFound(ApiResponse.ErrorResult("找不到指定的聯絡方式"));
            }

            contactMethods.Remove(contactMethod);
            await _dataService.SaveContactMethodsAsync(contactMethods);

            return Ok(ApiResponse.SuccessResult("聯絡方式刪除成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 批量更新排序
    /// </summary>
    [HttpPut("reorder")]
    public async Task<IActionResult> ReorderContactMethods([FromBody] List<int> orderedIds)
    {
        try
        {
            var contactMethods = await _dataService.GetContactMethodsAsync();
            
            for (int i = 0; i < orderedIds.Count; i++)
            {
                var method = contactMethods.FirstOrDefault(c => c.Id == orderedIds[i]);
                if (method != null)
                {
                    method.SortOrder = i + 1;
                    method.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _dataService.SaveContactMethodsAsync(contactMethods);

            return Ok(ApiResponse.SuccessResult("聯絡方式排序更新成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}