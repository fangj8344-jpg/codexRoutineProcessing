using System;
using System.Collections.Generic;
using System.Text;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Services.Interfaces
{
    public interface IServiceFactory
    {
        public ICameraService GetCameraDevice(string type);

        public IUsbService GetUsbDevice(string type);

        public IAsynRWService GetAsynRWDevice(string type);

        public ISyncRWService GetSyncRWDevice(string type);
    }
}
