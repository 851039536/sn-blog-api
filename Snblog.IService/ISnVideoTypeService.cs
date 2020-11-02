﻿using Snblog.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Snblog.IService
{
    public interface ISnVideoTypeService
    {
        /// <summary>
        /// 异步查询
        /// </summary>
        /// <returns></returns>
        Task<List<SnVideoType>> AsyGetTest();

        /// <summary>
        /// 主键查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<List<SnVideoType>> GetAllAsync(int id);

        /// <summary>
        /// 查询总数
        /// </summary>
        /// <returns></returns>
        Task<int> CountAsync();

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="Entity"></param>
        /// <returns></returns>
        Task<bool> AddAsync(SnVideoType Entity);
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="Entity"></param>
        /// <returns></returns>
        Task<bool> DeleteAsync(SnVideoType Entity);

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="Entity"></param>
        /// <returns></returns>
        Task<bool> UpdateAsync(SnVideoType Entity);
    }
}
