using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using NLog;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.NavigationImagePositioning.Model;
using UtilityTools.Modules.NavigationImagePositioning.Services;
using UtilityTools.Modules.NavigationImagePositioning.Utility;

namespace UtilityTools.Modules.NavigationImagePositioning.ViewModels
{
    /// <summary>
    /// 导航图检测页视图模型：扫描 <see cref="NavigationModelPaths"/> 下的 .onnx、加载推理服务、
    /// 显示原图/结果图与检测列表，并在导航进入时自动刷新模型列表。
    /// </summary>
    public class NavigationImagePositioningViewModel : RegionViewModelBase
    {
        private static readonly ILogger _log = LogManager.GetCurrentClassLogger();
        private readonly INavigationYoloInferenceService _yolo;

        public NavigationImagePositioningViewModel(
            IContainerProvider containerProvider,
            INavigationYoloInferenceService yoloInferenceService)
            : base(containerProvider)
        {
            _yolo = yoloInferenceService;
            ModelStorageDirectory = NavigationModelPaths.GetOnnxDirectory();
            Detections = new ObservableCollection<NavigationDetectionResult>();
            OnnxModelItems = new ObservableCollection<NavigationOnnxFileItem>();

            RefreshModelListCommand = new DelegateCommand(RefreshModelList);
            OpenImageCommand = new DelegateCommand(OpenImage);
            //当以下三个属性修改时重新评估CanRunDetection
            RunDetectionCommand = new DelegateCommand(RunDetection, CanRunDetection)
                .ObservesProperty(() => ModelPath)
                .ObservesProperty(() => ImagePath)
                .ObservesProperty(() => SelectedModelItem);
        }

        public ObservableCollection<NavigationDetectionResult> Detections { get; }

        /// <summary>运行时存放 onnx 的目录（可在此放模型，或点「刷新列表」重新扫描）。</summary>
        public string ModelStorageDirectory { get; }

        public ObservableCollection<NavigationOnnxFileItem> OnnxModelItems { get; }

        private NavigationOnnxFileItem? _selectedModelItem;
        public NavigationOnnxFileItem? SelectedModelItem
        {
            get => _selectedModelItem;
            set
            {
                if (value == null)
                {
                    if (!SetProperty(ref _selectedModelItem, null))
                    {
                        return;
                    }

                    _yolo.UnloadModel();
                    ModelPath = "";
                    return;
                }

                if (!SetProperty(ref _selectedModelItem, value))
                {
                    return;
                }

                try
                {
                    _yolo.LoadModel(value.FullPath);
                    ModelPath = value.FullPath;
                    StatusMessage = "已加载模型: " + value.FileName;
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "加载 ONNX 失败");
                    _yolo.UnloadModel();
                    ModelPath = "";
                    _selectedModelItem = null;
                    RaisePropertyChanged(nameof(SelectedModelItem));
                    StatusMessage = "加载模型失败: " + ex.Message;
                }
            }
        }

        private string _modelPath = "";
        public string ModelPath
        {
            get => _modelPath;
            set => SetProperty(ref _modelPath, value);
        }

        private string _imagePath = "";
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                if (!SetProperty(ref _imagePath, value))
                {
                    return;
                }

                ResultPreview = null;
                LoadImagePreview();
            }
        }

        private string _statusMessage = "请在模型目录中放入 .onnx 文件，再从下拉框选择。";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private ImageSource? _inputPreview;
        public ImageSource? InputPreview
        {
            get => _inputPreview;
            set
            {
                if (SetProperty(ref _inputPreview, value))
                {
                    RaisePropertyChanged(nameof(DisplayImage));
                }
            }
        }

        private ImageSource? _resultPreview;
        public ImageSource? ResultPreview
        {
            get => _resultPreview;
            set
            {
                if (SetProperty(ref _resultPreview, value))
                {
                    RaisePropertyChanged(nameof(DisplayImage));
                }
            }
        }

        public ImageSource? DisplayImage => ResultPreview ?? InputPreview;

        public DelegateCommand RefreshModelListCommand { get; }
        public DelegateCommand OpenImageCommand { get; }
        public DelegateCommand RunDetectionCommand { get; }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            RefreshModelList();
        }

        private void LoadImagePreview()
        {
            if (string.IsNullOrWhiteSpace(ImagePath) || !File.Exists(ImagePath))
            {
                InputPreview = null;
                return;
            }

            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri(Path.GetFullPath(ImagePath), UriKind.Absolute);
                bmp.EndInit();
                bmp.Freeze();
                InputPreview = bmp;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "无法加载图片预览: {0}", ImagePath);
                InputPreview = null;
            }
        }

        private void RefreshModelList()
        {
            var previousPath = SelectedModelItem?.FullPath;
            var dir = NavigationModelPaths.GetOnnxDirectory();
            try
            {
                Directory.CreateDirectory(dir);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "无法创建或访问模型目录: {0}", dir);
                StatusMessage = "无法使用模型目录: " + ex.Message;
                return;
            }

            OnnxModelItems.Clear();
            var files = Directory
                .GetFiles(dir, "*.onnx", SearchOption.TopDirectoryOnly)
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var f in files)
            {
                OnnxModelItems.Add(new NavigationOnnxFileItem(f));
            }

            if (files.Count == 0)
            {
                SelectedModelItem = null;
                StatusMessage = $"模型目录中暂无 .onnx 文件: {dir}（将模型放入该目录后点「刷新列表」）。";
                return;
            }

            var match = string.IsNullOrEmpty(previousPath)
                ? null
                : OnnxModelItems.FirstOrDefault(
                    x => string.Equals(x.FullPath, previousPath, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                // 原逻辑只改私有字段+Raise，不会更新 ModelPath，且无法触发 RunDetection 的 CanExecute 重算
                _selectedModelItem = match;
                RaisePropertyChanged(nameof(SelectedModelItem));
                try
                {
                    if (File.Exists(match.FullPath) &&
                        (!_yolo.IsModelLoaded
                         || !string.Equals(ModelPath, match.FullPath, StringComparison.OrdinalIgnoreCase)))
                    {
                        _yolo.LoadModel(match.FullPath);
                    }

                    ModelPath = match.FullPath;
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "恢复上次选择的模型时加载失败");
                    StatusMessage = "加载模型失败: " + ex.Message;
                }

                RunDetectionCommand.RaiseCanExecuteChanged();
            }
            else if (files.Count == 1)
            {
                SelectedModelItem = OnnxModelItems[0];
            }
            else
            {
                _selectedModelItem = null;
                RaisePropertyChanged(nameof(SelectedModelItem));
                _yolo.UnloadModel();
                ModelPath = "";
            }

            StatusMessage = string.IsNullOrEmpty(previousPath) && match == null
                ? $"已扫描 {files.Count} 个 ONNX 模型，请选择。目录: {dir}"
                : (match != null
                    ? $"已扫描 {files.Count} 个 ONNX 模型（已保持上次选择）。目录: {dir}"
                    : $"已扫描 {files.Count} 个 ONNX 模型，请重新选择。目录: {dir}");
        }

        private void OpenImage()
        {
            var dialog = new OpenFileDialog
            {
                Title = "选择导航图",
                Filter = "图像|*.png;*.jpg;*.jpeg;*.bmp;*.tiff;*.tif|所有文件|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                ImagePath = dialog.FileName;
            }
        }

        /// <summary>
        /// 与 ObservesProperty 一致：仅用可通知属性 + 文件存在判断。勿单独依赖
        /// <see cref="INavigationYoloInferenceService.IsModelLoaded"/>，否则刷新列表仅恢复
        /// <see cref="SelectedModelItem"/> 而不改 <see cref="ModelPath"/> 时，不会触发命令重算，按钮会一直保持禁用。
        /// </summary>
        private bool CanRunDetection() =>
            !string.IsNullOrWhiteSpace(ModelPath)
            && File.Exists(ModelPath)
            && !string.IsNullOrWhiteSpace(ImagePath)
            && File.Exists(ImagePath);

        private void RunDetection()
        {
            UpdateLoading(true, "检测中…");
            try
            {
                Detections.Clear();
                ResultPreview = null;
                var outcome = _yolo.DetectFromFile(ImagePath);
                foreach (var item in outcome.Detections)
                {
                    Detections.Add(item);
                }

                ResultPreview = outcome.AnnotatedImage;
                RaisePropertyChanged(nameof(DisplayImage));

                StatusMessage = outcome.Detections.Count == 0
                    ? "检测完成：未检出目标（可适当调低置信度阈值或检查模型/图像）。"
                    : $"检测完成，共 {outcome.Detections.Count} 个目标。";
            }
            catch (Exception ex)
            {
                _log.Error(ex, "检测过程异常");
                StatusMessage = "检测失败: " + ex.Message;
            }
            finally
            {
                UpdateLoading(false);
            }
        }
    }
}
