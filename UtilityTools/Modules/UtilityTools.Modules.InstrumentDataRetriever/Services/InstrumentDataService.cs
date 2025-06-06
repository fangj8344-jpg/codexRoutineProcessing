using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UtilityTools.Modules.InstrumentDataRetriever.Entity;

namespace UtilityTools.Modules.InstrumentDataRetriever.Services
{
    public class InstrumentDataService : IInstrumentDataService
    {
        private InstrumentDbContext _context;
        private bool _isConnected;
        private string _dbPath;

        public bool IsConnected => _isConnected;

        public async Task<bool> ConnectToDatabaseAsync(string dbPath)
        {
            try
            {
                _dbPath = dbPath;
                _context = new InstrumentDbContext(dbPath);
                await _context.Database.EnsureCreatedAsync();
                _isConnected = true;
                return true;
            }
            catch
            {
                _isConnected = false;
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            if (_context != null)
            {
                await _context.DisposeAsync();
                _context = null;
            }
            _isConnected = false;
        }

        public async Task<List<InstrumentInfo>> GetByConditionsAsync(DateTime? start, DateTime? end, bool? isGun, bool? isSEHV)
        {
            if (!_isConnected) return new List<InstrumentInfo>();
            var query = _context.InstrumentInfos.AsQueryable();
            if (start.HasValue)
                query = query.Where(x => x.Time >= start.Value);
            if (end.HasValue)
                query = query.Where(x => x.Time <= end.Value);
            if (isGun.HasValue)
                query = query.Where(x => x.IsOpenGun == isGun.Value);
            if (isSEHV.HasValue)
                query = query.Where(x => x.SeHvEnable == isSEHV.Value);
            return await query.OrderBy(x => x.Time).ToListAsync();
        }

        public async Task<List<InstrumentInfo>> GetAllAsync()
        {
            if (!_isConnected) return new List<InstrumentInfo>();
            return await _context.InstrumentInfos.ToListAsync();
        }
    }

    public class InstrumentDbContext : DbContext
    {
        private readonly string _databasePath;

        public InstrumentDbContext(string databasePath)
        {
            _databasePath = databasePath;
        }

        public DbSet<InstrumentInfo> InstrumentInfos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_databasePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InstrumentInfo>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<InstrumentInfo>()
                .Property(x => x.Time)
                .IsRequired();

            base.OnModelCreating(modelBuilder);
        }
    }
} 