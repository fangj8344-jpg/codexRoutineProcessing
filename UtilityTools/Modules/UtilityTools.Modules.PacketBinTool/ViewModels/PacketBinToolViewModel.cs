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

            PackageCommand = new DelegateCommand(Package);
            LoadFilePathCommand = new DelegateCommand(LoadFilePath);
            AllDeviceID = new Dictionary<string, ushort>() { { "主控制板", 0x1000 },{"真空控制板", 0x0100 }, { "灯带控制板", 0x0102 }, { "20kv高压箱", 0x0200 }, { "五轴电机控制板", 0x0300 } };
          

        }
        #endregion

        #region ------------Field------------

        #endregion

      
        
        private Dictionary<string, UInt16> _allDeviceID;
        
        /// <summary>
        /// 所有的设备ID
        /// </summary>
        public Dictionary<string, UInt16> AllDeviceID 
        { 
            get { return _allDeviceID; }
            set { _allDeviceID = value; RaisePropertyChanged(); }
        }
        private string _selectDeviceID;
        public string SelectDeviceID
        { 
            get { return _selectDeviceID; }
            set  { _selectDeviceID = value; RaisePropertyChanged(); }
        }
        /*
        private int _selectDeviceIDIndex;
        public int SelectDeviceIDIndex
        {
            get { return _selectDeviceIDIndex; }
            set { _selectDeviceIDIndex = value; RaisePropertyChanged(); }
        }
        */

        /// <summary>
        /// 板卡类型
        /// </summary>
        private string developmentBoardType;
        public string DevelopmentBoardType
        {
            get { return developmentBoardType; }
            set { developmentBoardType = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 烧录文件的版本号
        /// </summary>
        private string versionNumber;
        public string VersionNumber
        {
            get { return versionNumber; }
            set { versionNumber = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 版本变更信息
        /// </summary>
        private string updataInformation;
        public string UpdataInformation
        {
            get { return updataInformation; }
            set { updataInformation = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 备注信息
        /// </summary>
        private string description;
        public string Description
        {
            get { return description; }
            set { description = value; RaisePropertyChanged(); }
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
        public DevelopmentBoardMessage developmentBoardMessage{ get; set; }
  
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
            if (SelectDeviceID == null || VersionNumber == null )
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
            if (result != System.Windows.Forms.DialogResult.OK )
            {
                return;
            }
            destinationPath = folderBrowserDialog.SelectedPath;
            
            Status = "正在打包";
            //创建的对象，包含开发板信息
            DevelopmentBoardMessage bagMessage = new DevelopmentBoardMessage { DevelopmentBoardType = AllDeviceID[SelectDeviceID], VersionNumber = this.VersionNumber, UpdataInformation = this.UpdataInformation, Description = this.Description };
            //将对象序列化为JSON字符串
            string JsonMessageFile = JsonConvert.SerializeObject(bagMessage);
            //创建一个文件夹
            string BagMessageDirectoryPath = Path.GetDirectoryName(SourcePath) + @"\" + SelectDeviceID + "_" + versionNumber + "_" + System.DateTime.Now.ToString("HH时mm分ss秒");
            if (!Directory.Exists(BagMessageDirectoryPath))
            {
                Directory.CreateDirectory(BagMessageDirectoryPath);
                //指定文件路径和文件名
                string jspmFilePath = BagMessageDirectoryPath+ @"\DevelopmentBoardMessage.json";
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
            Directory.Delete(BagMessageDirectoryPath, true );
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
            }
        }
       
        
        #endregion

        #region ------------StaticMethod------------
        #endregion







    }
}
