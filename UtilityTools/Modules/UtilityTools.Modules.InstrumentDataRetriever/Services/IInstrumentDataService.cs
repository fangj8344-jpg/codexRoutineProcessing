using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Modules.InstrumentDataRetriever.Entity;

namespace UtilityTools.Modules.InstrumentDataRetriever.Services
{
    public interface IInstrumentDataService
    {
        Task<bool> ConnectToDatabaseAsync(string dbPath);
        Task DisconnectAsync();
        Task<List<InstrumentInfo>> GetByConditionsAsync(DateTime? start, DateTime? end, bool? isGun, bool? isSEHV);
        Task<List<InstrumentInfo>> GetAllAsync();
        bool IsConnected { get; }
    }
}
