﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Snblog.Cache.CacheUtil;
using Snblog.Enties.Models;
using Snblog.Enties.ModelsDto;
using Snblog.IService.IService;
using Snblog.Repository.Repository;
using Snblog.Util.components;

namespace Snblog.Service.Service
{
    public class SnLabelsService : ISnLabelsService
    {
        private readonly snblogContext _service;//DB
        private readonly CacheUtil _cacheUtil;
        Tool<SnLabel> data = new Tool<SnLabel>();
         Tool<SnLabelDto> datas = new Tool<SnLabelDto>();
        private readonly ILogger<SnLabelsService> _logger;
        private readonly IMapper _mapper;
        public SnLabelsService(ICacheUtil cacheUtil, snblogContext coreDbContext, ILogger<SnLabelsService> logger, IMapper mapper = null)
        {
            _service = coreDbContext;
            _cacheUtil = (CacheUtil)cacheUtil;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("删除数据_SnLabels" + id);
            var todoItem = await _service.SnLabels.FindAsync(id);
            if (todoItem == null) return false;
            _service.SnLabels.Remove(todoItem);
            return await _service.SaveChangesAsync() > 0;
        }

        public async Task<SnLabelDto> GetByIdAsync(int id, bool cache)
        {
            _logger.LogInformation("SnLabels_主键查询=>" + id + cache);
            datas.resultDto = _cacheUtil.CacheString("SnLabels_GetByIdAsync" + id + cache, datas.resultDto, cache);
            if (datas.resultDto == null)
            {
                datas.resultDto= _mapper.Map<SnLabelDto>(await _service.SnLabels.FindAsync(id));
                _cacheUtil.CacheString("SnLabels_GetByIdAsync" + id + cache, datas.resultDto, cache);
            }
            return datas.resultDto;
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> AddAsync(SnLabel entity)
        {
            _logger.LogInformation("添加数据_SnLabels" + entity);
            await _service.SnLabels.AddAsync(entity);
            return await _service.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(SnLabel entity)
        {
            _logger.LogInformation("更新数据_SnLabels" + entity);
            _service.SnLabels.Update(entity);
            return await _service.SaveChangesAsync() > 0;

        }

        /// <summary>
        /// 查询所有
        /// </summary>
        /// <returns></returns>
        public async Task<List<SnLabelDto>> GetAllAsync(bool cache)
        {
            _logger.LogInformation("查询所有_SnLabels" + cache);
            datas.resultListDto = _cacheUtil.CacheString("GetAllAsync_SnLabels" + cache, datas.resultListDto, cache);
            if (datas.resultListDto == null)
            {
                datas.resultListDto = _mapper.Map<List<SnLabelDto>>(await _service.SnLabels.ToListAsync());
                _cacheUtil.CacheString("GetAllAsync_SnLabels" + cache, datas.resultListDto, cache);
            }
            return datas.resultListDto;
        }

        public async Task<int> GetCountAsync(bool cache)
        {
            _logger.LogInformation("SnLabels_查询总数" + cache);
            data.resulInt = _cacheUtil.CacheNumber("GetCountAsync__SnLabels" + cache, data.resulInt, cache);
            if (data.resulInt == 0)
            {
                data.resulInt = await _service.SnLabels.CountAsync();
                _cacheUtil.CacheNumber("GetCountAsync__SnLabels" + cache, data.resulInt, cache);
            }
            return data.resulInt;
        }

        public async Task<List<SnLabelDto>> GetFyAsync(int pageIndex, int pageSize, bool isDesc, bool cache)
        {
            _logger.LogInformation("SnLabels_分页查询=>" + pageIndex + pageSize + isDesc + cache);

            datas.resultListDto = _cacheUtil.CacheString("GetfyAllAsync_SnLabels" + pageIndex + pageSize + isDesc + cache, datas.resultListDto, cache);
            if (datas.resultListDto == null)
            {
                await GetfyAll(pageIndex, pageSize, isDesc);
                _cacheUtil.CacheString("GetfyAllAsync_SnLabels" + pageIndex + pageSize + isDesc + cache, datas.resultListDto, cache);
            }
            return datas.resultListDto;

        }
        private async Task GetfyAll(int pageIndex, int pageSize, bool isDesc)
        {
            if (isDesc)
            {
                data.resultList = await _service.SnLabels.OrderByDescending(c => c.Id).Skip((pageIndex - 1) * pageSize)
                       .Take(pageSize).ToListAsync();
                datas.resultListDto= _mapper.Map<List<SnLabelDto>>(data.resultList);
            }
            else
            {
                data.resultList = await _service.SnLabels.OrderBy(c => c.Id).Skip((pageIndex - 1) * pageSize)
                         .Take(pageSize).ToListAsync();
                datas.resultListDto = _mapper.Map<List<SnLabelDto>>(data.resultList);
            }
        }
    }
}
