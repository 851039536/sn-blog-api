﻿using System.Threading.Tasks;
using Blog.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Snblog.Enties.Models;
using Snblog.IService.IService;

//默认的约定集将应用于程序集中的所有操作：
[assembly: ApiConventionType(typeof(DefaultApiConventions))]
namespace Snblog.Controllers
{
    #region 导航内容 SnNavigationController


    /// <summary>
    /// 导航内容
    /// </summary>
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "V1")] //版本控制
    [ApiController]
    public class SnNavigationController : ControllerBase
    {
        private readonly ISnNavigationService _service; //IOC依赖注入

        #region SnNavigationController
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="service"></param>
        public SnNavigationController(ISnNavigationService service)
        {
            _service = service;
        }
        #endregion

        #region 查询总数 GetCountAsync
        /// <summary>
        /// 查询总数 
        /// </summary>
        /// <param name="identity">所有:0 || 分类:1 || 用户:2  </param>
        /// <param name="type">条件(identity为0则填0) </param>
        /// <param name="cache"></param>
        /// <returns></returns>
        [HttpGet("GetCountAsync")]
        public async Task<IActionResult> GetCountAsync(int identity = 0, int type = 0, bool cache = false)
        {
            return Ok(await _service.GetCountAsync(identity, type, cache));
        }
        #endregion

        #region 查询所有GetAllAsync
        /// <summary>
        /// 查询所有
        /// </summary>
        /// <param name="cache">是否开启缓存</param>
        /// <returns></returns>
        [HttpGet("GetAllAsync")]
        public async Task<IActionResult> GetAllAsync(bool cache =false)
        {
            return Ok(await _service.GetAllAsync(cache));
        }
        #endregion

        #region 模糊查询 Contains
        /// <summary>
        /// 模糊查询
        /// </summary>
        /// <param name="identity">无条件:0 || 分类:1 || 用户:2</param>
        /// <param name="type">查询条件:用户||分类</param>
        /// <param name="name">查询字段</param>
        /// <param name="cache">是否开启缓存</param>
        /// <returns></returns>
        [HttpGet("GetContainsAsync")]
        public async Task<IActionResult> GetContainsAsync(int identity = 0, int type = 0, string name = "c", bool cache = false)
        {
            return Ok(await _service.GetContainsAsync(identity,type, name, cache));
        }
        #endregion

        #region 主键查询GetByIdAsync
        /// <summary>
        /// 主键查询
        /// </summary>
        /// <param name="id">主键</param>
        /// <param name="cache">是否开启缓存</param>
        /// <returns></returns>
        [HttpGet("GetByIdAsync")]
        public async Task<IActionResult> GetByIdAsync(int id,bool cache =false)
        {
            return Ok(await _service.GetByIdAsync(id,cache));
        }
        #endregion

        #region 条件查询GetTypeOrderAsync
        /// <summary>
        /// 条件查询 
        /// </summary>
        /// <param name="type">条件</param>
        /// <param name="order">排序</param>
        /// <param name="cache">是否开启缓存</param>
        /// <returns>List</returns>
        [HttpGet("GetTypeOrderAsync")]
        public async Task<IActionResult> GetTypeOrderAsync(string type, bool order,bool cache)
        {
            return Ok(await _service.GetTypeOrderAsync(type, order,cache));
        }
        #endregion



        #region 分页查询 GetFyAllAsync
        /// <summary>
        /// 分页查询 
        /// </summary>
        /// <param name="type">查询条件:all -表示查询所有</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页记录条数</param>
        /// <param name="isDesc">是否倒序</param>
        /// <param name="cache">是否开启缓存</param>
        [HttpGet("GetFyAllAsync")]
        public async Task<IActionResult> GetFyAllAsync(string type, int pageIndex, int pageSize, bool isDesc,bool cache)
        {
            return Ok(await _service.GetFyAllAsync(type, pageIndex, pageSize, isDesc,cache));
        }
        #endregion

        #region 添加数据AddAsync
        /// <summary>
        /// 添加数据 
        /// </summary>
        /// <returns></returns>
        [HttpPost("AddAsync")]
        [Authorize(Roles = Permissions.Name)]
        public async Task<ActionResult<SnNavigation>> AddAsync(SnNavigation entity)
        {
            return Ok(await _service.AddAsync(entity));
        }
        #endregion

        #region 更新数据 UpdateAsync
        /// <summary>
        /// 更新数据 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPut("UpdateAsync")]
        [Authorize(Roles = Permissions.Name)]
        public async Task<IActionResult> UpdateAsync(SnNavigation entity)
        {
            return Ok(await _service.UpdateAsync(entity));
        }
        #endregion

        #region 删除数据DeleteAsync
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="id">主键</param>
        /// <returns></returns>
        [HttpDelete("DeleteAsync")]
        [Authorize(Roles = Permissions.Name)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            return Ok(await _service.DeleteAsync(id));
        }
        #endregion
    }
    #endregion
}
