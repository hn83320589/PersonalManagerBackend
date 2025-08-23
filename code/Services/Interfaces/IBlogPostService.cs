using PersonalManagerAPI.DTOs.BlogPost;

namespace PersonalManagerAPI.Services.Interfaces;

/// <summary>
/// 部落格文章服務介面
/// </summary>
public interface IBlogPostService
{
    /// <summary>
    /// 取得所有文章（分頁）
    /// </summary>
    Task<IEnumerable<BlogPostResponseDto>> GetAllBlogPostsAsync(int skip = 0, int take = 50);

    /// <summary>
    /// 根據ID取得文章
    /// </summary>
    Task<BlogPostResponseDto?> GetBlogPostByIdAsync(int id);

    /// <summary>
    /// 根據Slug取得文章
    /// </summary>
    Task<BlogPostResponseDto?> GetBlogPostBySlugAsync(string slug);

    /// <summary>
    /// 取得指定使用者的文章
    /// </summary>
    Task<IEnumerable<BlogPostResponseDto>> GetBlogPostsByUserIdAsync(int userId);

    /// <summary>
    /// 取得已發布的公開文章
    /// </summary>
    Task<IEnumerable<BlogPostResponseDto>> GetPublishedBlogPostsAsync(int skip = 0, int take = 50);

    /// <summary>
    /// 取得草稿文章
    /// </summary>
    Task<IEnumerable<BlogPostResponseDto>> GetDraftBlogPostsAsync(int userId);

    /// <summary>
    /// 根據分類篩選文章
    /// </summary>
    Task<IEnumerable<BlogPostResponseDto>> GetBlogPostsByCategoryAsync(string category, bool publicOnly = true);

    /// <summary>
    /// 根據標籤搜尋文章
    /// </summary>
    Task<IEnumerable<BlogPostResponseDto>> SearchBlogPostsByTagsAsync(string tags, bool publicOnly = true);

    /// <summary>
    /// 搜尋文章
    /// </summary>
    Task<IEnumerable<BlogPostResponseDto>> SearchBlogPostsAsync(string keyword, bool publicOnly = true);

    /// <summary>
    /// 取得熱門文章
    /// </summary>
    Task<IEnumerable<BlogPostResponseDto>> GetPopularBlogPostsAsync(int count = 10, bool publicOnly = true);

    /// <summary>
    /// 取得最新文章
    /// </summary>
    Task<IEnumerable<BlogPostResponseDto>> GetLatestBlogPostsAsync(int count = 10, bool publicOnly = true);

    /// <summary>
    /// 取得相關文章
    /// </summary>
    Task<IEnumerable<BlogPostResponseDto>> GetRelatedBlogPostsAsync(int blogPostId, int count = 5);

    /// <summary>
    /// 根據日期範圍搜尋文章
    /// </summary>
    Task<IEnumerable<BlogPostResponseDto>> GetBlogPostsByDateRangeAsync(DateTime startDate, DateTime endDate, bool publicOnly = true);

    /// <summary>
    /// 建立新文章
    /// </summary>
    Task<BlogPostResponseDto> CreateBlogPostAsync(CreateBlogPostDto createBlogPostDto);

    /// <summary>
    /// 更新文章
    /// </summary>
    Task<BlogPostResponseDto?> UpdateBlogPostAsync(int id, UpdateBlogPostDto updateBlogPostDto);

    /// <summary>
    /// 發布文章
    /// </summary>
    Task<bool> PublishBlogPostAsync(int id);

    /// <summary>
    /// 取消發布文章
    /// </summary>
    Task<bool> UnpublishBlogPostAsync(int id);

    /// <summary>
    /// 增加文章瀏覽次數
    /// </summary>
    Task<bool> IncrementViewCountAsync(int id);

    /// <summary>
    /// 刪除文章
    /// </summary>
    Task<bool> DeleteBlogPostAsync(int id);

    /// <summary>
    /// 批量更新文章狀態
    /// </summary>
    Task<int> BatchUpdateStatusAsync(List<int> blogPostIds, bool isPublished);

    /// <summary>
    /// 取得文章統計資訊
    /// </summary>
    Task<object> GetBlogPostStatsAsync(int userId);

    /// <summary>
    /// 取得分類列表
    /// </summary>
    Task<IEnumerable<string>> GetCategoriesAsync(bool publicOnly = true);

    /// <summary>
    /// 取得標籤列表
    /// </summary>
    Task<IEnumerable<string>> GetTagsAsync(bool publicOnly = true);

    /// <summary>
    /// 生成Slug
    /// </summary>
    Task<string> GenerateSlugAsync(string title, int? excludeId = null);

    /// <summary>
    /// 驗證Slug唯一性
    /// </summary>
    Task<bool> IsSlugUniqueAsync(string slug, int? excludeId = null);

    /// <summary>
    /// 取得文章存檔（按年月分組）
    /// </summary>
    Task<object> GetBlogPostArchiveAsync(bool publicOnly = true);
}