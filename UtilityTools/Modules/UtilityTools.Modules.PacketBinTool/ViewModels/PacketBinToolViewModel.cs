using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime;
using UtilityTools.Core.Helper;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Forms;

using static OpenCvSharp.Stitcher;
using NLog.LayoutRenderers;
using System.IO.Compression;
using UtilityTools.Core.Model;
using System.ComponentModel;

namespace UtilityTools.Modules.PacketBinTool.ViewModels
{
    public enum EnumDevelopmentBoard
    {

    }
    internal class PacketBinToolViewModel : BindableBase
    {
        #region ------------Constructor------------
        public PacketBinToolViewModel()
        {
            DevelopmentBoardMessage = new DevelopmentBoardMessage();
            PackageCommand = new DelegateCommand(Package);
            LoadFilePathCommand = new DelegateCommand(LoadFilePath);
        }
        #endregion

        #region ------------Field------------

        #endregion

        private DevelopmentBoardMessage _developmentBoardMessage;
        /// <summary>
        /// 升级信息
        /// </summary>
        public DevelopmentBoardMessage DevelopmentBoardMessage
        {
            get { return _developmentBoardMessage; }
            set { _developmentBoardMessage = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 需要打包的文件路径
        /// </summary>
        private string sourcePath;
        public string SourcePath
        {
            get { return sourcePath; }
            set { sourcePath = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 需要打包的文件路径
        /// </summary>
        private string packfilePath;
        public string PackfilePath
        {
            get { return packfilePath; }
            set { packfilePath = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 进度条最大长度
        /// </summary>
        private int maxFrameCount = 1;
        public int MaxFrameCount
        {
            get { return maxFrameCount; }
            set { maxFrameCount = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 进度条实时信息
        /// </summary>
        private int curFrameIndex = 0;
        public int CurFrameIndex
        {
            get { return curFrameIndex; }
            set { curFrameIndex = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 压缩状态
        /// </summary>
        private string status;
        public string Status
        {
            get { return status; }
            set { status = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 开发板信息
        /// </summary>
        public DevelopmentBoardMessage developmentBoardMessage { get; set; }

        #region ------------Command------------
        public DelegateCommand PackageCommand { get; set; }
        public DelegateCommand LoadFilePathCommand { get; set; }


        #endregion
        #region ------------PublicMethod------------

        #endregion

        #region ------------PrivateMethod------------
        /// <summary>
        /// 打包
        /// </summary>
        /// <returns></returns>
        private void Package()
        {
            if (SourcePath == null)
            {
                Status = "请先选择固件";
                return;
            }
            if (DevelopmentBoardMessage.VersionNumber == null || !DevelopmentBoardMessage.DeviceID.HasValue)
            {
                Status = "请先填写板卡信息和版本号";
                return;
            }
            Status = "请先选择解压完成后文件的位置";
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "请选择打包后的目标位置";
            folderBrowserDialog.UseDescriptionForTitle = true;
            var result = folderBrowserDialog.ShowDialog();
            string destinationPath;
            if (result != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            destinationPath = folderBrowserDialog.SelectedPath;

            Status = "正在打包";
            //将对象序列化为JSON字符串
            string JsonMessageFile = JsonConvert.SerializeObject(DevelopmentBoardMessage);
            //创建一个文件夹
            string BagMessageDirectoryPath = Path.GetDirectoryName(SourcePath) + @"\" + EnumMethod.GetEnumDesc(DevelopmentBoardMessage.DeviceID) + "_" + DevelopmentBoardMessage.VersionNumber + "_" + System.DateTime.Now.ToString("HH时mm分ss秒");
            if (!Directory.Exists(BagMessageDirectoryPath))
            {
                Directory.CreateDirectory(BagMessageDirectoryPath);
                //指定文件路径和文件名
                string jspmFilePath = BagMessageDirectoryPath + @"\DevelopmentBoardMessage.json";
                using (StreamWriter file = File.CreateText(jspmFilePath))
                {
                    file.Write(JsonMessageFile);
                }
                using (FileStream fs = new(jspmFilePath, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        sw.WriteLine(JsonMessageFile);
                    }
                }
            }
            else
            {
                return;
            }

            //压缩文件夹
            string zipPath = ZipCompress.CompressFile(BagMessageDirectoryPath);
            if (zipPath == null)
            {
                status = "压缩失败";
                return;
            }
            //向压缩文件中添加固件文件
            ZipCompress.ZipFiles(zipPath, SourcePath);
            Directory.Delete(BagMessageDirectoryPath, true);
            destinationPath += @"\" + Path.GetFileName(zipPath);
            File.Move(zipPath, destinationPath);
            Status = "打包完成";
            CurFrameIndex = 1;
        }

        private void LoadFilePath()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result = openFileDialog.ShowDialog();
            if (result == true)
            {
                SourcePath = openFileDialog.FileName;
                Status = "";
                CurFrameIndex = 0;
                DevelopmentBoardMessage.FileName = Path.GetFileName(SourcePath);
                ResolvingFilename(Path.GetFileNameWithoutExtension(SourcePath));
            }
        }
        //解析文件名
        private void ResolvingFilename(String filename)
        {

            var message = filename.Split('_');
            if (message.Length < 4)
            {
                return;
            }
            var firmwareName = message[0];
            var deviceID = message[1];
            var firmwareVersion = message[2];
            var hardwareVersion = message[3];
            firmwareVersion.Substring(1, firmwareVersion.Length - 1);
            var testDeviceID = ((EnumDeviceID)Convert.ToUInt32(deviceID, 16)); ;
            //解析设备id
            if (DevelopmentBoardMessage.DeviceID == null && Enum.IsDefined(typeof(EnumDeviceID), testDeviceID)) 
            {
                DevelopmentBoardMessage.DeviceID = testDeviceID;
            }
            //版本号
            if (DevelopmentBoardMessage.VersionNumber == "" || DevelopmentBoardMessage.VersionNumber == null)
            {
                
                if (firmwareVersion[0] == 'v' || firmwareVersion[0] == 'V')
                {
                    firmwareVersion = firmwareVersion.Substring(1, firmwareVersion.Length-1);
                }
                DevelopmentBoardMessage.VersionNumber = firmwareVersion;

            }
            //硬件版本
            if (DevelopmentBoardMessage.HardwareVersion == "" || DevelopmentBoardMessage.HardwareVersion == null)
            {
                
                DevelopmentBoardMessage.HardwareVersion = hardwareVersion;
            }
            // 固件名称
            if (DevelopmentBoardMessage.FirmwareName == "" || DevelopmentBoardMessage.FirmwareName == null)
            {
                DevelopmentBoardMessage.FirmwareName = firmwareName;
            }
               
        }
        

        /// <summary>
        /// 通过枚举类型获取描述属性
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public string GetDescriptionByEnum(Enum enumValue)
        {
            string value = enumValue.ToString();
            System.Reflection.FieldInfo field = enumValue.GetType().GetField(value);
            object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);    //获取描述属性
            if (objs.Length == 0)    //当描述属性没有时，直接返回名称
                return value;
            DescriptionAttribute descriptionAttribute = (DescriptionAttribute)objs[0];
            return descriptionAttribute.Description;
        }
        /// <summary>
        /// 通过描述属性获取枚举
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="description"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public T GetEnumByDescription<T>(string description) where T : Enum
        {
            System.Reflection.FieldInfo[] fields = typeof(T).GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);    //获取描述属性
                if (objs.Length > 0 && (objs[0] as DescriptionAttribute).Description == description)
                {
                    return (T)field.GetValue(null);
                }
            }

            throw new ArgumentException(string.Format("{0} 未能找到对应的枚举.", description), "Description");
        }



        #endregion

        #region ------------StaticMethod------------
        #endregion







    }
}
