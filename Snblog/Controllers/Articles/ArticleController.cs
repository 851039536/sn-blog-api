using Microsoft.AspNetCore.RateLimiting;
using Snblog.IService.IService.Articles;
using Snblog.Jwt;

namespace Snblog.Controllers.Articles;

/// <summary>
/// 文章API
/// </summary>
[ApiExplorerSettings(GroupName = "V1")] //版本控制
[ApiController] //控制路由
[Route("article")]
public class ArticleController : BaseController
{
    #region 服务

    private readonly IArticleService _service;
    private readonly IValidator<Article> _validator;
  
    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化实例
    /// </summary>
    /// <param name="service">接口实例</param>
    public ArticleController(IServiceProvider service)
    {
        _service = service.GetRequiredService<IArticleService>();
        _validator = service.GetRequiredService<IValidator<Article>>();
    }

    #endregion
    
   
    
    #region 查询总数

    /// <summary>
    /// 查询总数
    /// </summary>
    /// <param name="identity">所有:0|分类:1|标签:2|用户3</param>
    /// <param name="type">条件</param>
    /// <param name="cache">缓存</param>
    /// <returns>int</returns>
    [EnableRateLimiting("fixed")]
    [HttpGet("sum")]
    public async Task<IActionResult> GetSumAsync(int identity = 0, string type = null, bool cache = false)
    {
        int data = await _service.GetSumAsync(identity, type, cache);
        if (data == -1)
        {
            // 如果方法返回-1，表示失败，返回一个失败的API响应
            return ApiResponseFailure<string>();
        }
        // 如果方法成功，返回一个成功的API响应
        return ApiResponseSuccess(cache: cache, data: data);
    }
    #endregion

    #region 模糊查询

    /// <summary>
    /// 执行模糊查询操作，根据不同的标识和类型筛选文章。
    /// </summary>
    /// <param name="identity">查询的标识类型，具体值含义：所有:0|分类:1|标签:2|用户:3|标签,用户:4</param>
    /// <param name="type">查询的类型参数，用于进一步筛选文章。多条件以','分割。</param>
    /// <param name="name">查询的关键字段，用于模糊匹配文章名称。</param>
    /// <param name="cache">是否使用缓存。如果为true，则优先从缓存中获取结果。</param>
    /// <returns>包含匹配文章的列表。</returns>
    [HttpGet("contains")]
    public async Task<IActionResult> GetContainsAsync(
        int identity = 0,
        string type = "null",
        string name = "winfrom",
        bool cache = false
    )
    {
        var data = await _service.GetContainsAsync(identity, type, name, cache);
        return ApiResponse(cache: cache, data: data);
    }

    #endregion

    #region 主键查询

    /// <summary>
    /// 主键查询
    /// </summary>
    /// <param name="id">主键</param>
    /// <param name="cache">是否使用缓存</param>
    /// <returns>ArticleDto对象</returns>
    [HttpGet("bid")]
    public async Task<IActionResult> GetByIdAsync(int id, bool cache = false)
    {
        var data = await _service.GetByIdAsync(id, cache);
        return ApiResponse(cache: cache, data: data);
    }

    #endregion

    #region 类别查询

    /// <summary>
    ///类别查询
    /// </summary>
    /// <param name="identity">分类:1|标签:2</param>
    /// <param name="type">类别</param>
    /// <param name="cache">缓存</param>
    [HttpGet("type")]
    public async Task<IActionResult> GetTypeAsync(int identity = 1, string type = "null", bool cache = false)
    {
        var data = await _service.GetTypeAsync(identity, type, cache);
        return ApiResponse(cache: cache, data: data);
    }

    #endregion

    #region 内容统计

    /// <summary>
    /// 内容统计
    /// </summary>
    /// <param name="identity">统计的类型：所有:0|分类:1|标签:2|用户:3</param>
    /// <param name="type">统计的内容类型：内容:1|阅读:2|点赞:3</param>
    /// <param name="name">查询参数，如分类名、标签名或用户名</param>
    /// <param name="cache">是否使用缓存</param>
    /// <returns>统计结果，整数类型</returns>
    [HttpGet("strSum")]
    public async Task<IActionResult> GetStrSumAsync(int identity = 0, int type = 1, string name = "null", bool cache = false)
    {
        int data = await _service.GetStrSumAsync(identity, type, name, cache);
        return ApiResponse(cache: cache, data: data);
    }
    #endregion

    #region 分页查询

    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="identity">查询的类型：所有:0|分类:1|标签:2|用户:3|标签+用户:4</param>
    /// <param name="type">查询参数，多条件以','分割</param>
    /// <param name="pageIndex">当前页码</param>
    /// <param name="pageSize">每页记录条数</param>
    /// <param name="ordering">排序规则：data:时间|read:阅读|give:点赞|id:主键</param>
    /// <param name="isDesc">排序方式：true为降序，false为升序</param>
    /// <param name="cache">是否使用缓存</param>
    /// <returns>分页查询结果，List ArticleDto类型</returns>
    [HttpGet("paging")]
    public async Task<IActionResult> GetPagingAsync(
        int identity = 0,
        string type = "null",
        int pageIndex = 1,
        int pageSize = 10,
        string ordering = "id",
        bool isDesc = true,
        bool cache = false
    )
    {
        var ret = await _service.GetPagingAsync(identity, type, pageIndex, pageSize, ordering, isDesc, cache);
        return ApiResponse(cache: cache, data: ret);
    }

    #endregion

    #region 新增

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="entity">实体</param>
    /// <returns>bool</returns>
    [Authorize(Policy = JPermissions.Create)]
    [HttpPost("add")]
    public async Task<IActionResult> AddAsync(Article entity)
    {
        var result = await _validator.ValidateAsync(entity);
        if (!result.IsValid)
        {
            return ApiResponse(statusCode: 404, message: result.Errors[0].ErrorMessage, data: entity);
        }
        bool data = await _service.AddAsync(entity);
        return ApiResponse(data: data);
    }

    #endregion

    #region 更新

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="entity">实体</param>
    /// <returns>bool</returns>
    [Authorize(Policy = JPermissions.Edit)]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateAsync(Article entity)
    {
        bool data = await _service.UpdateAsync(entity);
        return ApiResponse(data: data);
    }

    #region 条件更新

    /// <summary>
    /// 条件更新
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="type">更新字段: Read | Give | Comment</param>
    /// <returns>bool</returns>
    [HttpPut("upPortion")]
    public async Task<IActionResult> UpdatePortionAsync(Article entity, string type)
    {
        bool data = await _service.UpdatePortionAsync(entity, type);
        return ApiResponse(data: data);
    }

    #endregion

    #endregion

    #region 删除

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="id">主键</param>
    /// <returns>bool</returns>
    [Authorize(Policy = JPermissions.Delete)]
    [HttpDelete("del")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        bool data = await _service.DelAsync(id);
        return ApiResponse(data: data);
    }

    #endregion
}
