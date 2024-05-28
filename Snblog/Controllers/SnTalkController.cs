﻿using Snblog.Util.GlobalVar;

namespace Snblog.Controllers;

/// <summary>
/// SnTalkController
/// </summary>
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "V1")] //版本控制
[ApiController]
public class SnTalkController : Controller
{
    private readonly ISnTalkService _service; //IOC依赖注入

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="service"></param>
    public SnTalkController(ISnTalkService service)
    {
        _service = service;
    }

    #region 查询所有

    /// <summary>
    /// 查询所有
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetAllAsync")]
    public async Task<IActionResult> GetAllAsync()
    {
        return Ok(await _service.GetAllAsync());
    }

    #endregion

    /// <summary>
    /// 主键查询
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    [HttpGet("GetAllAsyncID")]
    public async Task<IActionResult> GetAllAsync(int id)
    {
        return Ok(await _service.GetAllAsync(id));
    }


    /// <summary>
    /// 分页查询 - 支持排序
    /// </summary>
    /// <param name="pageIndex">当前页码</param>
    /// <param name="pageSize">每页记录条数</param>
    /// <param name="isDesc">是否倒序</param>
    [HttpGet("GetFyAllAsync")]
    public async Task<IActionResult> GetFyAllAsync(int pageIndex, int pageSize, bool isDesc)
    {
        return Ok(await _service.GetFyAllAsync(pageIndex, pageSize, isDesc));
    }

    /// <summary>
    /// 条件分页查询
    /// </summary>
    /// <param name="type"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="isDesc"></param>
    /// <returns></returns>
    [HttpGet("GetFyTypeAllAsync")]
    public async Task<IActionResult> GetFyTypeAllAsync(int type, int pageIndex, int pageSize, bool isDesc)
    {
        return Ok(await _service.GetFyTypeAllAsync(type, pageIndex, pageSize, isDesc));
    }

    /// <summary>
    /// 总数
    /// </summary>
    /// <returns></returns>
    [HttpGet("CountAsync")]
    public async Task<IActionResult> CountAsync()
    {
        return Ok(await _service.CountAsync());
    }

    /// <summary>
    /// 条件查询总数
    /// </summary>
    /// <returns></returns>
    [HttpGet("CountTypeAsync")]
    public async Task<IActionResult> CountAsync(int type)
    {
        int data = await _service.CountAsync(type);
        return Ok(data);
    }

    /// <summary>
    /// 添加数据 （权限）
    /// </summary>
    /// <returns></returns>
    [HttpPost("AddAsync")]
    [Authorize(Roles = Permissionss.Name)]
    public async Task<IActionResult> AddAsync(SnTalk entity)
    {
        return Ok(await _service.AddAsync(entity));
    }

    /// <summary>
    /// 删除数据 （权限）
    /// </summary>
    /// <returns></returns>
    [HttpDelete("DelAsync")]
    [Authorize(Roles = Permissionss.Name)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        return Ok(await _service.DeleteAsync(id));
    }

    /// <summary>
    /// 更新数据 （权限）
    /// </summary>
    /// <returns></returns>
    [HttpPut("UpdateAsync")]
    [Authorize(Roles = Permissionss.Name)]
    public async Task<IActionResult> UpdateAsync(SnTalk entity)
    {
        return Ok(await _service.UpdateAsync(entity));
    }
}