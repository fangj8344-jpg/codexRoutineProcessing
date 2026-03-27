using Microsoft.EntityFrameworkCore;
using UtilityTools.Modules.MotorTest.Model;

namespace UtilityTools.Modules.MotorTest.SQLite
{
    public class MotorDbContext : DbContext
    {
        private readonly string _dbFileName;

        // 核心妙招：通过构造函数传入数据库文件名！想要几轴传几轴！
        public MotorDbContext(string dbFileName)
        {
            _dbFileName = dbFileName;

            // 保证只要实例化，就检查并创建（生命周期内只执行一次建表检查）
            Database.EnsureCreated();
        }

        public DbSet<PlotViewPointMessage> PlotViewPointMessages { get; set; }
        public DbSet<PlotViewSpeedMessage> PlotViewSpeedMessages { get; set; }
        public DbSet<MotorMessage> MotorMessages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // 动态使用传入的文件名
                optionsBuilder.UseSqlite($"Data Source={_dbFileName}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PlotViewPointMessage>().ToTable("PlotViewPointMessages");
            modelBuilder.Entity<PlotViewSpeedMessage>().ToTable("PlotViewSpeedMessages");
            modelBuilder.Entity<MotorMessage>().ToTable("MotorMessage");
        }
    }
}