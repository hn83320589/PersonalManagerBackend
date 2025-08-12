using Microsoft.AspNetCore.Mvc;
using PersonalManagerAPI.DTOs;
using PersonalManagerAPI.Models;
using PersonalManagerAPI.Services;

namespace PersonalManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfoliosController : BaseController
{
    private readonly JsonDataService _dataService;

    public PortfoliosController(JsonDataService dataService)
    {
        _dataService = dataService;
    }

    /// <summary>
    /// 取得所有公開作品集
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPortfolios()
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            var publicPortfolios = portfolios
                .Where(p => p.IsPublic)
                .OrderByDescending(p => p.StartDate)
                .ToList();
                
            return Ok(ApiResponse<List<Portfolio>>.SuccessResult(publicPortfolios, "成功取得作品集列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<Portfolio>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得指定作品集
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPortfolio(int id)
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            var portfolio = portfolios.FirstOrDefault(p => p.Id == id);
            
            if (portfolio == null)
            {
                return NotFound(ApiResponse<Portfolio>.ErrorResult("找不到指定的作品集"));
            }

            return Ok(ApiResponse<Portfolio>.SuccessResult(portfolio, "成功取得作品集資料"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Portfolio>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 取得指定使用者的作品集
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetPortfoliosByUser(int userId)
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            var userPortfolios = portfolios
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.StartDate)
                .ToList();
                
            return Ok(ApiResponse<List<Portfolio>>.SuccessResult(userPortfolios, "成功取得使用者作品集列表"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<Portfolio>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 依技術類型篩選作品集
    /// </summary>
    [HttpGet("technology/{tech}")]
    public async Task<IActionResult> GetPortfoliosByTechnology(string tech)
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            var filteredPortfolios = portfolios
                .Where(p => p.IsPublic && 
                       (!string.IsNullOrEmpty(p.Technologies) && 
                        p.Technologies.Contains(tech, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(p => p.StartDate)
                .ToList();
                
            return Ok(ApiResponse<List<Portfolio>>.SuccessResult(filteredPortfolios, $"成功取得 {tech} 相關作品集"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<Portfolio>>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 建立作品集
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePortfolio([FromBody] Portfolio portfolio)
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            
            portfolio.Id = portfolios.Count > 0 ? portfolios.Max(p => p.Id) + 1 : 1;
            portfolio.CreatedAt = DateTime.UtcNow;
            portfolio.UpdatedAt = DateTime.UtcNow;
            
            portfolios.Add(portfolio);
            await _dataService.SavePortfoliosAsync(portfolios);

            return Ok(ApiResponse<Portfolio>.SuccessResult(portfolio, "作品集建立成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Portfolio>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 更新作品集
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePortfolio(int id, [FromBody] Portfolio updatedPortfolio)
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            var portfolio = portfolios.FirstOrDefault(p => p.Id == id);
            
            if (portfolio == null)
            {
                return NotFound(ApiResponse<Portfolio>.ErrorResult("找不到指定的作品集"));
            }

            portfolio.Title = updatedPortfolio.Title;
            portfolio.Description = updatedPortfolio.Description;
            portfolio.ProjectUrl = updatedPortfolio.ProjectUrl;
            portfolio.RepositoryUrl = updatedPortfolio.RepositoryUrl;
            portfolio.ImageUrl = updatedPortfolio.ImageUrl;
            portfolio.Technologies = updatedPortfolio.Technologies;
            portfolio.StartDate = updatedPortfolio.StartDate;
            portfolio.EndDate = updatedPortfolio.EndDate;
            portfolio.IsPublic = updatedPortfolio.IsPublic;
            portfolio.SortOrder = updatedPortfolio.SortOrder;
            portfolio.UpdatedAt = DateTime.UtcNow;

            await _dataService.SavePortfoliosAsync(portfolios);

            return Ok(ApiResponse<Portfolio>.SuccessResult(portfolio, "作品集更新成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Portfolio>.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }

    /// <summary>
    /// 刪除作品集
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePortfolio(int id)
    {
        try
        {
            var portfolios = await _dataService.GetPortfoliosAsync();
            var portfolio = portfolios.FirstOrDefault(p => p.Id == id);
            
            if (portfolio == null)
            {
                return NotFound(ApiResponse.ErrorResult("找不到指定的作品集"));
            }

            portfolios.Remove(portfolio);
            await _dataService.SavePortfoliosAsync(portfolios);

            return Ok(ApiResponse.SuccessResult("作品集刪除成功"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.ErrorResult("伺服器錯誤: " + ex.Message));
        }
    }
}