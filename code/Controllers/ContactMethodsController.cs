using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;
using PersonalManagerAPI.Services.Interfaces;
using PersonalManagerAPI.DTOs.ContactMethod;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactMethodsController : BaseController
{
    private readonly IContactMethodService _contactMethodService;

    public ContactMethodsController(IContactMethodService contactMethodService)
    {
        _contactMethodService = contactMethodService;
    }

    /// <summary>
    /// 取得所有公開聯絡方式
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetContactMethods()
    {
        try
        {
            var publicMethods = await _contactMethodService.GetPublicContactMethodsAsync();
                
            return Ok(ApiResponse<object>.SuccessResult(publicMethods, "成功取得聯絡方式列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("伺服器錯誤: " + ex.Message));
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
            var contactMethod = await _contactMethodService.GetContactMethodByIdAsync(id);
            
            if (contactMethod == null)
            {
                return NotFound(ApiResponse<object>.ErrorResult("找不到指定的聯絡方式"));
            }

            return Ok(ApiResponse<object>.SuccessResult(contactMethod, "成功取得聯絡方式資料"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("伺服器錯誤: " + ex.Message));
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
            IEnumerable<ContactMethodResponseDto> userContactMethods;
            if (includePrivate)
            {
                userContactMethods = await _contactMethodService.GetContactMethodsByUserIdAsync(userId);
            }
            else
            {
                userContactMethods = await _contactMethodService.GetPublicContactMethodsAsync(userId);
            }
                
            return Ok(ApiResponse<object>.SuccessResult(userContactMethods, "成功取得使用者聯絡方式列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("伺服器錯誤: " + ex.Message));
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
            // 解析 ContactType 枚舉
            if (!Enum.TryParse<ContactType>(type, true, out var contactType))
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"不支援的聯絡方式類型: {type}"));
            }

            var filteredMethods = await _contactMethodService.GetContactMethodsByTypeAsync(contactType);
                
            return Ok(ApiResponse<object>.SuccessResult(filteredMethods, $"成功取得 {type} 類型聯絡方式"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("伺服器錯誤: " + ex.Message));
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
            var socialTypes = new[] { 
                ContactType.Facebook, 
                ContactType.Twitter, 
                ContactType.Instagram, 
                ContactType.LinkedIn, 
                ContactType.GitHub
            };
            
            var allSocialMethods = new List<ContactMethodResponseDto>();
            foreach (var type in socialTypes)
            {
                var methods = await _contactMethodService.GetContactMethodsByTypeAsync(type);
                allSocialMethods.AddRange(methods);
            }
                
            return Ok(ApiResponse<object>.SuccessResult(allSocialMethods, "成功取得社群媒體聯絡方式"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("伺服器錯誤: " + ex.Message));
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
            var basicTypes = new[] { 
                ContactType.Email, 
                ContactType.Phone
            };
            
            var allBasicMethods = new List<ContactMethodResponseDto>();
            foreach (var type in basicTypes)
            {
                var methods = await _contactMethodService.GetContactMethodsByTypeAsync(type);
                allBasicMethods.AddRange(methods);
            }
                
            return Ok(ApiResponse<object>.SuccessResult(allBasicMethods, "成功取得基本聯絡資訊"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 建立聯絡方式
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateContactMethod([FromBody] CreateContactMethodDto createContactMethodDto)
    {
        try
        {
            var contactMethod = await _contactMethodService.CreateContactMethodAsync(createContactMethodDto);

            return Ok(ApiResponse<object>.SuccessResult(contactMethod, "聯絡方式建立成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 更新聯絡方式
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContactMethod(int id, [FromBody] UpdateContactMethodDto updateContactMethodDto)
    {
        try
        {
            var contactMethod = await _contactMethodService.UpdateContactMethodAsync(id, updateContactMethodDto);
            
            if (contactMethod == null)
            {
                return NotFound(ApiResponse<object>.ErrorResult("找不到指定的聯絡方式"));
            }

            return Ok(ApiResponse<object>.SuccessResult(contactMethod, "聯絡方式更新成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("伺服器錯誤: " + ex.Message));
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
            var success = await _contactMethodService.DeleteContactMethodAsync(id);
            
            if (!success)
            {
                return NotFound(ApiResponse.ErrorResult("找不到指定的聯絡方式"));
            }

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
            var orders = new Dictionary<int, int>();
            for (int i = 0; i < orderedIds.Count; i++)
            {
                orders[orderedIds[i]] = i + 1;
            }
            
            var success = await _contactMethodService.BatchUpdateContactMethodOrderAsync(orders);
            
            if (!success)
            {
                return BadRequest(ApiResponse.ErrorResult("排序更新失敗"));
            }

            return Ok(ApiResponse.SuccessResult("聯絡方式排序更新成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

}