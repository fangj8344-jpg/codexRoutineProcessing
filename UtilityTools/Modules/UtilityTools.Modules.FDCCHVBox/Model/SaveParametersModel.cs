using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UtilityTools.Modules.FDC12CHVBox.Model
{
    public class SaveParametersModel
    {
        
        public SaveParametersModel()
        {
            saveHvParameters = new ObservableCollection<SaveHvParameters>();
        }
      
        private string _filePath = Path.Combine("data", "FDC12CHVBoxConfig.json");
        public UInt32 TimerInterval { get; set; }
        public bool IsSetHvShow { get; set; }
        public ObservableCollection<SaveHvParameters>   saveHvParameters { get; set; }

        /// <summary>
        /// 保存参数
        /// </summary>
        public void SaveParameter(FDC12CHVBoxModel fDC12CHVBoxModel)
        {
            saveHvParameters = new ObservableCollection<SaveHvParameters>();
            TimerInterval = fDC12CHVBoxModel.TimerInterval;
            IsSetHvShow = fDC12CHVBoxModel.HVBoxModels[0].IsSetHvShow;
            foreach (HVBoxModel model in fDC12CHVBoxModel.HVBoxModels)
            {
                saveHvParameters.Add(new SaveHvParameters(model));
            }
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            var path =  Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
           
            File.WriteAllText(_filePath, json);
        }
        /// <summary>
        /// 加载参数
        /// </summary>
        public void LoadParameter(FDC12CHVBoxModel fDC12CHVBoxModel)
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    SaveParametersModel obj = JsonConvert.DeserializeObject<SaveParametersModel>(json);
                    fDC12CHVBoxModel.TimerInterval = obj.TimerInterval;
                    for (int i = 0; i < obj.saveHvParameters.Count; i++)
                    {
                        for (int j = 0; j < fDC12CHVBoxModel.HVBoxModels.Count; j++)
                        {
                            if (obj.saveHvParameters[i].Channel == fDC12CHVBoxModel.HVBoxModels[j].Channel)
                            {
                                fDC12CHVBoxModel.HVBoxModels[j].WriteHV = obj.saveHvParameters[i].WriteHV;
                                fDC12CHVBoxModel.HVBoxModels[j].IsCheck = obj.saveHvParameters[i].IsCheck;
                                fDC12CHVBoxModel.HVBoxModels[j].WriteStep = obj.saveHvParameters[i].WriteStep;
                                fDC12CHVBoxModel.HVBoxModels[j].SetStep = obj.saveHvParameters[i].SetStep;
                                fDC12CHVBoxModel.HVBoxModels[j].TimerInterval = obj.saveHvParameters[i].TimerInterval;
                                fDC12CHVBoxModel.HVBoxModels[j].IsSetHvShow = obj.IsSetHvShow; 
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($" 加载参数失败{ex}");
            }
           
        }
    }

    public class SaveHvParameters 
    {
        public SaveHvParameters(HVBoxModel hVBoxModel)
        {
            Channel = hVBoxModel.Channel;
            WriteHV = hVBoxModel.WriteHV;
            IsCheck = hVBoxModel.IsCheck;
            WriteStep = hVBoxModel.WriteStep;
            SetStep = hVBoxModel.SetStep;
            TimerInterval = hVBoxModel.TimerInterval;
        }
        public SaveHvParameters()
        {
            
        }
        public byte Channel { get; set; }
        public int WriteHV { get; set; }
        public bool IsCheck { get; set; }
        public ushort WriteStep { get; set; }
        public ushort SetStep { get; set; }
        public UInt32 TimerInterval { get; set; }
    }


}
