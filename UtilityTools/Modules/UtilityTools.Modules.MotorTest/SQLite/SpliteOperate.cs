using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UtilityTools.Modules.MotorTest.Model;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace UtilityTools.Modules.MotorTest.SQLite
{
    public class SpliteOperate : IDisposable
    {
        // 日志实例（静态单例，避免重复创建）
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        // 数据库上下文（注入获取，遵循EF Core Scoped生命周期）
        private readonly DbContextBase _context;
        // 标记是否已初始化数据库（避免重复调用EnsureCreatedAsync）
        private bool _isDbInitialized;

        /// <summary>
        /// 构造函数（注入数据库上下文并初始化数据库）
        /// </summary>
        /// <param name="appDbContext">EF Core 数据库上下文</param>
        public SpliteOperate(DbContextBase appDbContext)
        {
            _context = appDbContext ?? throw new ArgumentNullException(
                nameof(appDbContext), "数据库上下文不能为空");
          
            // 初始化数据库（异步转同步，确保构造时完成初始化，避免后续操作异常）
            InitializeDatabaseAsync().GetAwaiter().GetResult();
        }

        #region 数据添加操作（统一异常处理+空值校验）
        /// <summary>
        /// 新增 PlotView 位置数据到数据库
        /// </summary>
        /// <param name="pointMessage">位置数据实体（不可为null）</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> AddPlotViewPointMessageAsync(PlotViewPointMessage pointMessage)
        {
            // 1. 空值校验（避免无效数据入库）
            if (pointMessage == null)
            {
                _logger.Warn("添加位置数据失败：PlotViewPointMessage 为 null");
                return false;
            }

            // 2. 数据库操作（统一异常处理）
            return await ExecuteDbOperationAsync(
                operationName: "添加位置数据",
                operation: async () =>
                {
                    await _context.PlotViewPointMessages.AddAsync(pointMessage);
                    await _context.SaveChangesAsync();
                    return true;
                });
        }
        /// <summary>
        /// 将缓存区的数据添加导数据库
        /// </summary>
        /// <param name="pointMessageList"></param>
        /// <returns></returns>
       
        public async Task<int> AddPlotViewPointListMessagesSimpleAsync(List<PlotViewPointMessage> messages)
        {
            if (messages == null || messages.Count == 0)
                return 0;

            try
            {
                // 一次性把多条数据加入 DbSet
                await _context.PlotViewPointMessages.AddRangeAsync(messages);

                // 一次性提交到数据库
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "批量插入多条数据出错");
                return 0;
            }
        }
        public async Task<int> AddPlotViewSpeedListMessagesSimpleAsync(List<PlotViewSpeedMessage> messages)
        {
            if (messages == null || messages.Count == 0)
                return 0;

            try
            {
                // 一次性把多条数据加入 DbSet
                await _context.PlotViewSpeedMessages.AddRangeAsync(messages);

                // 一次性提交到数据库
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "批量插入多条数据出错");
                return 0;
            }
        }

        /// <summary>
        /// 新增 PlotView 速度数据到数据库
        /// </summary>
        /// <param name="speedMessage">速度数据实体（不可为null）</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> AddPlotViewSpeedMessageAsync(PlotViewSpeedMessage speedMessage)
        {
            // 1. 空值校验
            if (speedMessage == null)
            {
                _logger.Warn("添加速度数据失败：PlotViewSpeedMessage 为 null");
                return false;
            }

            // 2. 数据库操作（复用异常处理逻辑）
            return await ExecuteDbOperationAsync(
                operationName: "添加速度数据",
                operation: async () =>
                {
                    await _context.PlotViewSpeedMessages.AddAsync(speedMessage);
                    await _context.SaveChangesAsync();
                    return true;
                });
        }


        #endregion
        #region 数据查询操作（修复分页逻辑+排序保障）
        /// <summary>
        /// 根据ID查询单个位置数据
        /// </summary>
        /// <param name="messageId">数据ID</param>
        /// <returns>匹配的位置数据（无匹配时返回null）</returns>
        public async Task<PlotViewPointMessage?> GetPointMessageByIdAsync(int messageId)
        {
            try
            {
                return await _context.PlotViewPointMessages.FindAsync(messageId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"根据ID[{messageId}]查询位置数据失败");
                return null;
            }
        }

        /// <summary>
        /// 分页查询位置数据（按ID升序排序，确保数据顺序一致）
        /// </summary>
        /// <param name="startIndex">起始索引（从0开始，跳过前N条数据）</param>
        /// <param name="takeCount">获取条数（一次最多获取的数据量）</param>
        /// <returns>分页后的位置数据列表</returns>
        /// <exception cref="ArgumentOutOfRangeException">参数非法时抛出</exception>
        public async Task<List<PlotViewPointMessage>> GetPlotViewPointMessagesAsync(int startIndex = 0, int takeCount = 10)
        {
            // 参数合法性校验（避免无效查询）
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "起始索引不能为负数");
            try
            {
                // 修复原分页逻辑：按ID升序，跳过startIndex条，取takeCount条
                return await _context.PlotViewPointMessages
                    .OrderBy(m => m.Id) // 强制排序，确保分页顺序一致
                    .Skip(startIndex)
                    .Take(takeCount)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"分页查询位置数据失败（起始索引：{startIndex}，获取条数：{takeCount}）");
                return new List<PlotViewPointMessage>(); // 返回空列表，避免上层空引用
            }
        }

        /// <summary>
        /// 分页查询速度数据（按ID升序排序，确保数据顺序一致）
        /// </summary>
        /// <param name="startIndex">起始索引（从0开始，跳过前N条数据）</param>
        /// <param name="takeCount">获取条数（一次最多获取的数据量）</param>
        /// <returns>分页后的速度数据列表</returns>
        /// <exception cref="ArgumentOutOfRangeException">参数非法时抛出</exception>
        public async Task<List<PlotViewSpeedMessage>> GetPlotViewSpeedMessagesAsync(int startIndex = 0, int takeCount = 10)
        {
            // 参数合法性校验
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "起始索引不能为负数");
            try
            {
                // 修复原分页逻辑，统一排序规则
                return await _context.PlotViewSpeedMessages
                    .OrderBy(m => m.Id)
                    .Skip(startIndex)
                    .Take(takeCount)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"分页查询速度数据失败（起始索引：{startIndex}，获取条数：{takeCount}）");
                return new List<PlotViewSpeedMessage>();
            }
        }

        /// <summary>
        /// 获取位置数据总条数
        /// </summary>
        /// <returns>数据总条数（异常时返回0）</returns>
        public async Task<int> GetPlotViewPointMessageCountAsync()
        {
            try
            {
                return await _context.PlotViewPointMessages.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "获取位置数据总条数失败");
                return 0;
            }
        }

        /// <summary>
        /// 获取速度数据总条数
        /// </summary>
        /// <returns>数据总条数（异常时返回0）</returns>
        public async Task<int> GetPlotViewSpeedMessageCountAsync()
        {
            try
            {
                return await _context.PlotViewSpeedMessages.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "获取速度数据总条数失败");
                return 0;
            }
        }
        #endregion
        #region 通用辅助方法（提取重复逻辑，降低冗余）
        /// <summary>
        /// 初始化数据库（仅执行一次，避免重复调用EnsureCreatedAsync）
        /// </summary>
        private async Task InitializeDatabaseAsync()
        {
            if (_isDbInitialized) return;

            try
            {
                // 确保数据库和表已创建（EF Core 会自动判断，不会重复创建）
                await _context.Database.EnsureCreatedAsync();
                _isDbInitialized = true;
                _logger.Info("SQLite 数据库初始化完成");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "SQLite 数据库初始化失败");
                throw; // 抛出异常，终止构造，避免后续操作出错
            }
        }
        /// <summary>
        /// 数据库操作通用执行方法（统一异常处理，减少重复代码）
        /// </summary>
        /// <param name="operationName">操作名称（用于日志记录）</param>
        /// <param name="operation">具体数据库操作（异步委托）</param>
        /// <returns>操作是否成功</returns>
        private async Task<bool> ExecuteDbOperationAsync(string operationName, Func<Task<bool>> operation)
        {
            try
            {
                // 检查数据库是否已初始化
                if (!_isDbInitialized)
                    await InitializeDatabaseAsync();

                return await operation();
            }
            catch (DbUpdateException ex)
            {
                // 捕获EF Core数据库更新异常（如主键冲突、字段验证失败等）
                _logger.Error(ex, $"{operationName}失败（数据库更新异常）：{GetInnerExceptionMessage(ex)}");
                return false;
            }
            catch (Exception ex)
            {
                // 捕获其他通用异常
                _logger.Error(ex, $"{operationName}失败：{GetInnerExceptionMessage(ex)}");
                return false;
            }
        }
        /// <summary>
        /// 获取异常的内部消息（递归获取最内层异常，便于排查问题）
        /// </summary>
        private string GetInnerExceptionMessage(Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;
            return ex.Message;
        }
        #endregion

        /// <summary>
        /// 数据库安装Id查找
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public async Task<PlotViewPointMessage> GetMessageByIdAsync(int messageId)
        {
            var meeage = await _context.PlotViewPointMessages.FindAsync(messageId);
            return meeage; 
            
        }
       
       

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
