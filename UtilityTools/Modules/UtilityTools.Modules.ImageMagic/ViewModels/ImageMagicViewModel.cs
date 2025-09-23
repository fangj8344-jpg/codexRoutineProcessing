using Microsoft.Win32;
using NLog;
using Prism.Commands;
using Prism.Ioc;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Mvvm;
using UtilityTools.Core.Model;
using UtilityTools.Modules.ImageMagic.Model;

namespace UtilityTools.Modules.ImageMagic.ViewModels
{
    public class ImageMagicViewModel : RegionViewModelBase
    {
        #region Constructor
        public ImageMagicViewModel(IDialogHostService dialogHostService, IContainerProvider containerProvider)
            : base(containerProvider)
        {
            _dialogHostService = dialogHostService;
            _containerProvider = containerProvider;

            InitCommand();
            InitProperty();
        }
        #endregion

        #region Fields
        private readonly IDialogHostService _dialogHostService;
        private readonly IContainerProvider _containerProvider;
        private string _currentDirectory = string.Empty;
        private readonly string[] _supportedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".gif" };

        // 属性名称常量
        private const string Property_Name = "机器型号";
        private const string Property_AccVol = "加速电压";
        private const string Property_AvgLen = "点平均数";
        private const string Property_Date = "拍照时间";
        private const string Property_Detector = "探测器";
        private const string Property_FieldSize = "扫描尺寸";
        private const string Property_FirmwareVersion = "固件版本";
        private const string Property_SoftwareVersion = "软件版本";
        private const string Property_Position = "拍照坐标";
        private const string Property_Pressure = "枪头真空";
        private const string Property_Zoom = "放大倍数";
        private const string Property_SignificantBit = "数据标识";
        private const string Property_OB = "物镜高度";
        private const string Property_PixelLength = "像素尺寸";
        private const string Property_Title = "图像标题";
        private const string Property_Note = "图像备注";
        private const string Property_Freq = "采样频率";
        #endregion

        #region Commands
        public DelegateCommand OpenDirectoryCommand { get; set; } = null!;
        public DelegateCommand<string> SelectImageCommand { get; set; } = null!;
        public DelegateCommand BeautifyImageCommand { get; set; } = null!;
        public DelegateCommand BatchProcessCommand { get; set; } = null!;

        private void OpenDirectory()
        {
            try
            {
                var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
                folderDialog.Description = "选择图片文件夹";

                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var path = folderDialog.SelectedPath;
                    DirectoryPath = path;
                    _currentDirectory = path;
                    
                    LoadImagesFromDirectory(path);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error($"打开目录对话框异常：{ex.Message}");
            }
        }

        private void SelectImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                return;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                CurrentImage = bitmap;
                CurrentImagePath = imagePath;

                // 更新元数据
                UpdateMetadata(imagePath);
                
                // 检查并加载对应的美化图
                LoadBeautifiedImageIfExists(imagePath);
                
                // 更新美图命令状态
                BeautifyImageCommand.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error($"加载图像{imagePath}异常：{ex.Message}");
            }
        }
        #endregion

        #region Properties
        private string _directoryPath = string.Empty;
        public string DirectoryPath
        {
            get { return _directoryPath; }
            set
            {
                _directoryPath = value;
                RaisePropertyChanged();
            }
        }

        private string _currentImagePath = string.Empty;
        public string CurrentImagePath
        {
            get { return _currentImagePath; }
            set
            {
                _currentImagePath = value;
                RaisePropertyChanged();
            }
        }

        private BitmapImage? _currentImage;
        public BitmapImage? CurrentImage
        {
            get { return _currentImage; }
            set
            {
                _currentImage = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> _imageFiles = null!;
        public ObservableCollection<string> ImageFiles
        {
            get { return _imageFiles; }
            set
            {
                _imageFiles = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<PropertyInfoModel> _items = null!;
        public ObservableCollection<PropertyInfoModel> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged();
            }
        }

        // 美图相关属性
        private BitmapImage? _beautifiedImage;
        public BitmapImage? BeautifiedImage
        {
            get { return _beautifiedImage; }
            set
            {
                _beautifiedImage = value;
                RaisePropertyChanged();
            }
        }

        private string _beautifiedImagePath = string.Empty;
        public string BeautifiedImagePath
        {
            get { return _beautifiedImagePath; }
            set
            {
                _beautifiedImagePath = value;
                RaisePropertyChanged();
            }
        }

        private bool _isBeautifying;
        public bool IsBeautifying
        {
            get { return _isBeautifying; }
            set
            {
                _isBeautifying = value;
                RaisePropertyChanged();
            }
        }

        // 批处理相关属性
        private bool _isBatchProcessing;
        public bool IsBatchProcessing
        {
            get { return _isBatchProcessing; }
            set
            {
                _isBatchProcessing = value;
                RaisePropertyChanged();
            }
        }

        private int _batchProgress;
        public int BatchProgress
        {
            get { return _batchProgress; }
            set
            {
                _batchProgress = value;
                RaisePropertyChanged();
            }
        }

        private int _batchTotal;
        public int BatchTotal
        {
            get { return _batchTotal; }
            set
            {
                _batchTotal = value;
                RaisePropertyChanged();
            }
        }

        private string _batchStatus = string.Empty;
        public string BatchStatus
        {
            get { return _batchStatus; }
            set
            {
                _batchStatus = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Private Methods
        private void InitCommand()
        {
            OpenDirectoryCommand = new DelegateCommand(OpenDirectory);
            SelectImageCommand = new DelegateCommand<string>(SelectImage);
            BeautifyImageCommand = new DelegateCommand(BeautifyImage, CanBeautifyImage);
            BatchProcessCommand = new DelegateCommand(BatchProcess, CanBatchProcess);
        }

        private void InitProperty()
        {
            ImageFiles = new ObservableCollection<string>();
            Items = new ObservableCollection<PropertyInfoModel>();

            // 初始化属性列表
            Items.Add(new PropertyInfoModel(Property_Name, "Name", string.Empty));
            Items.Add(new PropertyInfoModel(Property_AccVol, "HighVol", "kV"));
            Items.Add(new PropertyInfoModel(Property_Date, "Date", string.Empty));
            Items.Add(new PropertyInfoModel(Property_Detector, "Detecter", string.Empty));
            Items.Add(new PropertyInfoModel(Property_FieldSize, "FieldSize", "μm"));
            Items.Add(new PropertyInfoModel(Property_Position, "Position", string.Empty));
            Items.Add(new PropertyInfoModel(Property_Zoom, "Mag", string.Empty));
            Items.Add(new PropertyInfoModel(Property_OB, "OB", "mm"));
            Items.Add(new PropertyInfoModel(Property_AvgLen, "AveragePoints", string.Empty));
            Items.Add(new PropertyInfoModel(Property_PixelLength, "PixelLength", "μm"));
            Items.Add(new PropertyInfoModel(Property_Pressure, "Pressure", string.Empty));
            Items.Add(new PropertyInfoModel(Property_Freq, "Frequency", "Hz"));
            Items.Add(new PropertyInfoModel(Property_SignificantBit, "DataFlag", string.Empty));
            Items.Add(new PropertyInfoModel(Property_FirmwareVersion, "FirmwareVersion", string.Empty));
            Items.Add(new PropertyInfoModel(Property_SoftwareVersion, "Version", string.Empty));
            Items.Add(new PropertyInfoModel(Property_Title, "Title", string.Empty));
            Items.Add(new PropertyInfoModel(Property_Note, "Note", string.Empty));
        }

        private void LoadImagesFromDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return;

            ImageFiles.Clear();
            CurrentImage = null;
            CurrentImagePath = string.Empty;
            
            // 清空美化图显示
            BeautifiedImage = null;
            BeautifiedImagePath = string.Empty;

            var files = Directory.GetFiles(directoryPath)
                .Where(file => _supportedExtensions.Contains(Path.GetExtension(file).ToLower()))
                .Where(file => !Path.GetFileNameWithoutExtension(file).EndsWith("_beautified")) // 过滤掉美化图文件
                .OrderBy(f => f)
                .ToList();

            foreach (var file in files)
            {
                ImageFiles.Add(file);
            }

            // 自动加载第一张图片
            if (ImageFiles.Count > 0)
            {
                SelectImage(ImageFiles.First());
            }
            
            // 更新批处理命令状态
            BatchProcessCommand.RaiseCanExecuteChanged();
        }

        private void UpdateMetadata(string imagePath)
        {
            try
            {
                var metadata = ImageMetadata.ReadMetadata(imagePath);
                if (metadata != null)
                {
                    UpdatePropertyList(metadata, Items);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Warn($"读取元数据失败 {imagePath}: {ex.Message}");
                // 清空属性值
                foreach (var item in Items)
                {
                    item.Value = string.Empty;
                }
            }
        }

        private void UpdatePropertyList(ZemMetaData metaData, ObservableCollection<PropertyInfoModel> list)
        {
            if (metaData != null)
            {
                foreach (var item in list)
                {
                    if (item != null)
                    {
                        switch (item.Description)
                        {
                            case Property_Name:
                                item.Value = metaData.Model ?? string.Empty;
                                break;
                            case Property_AccVol:
                                item.Value = metaData.AccVoltage.ToString();
                                break;
                            case Property_AvgLen:
                                item.Value = metaData.AvgLength.ToString();
                                break;
                            case Property_Date:
                                item.Value = metaData.Date ?? string.Empty;
                                break;
                            case Property_Detector:
                                item.Value = metaData.Detector ?? string.Empty;
                                break;
                            case Property_FieldSize:
                                item.Value = metaData.FieldSize.ToString();
                                break;
                            case Property_FirmwareVersion:
                                item.Value = metaData.FirmwareVersion ?? string.Empty;
                                break;
                            case Property_SoftwareVersion:
                                item.Value = metaData.Version ?? string.Empty;
                                break;
                            case Property_Position:
                                item.Value = metaData.Position ?? string.Empty;
                                break;
                            case Property_Pressure:
                                item.Value = metaData.Pressure ?? string.Empty;
                                break;
                            case Property_Zoom:
                                item.Value = metaData.Zoom.ToString();
                                break;
                            case Property_SignificantBit:
                                item.Value = metaData.SignificantBit.ToString();
                                break;
                            case Property_OB:
                                item.Value = metaData.Ob.ToString();
                                break;
                            case Property_PixelLength:
                                item.Value = metaData.PixelLength.ToString();
                                break;
                            case Property_Title:
                                item.Value = metaData.Title ?? string.Empty;
                                break;
                            case Property_Note:
                                item.Value = metaData.Note ?? string.Empty;
                                break;
                            case Property_Freq:
                                item.Value = metaData.Frequency.ToString();
                                break;
                        }
                    }
                }
            }
        }

        private bool CanBeautifyImage()
        {
            return !string.IsNullOrEmpty(CurrentImagePath) && !IsBeautifying;
        }

        private async void BeautifyImage()
        {
            if (string.IsNullOrEmpty(CurrentImagePath))
            {
                return;
            }

            try
            {
                IsBeautifying = true;
                BeautifyImageCommand.RaiseCanExecuteChanged();
                BatchProcessCommand.RaiseCanExecuteChanged();

                // 调用美图API
                string beautifiedBase64 = await MeituAlgorithmMethod.PostByFile(CurrentImagePath);
                
                if (!string.IsNullOrEmpty(beautifiedBase64))
                {
                    // 将Base64转换为BitmapImage
                    BeautifiedImage = MeituAlgorithmMethod.Base64ToImage(beautifiedBase64);
                    
                    // 保存美化后的图片到原图所在目录
                    string originalDirectory = Path.GetDirectoryName(CurrentImagePath) ?? string.Empty;
                    string originalFileName = Path.GetFileNameWithoutExtension(CurrentImagePath);
                    string originalExtension = Path.GetExtension(CurrentImagePath);
                    string beautifiedFileName = $"{originalFileName}_beautified{originalExtension}";
                    string beautifiedFilePath = Path.Combine(originalDirectory, beautifiedFileName);
                    
                    // 将Base64转换为字节数组并保存
                    byte[] imageBytes = Convert.FromBase64String(beautifiedBase64);
                    File.WriteAllBytes(beautifiedFilePath, imageBytes);
                    
                    BeautifiedImagePath = beautifiedFilePath;
                    LogManager.GetCurrentClassLogger().Info($"美化图片已保存到: {beautifiedFilePath}");
                }
                else
                {
                    LogManager.GetCurrentClassLogger().Warn("美图API返回空结果");
                }
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error($"美图处理失败: {ex.Message}");
                // 可以显示错误消息给用户
            }
            finally
            {
                IsBeautifying = false;
                BeautifyImageCommand.RaiseCanExecuteChanged();
                BatchProcessCommand.RaiseCanExecuteChanged();
            }
        }

        private void LoadBeautifiedImageIfExists(string imagePath)
        {
            try
            {
                // 生成对应的美化图路径
                string originalDirectory = Path.GetDirectoryName(imagePath) ?? string.Empty;
                string originalFileName = Path.GetFileNameWithoutExtension(imagePath);
                string originalExtension = Path.GetExtension(imagePath);
                string beautifiedFileName = $"{originalFileName}_beautified{originalExtension}";
                string beautifiedFilePath = Path.Combine(originalDirectory, beautifiedFileName);

                // 检查美化图文件是否存在
                if (File.Exists(beautifiedFilePath))
                {
                    // 加载美化图
                    var beautifiedBitmap = new BitmapImage();
                    beautifiedBitmap.BeginInit();
                    beautifiedBitmap.UriSource = new Uri(beautifiedFilePath);
                    beautifiedBitmap.CacheOption = BitmapCacheOption.OnLoad;
                    beautifiedBitmap.EndInit();
                    beautifiedBitmap.Freeze();

                    BeautifiedImage = beautifiedBitmap;
                    BeautifiedImagePath = beautifiedFilePath;
                }
                else
                {
                    // 清空美化图显示
                    BeautifiedImage = null;
                    BeautifiedImagePath = string.Empty;
                }
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error($"加载美化图失败: {ex.Message}");
                // 清空美化图显示
                BeautifiedImage = null;
                BeautifiedImagePath = string.Empty;
            }
        }

        private bool CanBatchProcess()
        {
            return ImageFiles.Count > 0 && !IsBatchProcessing && !IsBeautifying;
        }

        private async void BatchProcess()
        {
            if (ImageFiles.Count == 0)
            {
                return;
            }

            try
            {
                IsBatchProcessing = true;
                BatchProgress = 0;
                BatchTotal = ImageFiles.Count;
                BatchStatus = "开始批量处理...";
                
                // 更新命令状态
                BatchProcessCommand.RaiseCanExecuteChanged();
                BeautifyImageCommand.RaiseCanExecuteChanged();

                int successCount = 0;
                int failCount = 0;

                for (int i = 0; i < ImageFiles.Count; i++)
                {
                    string imagePath = ImageFiles[i];
                    string fileName = Path.GetFileName(imagePath);
                    
                    try
                    {
                        BatchStatus = $"正在处理: {fileName} ({i + 1}/{BatchTotal})";
                        
                        // 检查是否已经存在美化图
                        string originalDirectory = Path.GetDirectoryName(imagePath) ?? string.Empty;
                        string originalFileName = Path.GetFileNameWithoutExtension(imagePath);
                        string originalExtension = Path.GetExtension(imagePath);
                        string beautifiedFileName = $"{originalFileName}_beautified{originalExtension}";
                        string beautifiedFilePath = Path.Combine(originalDirectory, beautifiedFileName);

                        if (File.Exists(beautifiedFilePath))
                        {
                            LogManager.GetCurrentClassLogger().Info($"跳过已存在的美化图: {beautifiedFilePath}");
                            BatchProgress = i + 1;
                            continue;
                        }

                        // 调用美图API
                        string beautifiedBase64 = await MeituAlgorithmMethod.PostByFile(imagePath);
                        
                        if (!string.IsNullOrEmpty(beautifiedBase64))
                        {
                            // 保存美化后的图片
                            byte[] imageBytes = Convert.FromBase64String(beautifiedBase64);
                            File.WriteAllBytes(beautifiedFilePath, imageBytes);
                            
                            successCount++;
                            LogManager.GetCurrentClassLogger().Info($"美化图片成功: {beautifiedFilePath}");
                        }
                        else
                        {
                            failCount++;
                            LogManager.GetCurrentClassLogger().Warn($"美化图片失败（API返回空）: {imagePath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        LogManager.GetCurrentClassLogger().Error($"美化图片失败: {imagePath}, 错误: {ex.Message}");
                    }
                    
                    BatchProgress = i + 1;
                }

                BatchStatus = $"批量处理完成! 成功: {successCount}, 失败: {failCount}";
                LogManager.GetCurrentClassLogger().Info($"批量处理完成，成功: {successCount}, 失败: {failCount}");
                
                // 如果当前选中的图片有了新的美化图，重新加载
                if (!string.IsNullOrEmpty(CurrentImagePath))
                {
                    LoadBeautifiedImageIfExists(CurrentImagePath);
                }
            }
            catch (Exception ex)
            {
                BatchStatus = $"批量处理出错: {ex.Message}";
                LogManager.GetCurrentClassLogger().Error($"批量处理出错: {ex.Message}");
            }
            finally
            {
                IsBatchProcessing = false;
                // 更新命令状态
                BatchProcessCommand.RaiseCanExecuteChanged();
                BeautifyImageCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion
    }
}
