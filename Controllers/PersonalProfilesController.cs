using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonalProfilesController : BaseController
{
    private readonly JsonDataService _dataService;

    public PersonalProfilesController(JsonDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PersonalProfile>>>> GetProfiles()
    {
        try
        {
            var profiles = await _dataService.GetPersonalProfilesAsync();
            return Ok(ApiResponse<List<PersonalProfile>>.SuccessResult(profiles, "成功取得個人資料列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<PersonalProfile>>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PersonalProfile>>> GetProfile(int id)
    {
        try
        {
            var profile = await _dataService.GetByIdAsync<PersonalProfile>("personalProfiles.json", id);
            
            if (profile == null)
            {
                return NotFound(ApiResponse<PersonalProfile>.ErrorResult("找不到指定的個人資料"));
            }

            return Ok(ApiResponse<PersonalProfile>.SuccessResult(profile, "成功取得個人資料"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PersonalProfile>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<PersonalProfile>>> GetProfileByUserId(int userId)
    {
        try
        {
            var profiles = await _dataService.GetPersonalProfilesAsync();
            var profile = profiles.FirstOrDefault(p => p.UserId == userId);
            
            if (profile == null)
            {
                return NotFound(ApiResponse<PersonalProfile>.ErrorResult("找不到該使用者的個人資料"));
            }

            return Ok(ApiResponse<PersonalProfile>.SuccessResult(profile, "成功取得個人資料"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PersonalProfile>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpGet("public")]
    public async Task<ActionResult<ApiResponse<List<PersonalProfile>>>> GetPublicProfiles()
    {
        try
        {
            var profiles = await _dataService.GetPersonalProfilesAsync();
            var publicProfiles = profiles.Where(p => p.IsPublic).ToList();
            
            return Ok(ApiResponse<List<PersonalProfile>>.SuccessResult(publicProfiles, "成功取得公開個人資料"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<PersonalProfile>>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PersonalProfile>>> CreateProfile([FromBody] PersonalProfile profile)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();
                return BadRequest(ApiResponse<PersonalProfile>.ErrorResult("資料驗證失敗", errors));
            }

            var profiles = await _dataService.GetPersonalProfilesAsync();
            
            // Check if user already has a profile
            if (profile.UserId.HasValue && profiles.Any(p => p.UserId == profile.UserId))
            {
                return BadRequest(ApiResponse<PersonalProfile>.ErrorResult("該使用者已有個人資料"));
            }

            // Generate new ID
            profile.Id = profiles.Any() ? profiles.Max(p => p.Id) + 1 : 1;
            profile.CreatedAt = DateTime.UtcNow;
            profile.UpdatedAt = DateTime.UtcNow;
            
            profiles.Add(profile);
            await _dataService.SavePersonalProfilesAsync(profiles);

            return CreatedAtAction(nameof(GetProfile), new { id = profile.Id }, 
                ApiResponse<PersonalProfile>.SuccessResult(profile, "個人資料建立成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PersonalProfile>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<PersonalProfile>>> UpdateProfile(int id, [FromBody] PersonalProfile profile)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();
                return BadRequest(ApiResponse<PersonalProfile>.ErrorResult("資料驗證失敗", errors));
            }

            var profiles = await _dataService.GetPersonalProfilesAsync();
            var existingProfile = profiles.FirstOrDefault(p => p.Id == id);
            
            if (existingProfile == null)
            {
                return NotFound(ApiResponse<PersonalProfile>.ErrorResult("找不到指定的個人資料"));
            }

            // Update profile properties
            existingProfile.Title = profile.Title;
            existingProfile.Summary = profile.Summary;
            existingProfile.Description = profile.Description;
            existingProfile.ProfileImageUrl = profile.ProfileImageUrl;
            existingProfile.Website = profile.Website;
            existingProfile.Location = profile.Location;
            existingProfile.Birthday = profile.Birthday;
            existingProfile.IsPublic = profile.IsPublic;
            existingProfile.UpdatedAt = DateTime.UtcNow;

            await _dataService.SavePersonalProfilesAsync(profiles);

            return Ok(ApiResponse<PersonalProfile>.SuccessResult(existingProfile, "個人資料更新成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PersonalProfile>.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteProfile(int id)
    {
        try
        {
            var profiles = await _dataService.GetPersonalProfilesAsync();
            var profile = profiles.FirstOrDefault(p => p.Id == id);
            
            if (profile == null)
            {
                return NotFound(ApiResponse.ErrorResult("找不到指定的個人資料"));
            }

            profiles.Remove(profile);
            await _dataService.SavePersonalProfilesAsync(profiles);

            return Ok(ApiResponse.SuccessResult("個人資料刪除成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.ErrorResult($"伺服器錯誤: {ex.Message}"));
        }
    }
}