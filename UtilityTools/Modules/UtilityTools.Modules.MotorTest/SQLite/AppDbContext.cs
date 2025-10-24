using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Modules.MotorTest.Model;

namespace UtilityTools.Modules.MotorTest.SQLite
{


    public  class DbContextBase:DbContext
    {

        public DbContextBase()
        {
            
        }
        /// <summary>
        /// 代表数据库中的PlotViewPointMessages元素
        /// </summary>
        public DbSet<PlotViewPointMessage>  PlotViewPointMessages { get; set; }
        public DbSet<PlotViewSpeedMessage>  PlotViewSpeedMessages { get; set; }
        /// <summary>
        /// 代表数据库中的PlotViewPointMessages元素
        /// </summary>
        public DbSet<MotorMessage> MotorMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //根据轴名设置表名
            modelBuilder.Entity<PlotViewPointMessage>().ToTable($"PlotViewPointMessages");
            modelBuilder.Entity<PlotViewSpeedMessage>().ToTable($"PlotViewSpeedMessages");
            modelBuilder.Entity<MotorMessage>().ToTable($"MotorMessage");

        }




    }

    public class FiveAxisDbContextBase : DbContextBase
    {

        public FiveAxisDbContextBase()
        {
        }
        /// <summary>
        /// 配置数据库连接
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //使用SQLite，并指定数据库文件路径
                optionsBuilder.UseSqlite("Data Source=FiveMotorTetsMessages.db");
            }

        }
    }
    public class TwoAxisDbContextBase : DbContextBase
    {

        public TwoAxisDbContextBase()
        {

        }
        /// <summary>
        /// 配置数据库连接
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //使用SQLite，并指定数据库文件路径
                optionsBuilder.UseSqlite("Data Source=TwoMotorTetsMessages.db");
            }

        }

       
    }
    public class MotorMessageDbContextBase : DbContextBase
    {

        public MotorMessageDbContextBase()
        {
        }
        /// <summary>
        /// 配置数据库连接
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //使用SQLite，并指定数据库文件路径
                optionsBuilder.UseSqlite("Data Source=TwoMotorTetsMessages.db");
            }
        }
    }

}
