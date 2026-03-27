using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UtilityTools.Modules.MotorTest.Model;

namespace UtilityTools.Modules.MotorTest.SQLite
{
    public static class SpliteOperate
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        public static readonly SemaphoreSlim MotorMessageSemaphore = new SemaphoreSlim(1, 1);

        // ==========================================
        // 批量插入操作 (极致纯粹，没有多余的检查)
        // ==========================================
        public static async Task<int> AddPlotViewPointListMessagesSimpleAsync(List<PlotViewPointMessage> messages, MotorDbContext context)
        {
            if (messages == null || messages.Count == 0) return 0;

            try
            {
                await context.PlotViewPointMessages.AddRangeAsync(messages);
                return await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "批量插入位置数据出错");
                return 0;
            }
        }

        public static async Task<int> AddPlotViewSpeedListMessagesSimpleAsync(List<PlotViewSpeedMessage> messages, MotorDbContext context)
        {
            if (messages == null || messages.Count == 0) return 0;

            try
            {
                await context.PlotViewSpeedMessages.AddRangeAsync(messages);
                return await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "批量插入速度数据出错");
                return 0;
            }
        }

        // ==========================================
        // 查询操作
        // ==========================================
        public static async Task<int> GetPlotViewPointMessageCountAsync(MotorDbContext context)
        {
            try
            {
                return await context.PlotViewPointMessages.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "获取位置数据总条数失败");
                return 0;
            }
        }

        public static async Task<List<PlotViewPointMessage>> GetPlotViewPointMessagesAsync(MotorDbContext context, int startIndex = 0, int takeCount = 10)
        {
            try
            {
                return await context.PlotViewPointMessages
                    .OrderBy(m => m.Id)
                    .Skip(startIndex)
                    .Take(takeCount)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"分页查询位置数据失败");
                return new List<PlotViewPointMessage>();
            }
        }
        // ==========================================
        // 速度查询操作 (新增)
        // ==========================================

        /// <summary>
        /// 获取速度数据总条数
        /// </summary>
        public static async Task<int> GetPlotViewSpeedMessageCountAsync(MotorDbContext context)
        {
            try
            {
                return await context.PlotViewSpeedMessages.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "获取速度数据总条数失败");
                return 0;
            }
        }

        /// <summary>
        /// 分页查询速度数据
        /// </summary>
        public static async Task<List<PlotViewSpeedMessage>> GetPlotViewSpeedMessagesAsync(MotorDbContext context, int startIndex = 0, int takeCount = 10)
        {
            try
            {
                return await context.PlotViewSpeedMessages
                    .OrderBy(m => m.Id)
                    .Skip(startIndex)
                    .Take(takeCount)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "分页查询速度数据失败");
                return new List<PlotViewSpeedMessage>();
            }
        }
        // ==========================================
        // 单条数据插入 (新增)
        // ==========================================

        /// <summary>
        /// 插入单条电机记录（对应你报错的方法名）
        /// </summary>
        public static async Task<int> AddMotorMessageAsync(PlotViewPointMessage message, MotorDbContext context)
        {
            if (message == null) return 0;

            try
            {
                // 假设你要存入的是位置信息表，如果表名不对，把下面的 PlotViewPointMessages 换成你的表名
                await context.PlotViewPointMessages.AddAsync(message);
                return await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "插入单条电机数据出错");
                return 0;
            }
        }
        // (其他单条插入、单条查询等方法，同理把 EnsureCreated 删掉，把 DbContextBase 替换为 MotorDbContext 即可，这里为了节省你阅读时间就不全贴了)
    }
}