using Microsoft.Win32;
using NLog;
using Prism.Commands;
using Prism.Ioc;
using System;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Mvvm;


namespace UtilityTools.Modules.ImageAnalyzer.ViewModels
{
    public class ImageAnalyzerViewModel : RegionViewModelBase
    {
        #region ------------Constructor------------
        public ImageAnalyzerViewModel(IDialogHostService dialogHostService, IContainerProvider containerProvider)
            : base(containerProvider)
        {
            _dialogHostService = dialogHostService;
            _containerProvider = containerProvider;

            InitCommand();
            InitProperty();
        }
        #endregion

        #region ------------Field------------
        private readonly IDialogHostService _dialogHostService;
        private readonly IContainerProvider _containerProvider;

        private delegate void ImagePathUpdatedHandler(string ImagePath);
        private event ImagePathUpdatedHandler ImagePathUpdated;

        private delegate void SaveResultHandler();
        private event SaveResultHandler SaveResultTriggered;

        #endregion


        #region Command
        public DelegateCommand SaveResultCommand { get; set; }
        private void SaveResult()
        {
            SaveResultTriggered();
        }

        public DelegateCommand OpenImageFileCommand { get; set; }
        private void OpenImageFile()
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Image File";
            dialog.Filter = "imageFile|*.png;*.tiff;*jpeg;*.jpg;*.bmp";

            if (dialog.ShowDialog() == true)
            {
                var path = dialog.FileName;
                FilePath = path;
                try
                {
                    ImagePathUpdated(path);
                }
                catch (Exception ex)
                { 
                    LogManager.GetCurrentClassLogger().Error($"加载图像{FilePath}异常：{ex.Message}");
                }
            }
        }


        #endregion

        #region ------------Property------------


        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                RaisePropertyChanged();
            }
        }


        public ImageNoiseSpectrumViewModel _imageNoiseSpectrumVM;
        public ImageNoiseSpectrumViewModel ImageNoiseSpectrumVM
        {
            get { return _imageNoiseSpectrumVM; }
            set { _imageNoiseSpectrumVM = value; RaisePropertyChanged(); }
        }

        public ImageHistogramViewModel _imageHistogramVM;
        public ImageHistogramViewModel ImageHistogramVM
        {
            get { return _imageHistogramVM; }
            set { _imageHistogramVM = value; RaisePropertyChanged(); }
        }

        private ImageOptViewModel _imageOptVM;

        public ImageOptViewModel ImageOptVM
        {
            get { return _imageOptVM; }
            set { _imageOptVM = value; RaisePropertyChanged(); }
        }

        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        private void InitCommand()
        {
            OpenImageFileCommand = new DelegateCommand(OpenImageFile);
            SaveResultCommand = new DelegateCommand(SaveResult);
        }

        private void InitProperty()
        {
            _imageNoiseSpectrumVM = new ImageNoiseSpectrumViewModel(this._containerProvider);
            ImagePathUpdated += ImageNoiseSpectrumVM.UpdateImage;
            SaveResultTriggered += ImageNoiseSpectrumVM.SaveResult;

            _imageHistogramVM = new ImageHistogramViewModel(this._containerProvider);
            ImagePathUpdated += ImageHistogramVM.UpdateImage;
            SaveResultTriggered += ImageHistogramVM.SaveResult;

            ImageOptVM = new ImageOptViewModel(this._containerProvider);
            ImagePathUpdated += ImageOptVM.UpdateImage;
            SaveResultTriggered += ImageOptVM.SaveResult;
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion




    }
}
