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
        #endregion

        #region Private Methods
        private void InitCommand()
        {
            OpenDirectoryCommand = new DelegateCommand(OpenDirectory);
            SelectImageCommand = new DelegateCommand<string>(SelectImage);
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

            var files = Directory.GetFiles(directoryPath)
                .Where(file => _supportedExtensions.Contains(Path.GetExtension(file).ToLower()))
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
        #endregion
    }
}
