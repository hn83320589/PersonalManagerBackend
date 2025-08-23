using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.DTOs.PersonalProfile;
using PersonalManagerAPI.Services.Interfaces;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonalProfilesController : BaseController
{
    private readonly IPersonalProfileService _profileService;

    public PersonalProfilesController(IPersonalProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<PersonalProfileResponseDto>>>> GetProfiles([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var profiles = await _profileService.GetAllProfilesAsync(skip, take);
        return Ok(ApiResponse<IEnumerable<PersonalProfileResponseDto>>.SuccessResult(profiles, "成功取得個人資料列表"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PersonalProfileResponseDto>>> GetProfile(int id)
    {
        var profile = await _profileService.GetProfileByIdAsync(id);
        
        if (profile == null)
        {
            return NotFound(ApiResponse<PersonalProfileResponseDto>.ErrorResult("找不到指定的個人資料"));
        }

        return Ok(ApiResponse<PersonalProfileResponseDto>.SuccessResult(profile, "成功取得個人資料"));
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<PersonalProfileResponseDto>>> GetProfileByUserId(int userId)
    {
        var profile = await _profileService.GetProfileByUserIdAsync(userId);
        
        if (profile == null)
        {
            return NotFound(ApiResponse<PersonalProfileResponseDto>.ErrorResult("找不到該使用者的個人資料"));
        }

        return Ok(ApiResponse<PersonalProfileResponseDto>.SuccessResult(profile, "成功取得個人資料"));
    }

    [HttpGet("public")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PersonalProfileResponseDto>>>> GetPublicProfiles([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var profiles = await _profileService.GetPublicProfilesAsync(skip, take);
        return Ok(ApiResponse<IEnumerable<PersonalProfileResponseDto>>.SuccessResult(profiles, "成功取得公開個人資料"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PersonalProfileResponseDto>>> CreateProfile([FromBody] CreatePersonalProfileDto createProfileDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
            return BadRequest(ApiResponse<PersonalProfileResponseDto>.ErrorResult("資料驗證失敗", errors));
        }

        var profile = await _profileService.CreateProfileAsync(createProfileDto);

        return CreatedAtAction(nameof(GetProfile), new { id = profile.Id }, 
            ApiResponse<PersonalProfileResponseDto>.SuccessResult(profile, "個人資料建立成功"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<PersonalProfileResponseDto>>> UpdateProfile(int id, [FromBody] UpdatePersonalProfileDto updateProfileDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
            return BadRequest(ApiResponse<PersonalProfileResponseDto>.ErrorResult("資料驗證失敗", errors));
        }

        var profile = await _profileService.UpdateProfileAsync(id, updateProfileDto);
        
        if (profile == null)
        {
            return NotFound(ApiResponse<PersonalProfileResponseDto>.ErrorResult("找不到指定的個人資料"));
        }

        return Ok(ApiResponse<PersonalProfileResponseDto>.SuccessResult(profile, "個人資料更新成功"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteProfile(int id)
    {
        var result = await _profileService.DeleteProfileAsync(id);
        
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResult("找不到指定的個人資料"));
        }

        return Ok(ApiResponse.SuccessResult("個人資料刪除成功"));
    }

    [HttpPost("{id}/toggle-public")]
    public async Task<ActionResult<ApiResponse>> TogglePublicStatus(int id)
    {
        var result = await _profileService.TogglePublicStatusAsync(id);
        
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResult("找不到指定的個人資料"));
        }

        return Ok(ApiResponse.SuccessResult("個人資料公開狀態已更新"));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PersonalProfileResponseDto>>>> SearchProfiles([FromQuery] string keyword, [FromQuery] bool publicOnly = true)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest(ApiResponse<IEnumerable<PersonalProfileResponseDto>>.ErrorResult("搜尋關鍵字不能為空"));
        }

        var profiles = await _profileService.SearchProfilesAsync(keyword, publicOnly);
        return Ok(ApiResponse<IEnumerable<PersonalProfileResponseDto>>.SuccessResult(profiles, "搜尋完成"));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<object>>> GetProfileStats()
    {
        var stats = await _profileService.GetProfileStatsAsync();
        return Ok(ApiResponse<object>.SuccessResult(stats, "成功取得個人資料統計"));
    }
}