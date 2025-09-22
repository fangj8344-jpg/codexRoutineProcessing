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
        private string _currentDirectory;
        private readonly string[] _supportedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".gif" };
        #endregion

        #region Commands
        public DelegateCommand OpenDirectoryCommand { get; set; }
        public DelegateCommand<string> SelectImageCommand { get; set; }

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
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error($"加载图像{imagePath}异常：{ex.Message}");
            }
        }
        #endregion

        #region Properties
        private string _directoryPath;
        public string DirectoryPath
        {
            get { return _directoryPath; }
            set
            {
                _directoryPath = value;
                RaisePropertyChanged();
            }
        }

        private string _currentImagePath;
        public string CurrentImagePath
        {
            get { return _currentImagePath; }
            set
            {
                _currentImagePath = value;
                RaisePropertyChanged();
            }
        }

        private BitmapImage _currentImage;
        public BitmapImage CurrentImage
        {
            get { return _currentImage; }
            set
            {
                _currentImage = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> _imageFiles;
        public ObservableCollection<string> ImageFiles
        {
            get { return _imageFiles; }
            set
            {
                _imageFiles = value;
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
        #endregion
    }
}
