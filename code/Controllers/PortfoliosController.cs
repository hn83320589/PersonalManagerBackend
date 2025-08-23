using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.DTOs.Portfolio;
using PersonalManagerAPI.Services.Interfaces;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfoliosController : BaseController
{
    private readonly IPortfolioService _portfolioService;

    public PortfoliosController(IPortfolioService portfolioService)
    {
        _portfolioService = portfolioService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortfolioResponseDto>>>> GetPortfolios([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var portfolios = await _portfolioService.GetAllPortfoliosAsync(skip, take);
        return Ok(ApiResponse<IEnumerable<PortfolioResponseDto>>.SuccessResult(portfolios, "成功取得作品集列表"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PortfolioResponseDto>>> GetPortfolio(int id)
    {
        var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);
        
        if (portfolio == null)
        {
            return NotFound(ApiResponse<PortfolioResponseDto>.ErrorResult("找不到指定的作品集"));
        }

        return Ok(ApiResponse<PortfolioResponseDto>.SuccessResult(portfolio, "成功取得作品集資料"));
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortfolioResponseDto>>>> GetPortfoliosByUserId(int userId)
    {
        var portfolios = await _portfolioService.GetPortfoliosByUserIdAsync(userId);
        return Ok(ApiResponse<IEnumerable<PortfolioResponseDto>>.SuccessResult(portfolios, "成功取得使用者作品集列表"));
    }

    [HttpGet("public")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortfolioResponseDto>>>> GetPublicPortfolios([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var portfolios = await _portfolioService.GetPublicPortfoliosAsync(skip, take);
        return Ok(ApiResponse<IEnumerable<PortfolioResponseDto>>.SuccessResult(portfolios, "成功取得公開作品集列表"));
    }

    [HttpGet("featured")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortfolioResponseDto>>>> GetFeaturedPortfolios([FromQuery] bool publicOnly = true)
    {
        var portfolios = await _portfolioService.GetFeaturedPortfoliosAsync(publicOnly);
        return Ok(ApiResponse<IEnumerable<PortfolioResponseDto>>.SuccessResult(portfolios, "成功取得特色作品集"));
    }

    [HttpGet("technology/{technology}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortfolioResponseDto>>>> SearchByTechnology(string technology, [FromQuery] bool publicOnly = true)
    {
        if (string.IsNullOrWhiteSpace(technology))
        {
            return BadRequest(ApiResponse<IEnumerable<PortfolioResponseDto>>.ErrorResult("技術名稱不能為空"));
        }

        var portfolios = await _portfolioService.SearchByTechnologyAsync(technology, publicOnly);
        return Ok(ApiResponse<IEnumerable<PortfolioResponseDto>>.SuccessResult(portfolios, $"成功取得 {technology} 相關作品集"));
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortfolioResponseDto>>>> SearchByType(string type, [FromQuery] bool publicOnly = true)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return BadRequest(ApiResponse<IEnumerable<PortfolioResponseDto>>.ErrorResult("作品類型不能為空"));
        }

        var portfolios = await _portfolioService.SearchByTypeAsync(type, publicOnly);
        return Ok(ApiResponse<IEnumerable<PortfolioResponseDto>>.SuccessResult(portfolios, $"成功取得 {type} 類型作品集"));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortfolioResponseDto>>>> SearchPortfolios([FromQuery] string keyword, [FromQuery] bool publicOnly = true)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest(ApiResponse<IEnumerable<PortfolioResponseDto>>.ErrorResult("搜尋關鍵字不能為空"));
        }

        var portfolios = await _portfolioService.SearchPortfoliosAsync(keyword, publicOnly);
        return Ok(ApiResponse<IEnumerable<PortfolioResponseDto>>.SuccessResult(portfolios, "搜尋完成"));
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortfolioResponseDto>>>> GetPortfoliosByDateRange(
        [FromQuery] DateTime? startDate, 
        [FromQuery] DateTime? endDate, 
        [FromQuery] bool publicOnly = true)
    {
        var portfolios = await _portfolioService.GetPortfoliosByDateRangeAsync(startDate, endDate, publicOnly);
        return Ok(ApiResponse<IEnumerable<PortfolioResponseDto>>.SuccessResult(portfolios, "成功取得指定時期的作品集"));
    }

    [HttpGet("technologies")]
    public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetTechnologies([FromQuery] bool publicOnly = true)
    {
        var technologies = await _portfolioService.GetTechnologiesAsync(publicOnly);
        return Ok(ApiResponse<IEnumerable<string>>.SuccessResult(technologies, "成功取得技術列表"));
    }

    [HttpGet("types")]
    public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetPortfolioTypes([FromQuery] bool publicOnly = true)
    {
        var types = await _portfolioService.GetPortfolioTypesAsync(publicOnly);
        return Ok(ApiResponse<IEnumerable<string>>.SuccessResult(types, "成功取得作品類型列表"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PortfolioResponseDto>>> CreatePortfolio([FromBody] CreatePortfolioDto createPortfolioDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
            return BadRequest(ApiResponse<PortfolioResponseDto>.ErrorResult("資料驗證失敗", errors));
        }

        var portfolio = await _portfolioService.CreatePortfolioAsync(createPortfolioDto);

        return CreatedAtAction(nameof(GetPortfolio), new { id = portfolio.Id }, 
            ApiResponse<PortfolioResponseDto>.SuccessResult(portfolio, "作品集建立成功"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<PortfolioResponseDto>>> UpdatePortfolio(int id, [FromBody] UpdatePortfolioDto updatePortfolioDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
            return BadRequest(ApiResponse<PortfolioResponseDto>.ErrorResult("資料驗證失敗", errors));
        }

        var portfolio = await _portfolioService.UpdatePortfolioAsync(id, updatePortfolioDto);
        
        if (portfolio == null)
        {
            return NotFound(ApiResponse<PortfolioResponseDto>.ErrorResult("找不到指定的作品集"));
        }

        return Ok(ApiResponse<PortfolioResponseDto>.SuccessResult(portfolio, "作品集更新成功"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeletePortfolio(int id)
    {
        var result = await _portfolioService.DeletePortfolioAsync(id);
        
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResult("找不到指定的作品集"));
        }

        return Ok(ApiResponse.SuccessResult("作品集刪除成功"));
    }

    [HttpPut("{id}/order")]
    public async Task<ActionResult<ApiResponse>> UpdatePortfolioOrder(int id, [FromBody] int newSortOrder)
    {
        var result = await _portfolioService.UpdatePortfolioOrderAsync(id, newSortOrder);
        
        if (!result)
        {
            return NotFound(ApiResponse.ErrorResult("找不到指定的作品集"));
        }

        return Ok(ApiResponse.SuccessResult("作品集排序更新成功"));
    }

    [HttpPut("batch-order")]
    public async Task<ActionResult<ApiResponse>> BatchUpdatePortfolioOrder([FromBody] Dictionary<int, int> portfolioOrders)
    {
        if (portfolioOrders == null || !portfolioOrders.Any())
        {
            return BadRequest(ApiResponse.ErrorResult("排序資料不能為空"));
        }

        var result = await _portfolioService.BatchUpdatePortfolioOrderAsync(portfolioOrders);
        
        if (!result)
        {
            return BadRequest(ApiResponse.ErrorResult("批量更新排序失敗"));
        }

        return Ok(ApiResponse.SuccessResult("批量更新作品集排序成功"));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<object>>> GetPortfolioStats()
    {
        var stats = await _portfolioService.GetPortfolioStatsAsync();
        return Ok(ApiResponse<object>.SuccessResult(stats, "成功取得作品集統計"));
    }
}