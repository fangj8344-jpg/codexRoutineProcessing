using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using UtilityTools.Core;
using UtilityTools.Core.Event;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Model;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.MotorTest.Model;
using UtilityTools.Modules.MultiAxisTest.Model;
using UtilityTools.Services.Interfaces;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.MultiAxisTest.ViewModels
{
    public class MultiAxisScanViewModel : RegionViewModelBase, INavigationAware, IRegionMemberLifetime
    {
        private SubscriptionToken _token;

        private readonly IRegionManager _regionManager;
        
        private readonly IContainerProvider _containerProvider;
        private readonly IThingboardService _thingboardService;

        private IEventAggregator _eventAggregator;
        // 云端查询状态
        private Visibility _cloudCheckingVisibility = Visibility.Collapsed;
        public Visibility CloudCheckingVisibility
        {
            get => _cloudCheckingVisibility;
            set => SetProperty(ref _cloudCheckingVisibility, value);

        }
        private Visibility _cloudResultVisibility = Visibility.Collapsed;
        public Visibility CloudResultVisibility
        {
            get => _cloudResultVisibility;
            set => SetProperty(ref _cloudResultVisibility, value);
        }
        private string _cloudResultText = string.Empty;
        public string CloudResultText
        {
            get => _cloudResultText;
            set => SetProperty(ref _cloudResultText, value);
        }
        private Brush _cloudResultColor = Brushes.Gray;
        public Brush CloudResultColor
        {
            get => _cloudResultColor;
            set => SetProperty(ref _cloudResultColor, value);
        }
        private Visibility _cloudRecordVisibility = Visibility.Collapsed;
        public Visibility CloudRecordVisibility
        {
            get => _cloudRecordVisibility;
            set => SetProperty(ref _cloudRecordVisibility, value);
        }
        private string _cloudRecordDeviceId = string.Empty;
        public string CloudRecordDeviceId
        {
            get => _cloudRecordDeviceId;
            set => SetProperty(ref _cloudRecordDeviceId, value);
        }
        private string _cloudRecordUpdatedAt = string.Empty;
        public string CloudRecordUpdatedAt
        {
            get => _cloudRecordUpdatedAt;
            set => SetProperty(ref _cloudRecordUpdatedAt, value);
        }
        private string _cloudRecordContentUrl = string.Empty;
        public string CloudRecordContentUrl
        {
            get => _cloudRecordContentUrl;
            set => SetProperty(ref _cloudRecordContentUrl, value);
        }
        /// <summary>云端 content 为对象时的摘要（开始/结束时间、电机数等）。</summary>
        private string _cloudRecordSummary = string.Empty;
        public string CloudRecordSummary
        {
            get => _cloudRecordSummary;
            set => SetProperty(ref _cloudRecordSummary, value);
        }

        public MultiAxisWorkflowState State { get; }
        public ObservableCollection<CloudKvDisplayItem> CloudContentFields { get; } = new();
        public ObservableCollection<CloudMotorAxisDisplay> CloudMotors { get; } = new();
        public DelegateCommand ConfirmCommand { get; private set; }
        public DelegateCommand OpenEngineerConfigCommand { get; private set; }
        public DelegateCommand SaveThresholdConfigCommand { get; private set; }

        private string _thresholdStageTypeDisplay = "未识别";
        public string ThresholdStageTypeDisplay
        {
            get => _thresholdStageTypeDisplay;
            set => SetProperty(ref _thresholdStageTypeDisplay, value);
        }

        private string _movementMinDistancePulseText = "50";
        public string MovementMinDistancePulseText
        {
            get => _movementMinDistancePulseText;
            set => SetProperty(ref _movementMinDistancePulseText, value);
        }

        private string _encoderMinDeltaPulseText = "100";
        public string EncoderMinDeltaPulseText
        {
            get => _encoderMinDeltaPulseText;
            set => SetProperty(ref _encoderMinDeltaPulseText, value);
        }

        private string _limitAccuracyMaxDiffPulseText = "200";
        public string LimitAccuracyMaxDiffPulseText
        {
            get => _limitAccuracyMaxDiffPulseText;
            set => SetProperty(ref _limitAccuracyMaxDiffPulseText, value);
        }

        private string _linearStdDevMaxUmText = "1.0";
        public string LinearStdDevMaxUmText
        {
            get => _linearStdDevMaxUmText;
            set => SetProperty(ref _linearStdDevMaxUmText, value);
        }

        private string _smoothnessStdDevMaxUmText = "150.0";
        public string SmoothnessStdDevMaxUmText
        {
            get => _smoothnessStdDevMaxUmText;
            set => SetProperty(ref _smoothnessStdDevMaxUmText, value);
        }

        private string _xFullTravelMinPulseText = "235000";
        public string XFullTravelMinPulseText
        {
            get => _xFullTravelMinPulseText;
            set => SetProperty(ref _xFullTravelMinPulseText, value);
        }

        private string _xFullTravelMaxPulseText = "255000";
        public string XFullTravelMaxPulseText
        {
            get => _xFullTravelMaxPulseText;
            set => SetProperty(ref _xFullTravelMaxPulseText, value);
        }

        private string _yFullTravelMinPulseText = "215000";
        public string YFullTravelMinPulseText
        {
            get => _yFullTravelMinPulseText;
            set => SetProperty(ref _yFullTravelMinPulseText, value);
        }

        private string _yFullTravelMaxPulseText = "225000";
        public string YFullTravelMaxPulseText
        {
            get => _yFullTravelMaxPulseText;
            set => SetProperty(ref _yFullTravelMaxPulseText, value);
        }

        private string _thresholdSaveMessage = "请先扫码识别样品台，再检查/修改测试标准";
        public string ThresholdSaveMessage
        {
            get => _thresholdSaveMessage;
            set => SetProperty(ref _thresholdSaveMessage, value);
        }

        private Brush _thresholdSaveMessageColor = Brushes.Gray;
        public Brush ThresholdSaveMessageColor
        {
            get => _thresholdSaveMessageColor;
            set => SetProperty(ref _thresholdSaveMessageColor, value);
        }

        private Visibility _thresholdPanelVisibility = Visibility.Collapsed;
        public Visibility ThresholdPanelVisibility
        {
            get => _thresholdPanelVisibility;
            set => SetProperty(ref _thresholdPanelVisibility, value);
        }

        private Visibility _thresholdPlaceholderVisibility = Visibility.Visible;
        public Visibility ThresholdPlaceholderVisibility
        {
            get => _thresholdPlaceholderVisibility;
            set => SetProperty(ref _thresholdPlaceholderVisibility, value);
        }

        public MultiAxisScanViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, MultiAxisWorkflowState state, IContainerProvider containerProvider )
            : base(containerProvider)
        {
            _regionManager = regionManager;
            State = state;
            _eventAggregator = eventAggregator;
            ConfirmCommand = new DelegateCommand(Confirm, CanConfirm);
            ApplyEnglishKeyboardForBarcodeScan();

            _thingboardService = containerProvider.Resolve<IServiceFactory>().GetThingboardService();
            OpenEngineerConfigCommand = new DelegateCommand(ExecuteOpenEngineerConfig);
            SaveThresholdConfigCommand = new DelegateCommand(ExecuteSaveThresholdConfig);
            // 兜底：扫码页VM创建后先打开一次扫码捕获，避免首帧导航时机差导致失效
            _eventAggregator.GetEvent<BarcodeCaptureEnabledEvent>().Publish(true);
            LoadThresholdConfigForStage(string.Empty);
            SetThresholdDisplayState(false);
        }

        /// <summary>
        /// 打开配置界面
        /// </summary>
        private async void ExecuteOpenEngineerConfig()
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("MultiAxisScanViewHost")) return;
            var view = new Views.EngineerConfigView();
            await MaterialDesignThemes.Wpf.DialogHost.Show(view, "MultiAxisScanViewHost");
        }

        private bool CanConfirm()
        {
            // 必须有扫码文本，且生产日期和序号都没问题
            return !string.IsNullOrWhiteSpace(State.CurrentScanText)
                   && State.CurrentScanDisplay != null
                   && !string.IsNullOrWhiteSpace(State.CurrentScanDisplay.SerialNumber);
        }


        /// <summary>
        /// 扫码枪扫码触发事件
        /// </summary>
        /// <param name="barcodeParts"></param>
        private void OnBarcodeReceived(string[] barcodeParts)
        {
            for (int i = 0; i < barcodeParts.Length; i++)
            {
                barcodeParts[i] = NormalizeToEnglishInput(barcodeParts[i]);
            }
            State.CurrentScanText = string.Join("/", barcodeParts);
            string lastPart = barcodeParts[barcodeParts.Length - 1];

            // 新格式固定 7 段：采购/生产/物料编码/操作员/电镜型号/样品台类型/生产日期+序列号
            const int newFormatSegmentCount = 7;
            if (barcodeParts.Length != newFormatSegmentCount)
            {
                State.CurrentScanDisplay = new ScanDisplayModel();
                SetThresholdDisplayState(false);
                ScanErrorNotice("请使用新二维码格式（共 7 段，含物料编码）。");
                ConfirmCommand.RaiseCanExecuteChanged();
                return;
            }

            State.CurrentScanDisplay = new ScanDisplayModel
            {
                PurchaseOrder = barcodeParts[0],
                ProductionOrder = barcodeParts[1],
                MaterialCode = barcodeParts[2],
                OperatorId = barcodeParts[3],
                ElectronMicroscopeModel = barcodeParts[4],
                StageType = barcodeParts[5],

                ProductionDate = lastPart.Length >= 6 ? lastPart.Substring(0, 6) : lastPart,
                SerialNumber = lastPart.Length > 6 ? lastPart.Substring(6) : string.Empty
            };
            string stageType = barcodeParts[5];
            State.UploadInformation.Content.ElectronMicroscopeModel = State.CurrentScanDisplay.ElectronMicroscopeModel;
            State.UploadInformation.Content.StageType = State.CurrentScanDisplay.StageType;
            State.UploadInformation.Content.SerialNumber = State.CurrentScanDisplay.SerialNumber;
            State.UploadInformation.Content.OperatorId = State.CurrentScanDisplay.OperatorId;
            State.UploadInformation.Content.PurchaseOrder = State.CurrentScanDisplay.PurchaseOrder;
            State.UploadInformation.Content.ProductionOrder = State.CurrentScanDisplay.ProductionOrder;
            State.UploadInformation.Content.ProductionDate = State.CurrentScanDisplay.ProductionDate;
            State.UploadInformation.Content.MaterialCode = State.CurrentScanDisplay.MaterialCode;

            string normalizedStage = stageType.ToUpper();
            switch (normalizedStage)
            {
                case "SAMPLEMINI":
                    State.MotorKindObj = MachineProfile.CompactTwoAxis;
                    break;
                case "SAMPLESTANDARD":
                    State.MotorKindObj = MachineProfile.StandardTwoAxis;
                    break;
                case "SAMPLEPRO":
                    State.MotorKindObj = MachineProfile.HeavyDutyThreeAxis;
                    break;
                case "SAMPLEULTRA":
                    State.MotorKindObj = MachineProfile.UniversalFiveAxis;
                    break;
                default:
                    State.MotorKindObj = MachineProfile.StandardTwoAxis;
                    break;
            }

            LoadThresholdConfigForStage(stageType);
            bool isDateOk = State.CurrentScanDisplay.ProductionDate.Length == 6;
            bool isSnOk = State.CurrentScanDisplay.SerialNumber.Length >= 3;

            if (!isDateOk || !isSnOk)
            {
                SetThresholdDisplayState(false);
                ScanErrorNotice();
                return; // 校验失败直接中断，保护后续逻辑
            }
            SetThresholdDisplayState(true);
            if (State.UploadInformation != null)
            {
                State.UploadInformation.SampleStageId = State.CurrentScanText;
                if (State.UploadInformation.Content != null)
                {
                    State.UploadInformation.Content.StageId = State.CurrentScanText;
                }
                _ = CheckCloudCalibrationAsync(State.UploadInformation.SampleStageId);
            }
            ConfirmCommand.RaiseCanExecuteChanged();
        }


        private void ScanErrorNotice(string? detail = null)
        {
            if (string.IsNullOrWhiteSpace(detail))
                detail = "请检查输入是否是英文";
            App.Current.Dispatcher.Invoke(async () =>
            {
                var errorContent = new StackPanel
                {
                    Margin = new System.Windows.Thickness(16)
                };
                errorContent.Children.Add(new TextBlock
                {
                    Text = "扫码解析失败",
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Red
                });
                errorContent.Children.Add(new TextBlock
                {
                    Text = detail,
                    Margin = new Thickness(0, 10, 0, 10),
                    TextWrapping = TextWrapping.Wrap
                });
                errorContent.Children.Add(new Button
                {
                    Content = "确定",
                    Command = MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand
                });
                await MaterialDesignThemes.Wpf.DialogHost.Show(errorContent, "MultiAxisScanViewHost");
            });
        }
        private async Task CheckCloudCalibrationAsync(string sampleStageId)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                CloudCheckingVisibility = Visibility.Visible;
                CloudResultVisibility = Visibility.Collapsed;
                CloudRecordVisibility = Visibility.Collapsed;
                CloudContentFields.Clear();
                CloudMotors.Clear();
            });

            var (success, hasData, rawJson, errorMsg) = await _thingboardService.QueryCalibrationExistsAsync(sampleStageId);

            App.Current.Dispatcher.Invoke(() =>
            {
                CloudCheckingVisibility = Visibility.Collapsed;
               
                CloudResultVisibility = Visibility.Visible;

                if (!success)
                {
                    CloudResultText = $"⚠ 查询失败：{errorMsg}";
                    CloudResultColor = Brushes.OrangeRed;
                    CloudRecordVisibility = Visibility.Collapsed;
                    CloudRecordSummary = string.Empty;
                }
                else if (hasData)
                {
                    CloudResultText = "✔ 云端已有标定数据，本次将覆盖更新";
                    CloudResultColor = Brushes.Green;

                    // 解析第一条记录的字段
                    try
                    {
                        using var doc = JsonDocument.Parse(rawJson);
                        var first = doc.RootElement[0];
                        CloudRecordDeviceId = first.TryGetProperty("device_id", out var did) && did.ValueKind == JsonValueKind.String
                            ? did.GetString() ?? "-"
                            : "-";
                        string rawTime = first.TryGetProperty("updated_at", out var ut) && ut.ValueKind == JsonValueKind.String
                            ? ut.GetString() ?? "-"
                            : "-";
                        if (DateTime.TryParse(rawTime, null, DateTimeStyles.RoundtripKind, out DateTime dt))
                            CloudRecordUpdatedAt = dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                        else
                            CloudRecordUpdatedAt = rawTime;
                        CloudRecordContentUrl = GetCalibrationDataLink(first);
                        CloudRecordSummary = BuildCalibrationContentSummary(first);
                        PopulateCloudDetailFromRecord(first);
                        CloudRecordVisibility = Visibility.Visible;
                    }
                    catch
                    {
                        CloudRecordDeviceId = "-";
                        CloudRecordUpdatedAt = "-";
                        CloudRecordContentUrl = "-";
                        CloudRecordSummary = string.Empty;
                        CloudRecordVisibility = Visibility.Visible;
                    }

                }
                else
                {
                    CloudResultText = "○ 云端暂无标定数据，本次为首次上传";
                    CloudResultColor = Brushes.Orange;
                    CloudRecordVisibility = Visibility.Collapsed;
                }
            });
        }

        /// <summary>数据链接：优先 content_url；老数据里 content 为字符串时当 URL。</summary>
        private static string GetCalibrationDataLink(JsonElement record)
        {
            if (record.TryGetProperty("content_url", out var urlEl) && urlEl.ValueKind == JsonValueKind.String)
            {
                var s = urlEl.GetString();
                if (!string.IsNullOrWhiteSpace(s))
                    return s;
            }
            if (record.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
                return content.GetString() ?? "-";
            return "-";
        }

        private static bool TryGetJsonString(JsonElement obj, string name, out string value)
        {
            value = string.Empty;
            if (!obj.TryGetProperty(name, out var p))
                return false;
            if (p.ValueKind == JsonValueKind.String)
            {
                value = p.GetString() ?? string.Empty;
                return true;
            }
            if (p.ValueKind == JsonValueKind.Null)
                return false;
            value = p.ToString();
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// 扫码输入归一化：把中文输入法下常见的全角/中文标点转为英文半角。
        /// </summary>
        private void LoadThresholdConfigForStage(string stageType)
        {
            MotorTestThresholdConfigManager.LoadConfig();
            string requestedKey = MotorTestThresholdConfigManager.NormalizeStageKey(stageType);
            var config = MotorTestThresholdConfigManager.ResolveForStage(stageType, out string resolvedKey, out bool usedFallback);

            ThresholdStageTypeDisplay = requestedKey;
            MovementMinDistancePulseText = config.MovementMinDistancePulse.ToString();
            EncoderMinDeltaPulseText = config.EncoderMinDeltaPulse.ToString();
            LimitAccuracyMaxDiffPulseText = config.LimitAccuracyMaxDiffPulse.ToString();
            LinearStdDevMaxUmText = config.LinearStdDevMaxUm.ToString(CultureInfo.InvariantCulture);
            SmoothnessStdDevMaxUmText = config.SmoothnessStdDevMaxUm.ToString(CultureInfo.InvariantCulture);
            (int xMin, int xMax) = GetAxisRangeOrFallback(config, "X");
            (int yMin, int yMax) = GetAxisRangeOrFallback(config, "Y");
            XFullTravelMinPulseText = xMin.ToString();
            XFullTravelMaxPulseText = xMax.ToString();
            YFullTravelMinPulseText = yMin.ToString();
            YFullTravelMaxPulseText = yMax.ToString();

            if (usedFallback)
            {
                ThresholdSaveMessage = $"未找到 {requestedKey} 对应配置，已使用标准20({resolvedKey})";
                ThresholdSaveMessageColor = Brushes.Orange;
            }
            else
            {
                ThresholdSaveMessage = $"已加载 {resolvedKey} 对应测试标准";
                ThresholdSaveMessageColor = Brushes.Gray;
            }
        }

        private void SetThresholdDisplayState(bool hasValidScan)
        {
            ThresholdPanelVisibility = hasValidScan ? Visibility.Visible : Visibility.Collapsed;
            ThresholdPlaceholderVisibility = hasValidScan ? Visibility.Collapsed : Visibility.Visible;
        }

        private static (int min, int max) GetAxisRangeOrFallback(MotorTestThresholdConfigModel config, string axisKey)
        {
            if (config.FullTravelAxisRanges != null
                && config.FullTravelAxisRanges.TryGetValue(axisKey, out var range)
                && range != null
                && range.MinPulse < range.MaxPulse)
            {
                return (range.MinPulse, range.MaxPulse);
            }
            return (config.FullTravelMinPulse, config.FullTravelMaxPulse);
        }

        private void ExecuteSaveThresholdConfig()
        {
            string stageType = State.CurrentScanDisplay?.StageType ?? string.Empty;
            string key = MotorTestThresholdConfigManager.NormalizeStageKey(stageType);

            if (!int.TryParse(MovementMinDistancePulseText, out int movementPulse)
                || !int.TryParse(EncoderMinDeltaPulseText, out int encoderPulse)
                || !int.TryParse(LimitAccuracyMaxDiffPulseText, out int limitPulse)
                || !int.TryParse(XFullTravelMinPulseText, out int xMinPulse)
                || !int.TryParse(XFullTravelMaxPulseText, out int xMaxPulse)
                || !int.TryParse(YFullTravelMinPulseText, out int yMinPulse)
                || !int.TryParse(YFullTravelMaxPulseText, out int yMaxPulse))
            {
                ThresholdSaveMessage = "保存失败：参数格式错误（脉冲需为整数，um 阈值需为数字）";
                ThresholdSaveMessageColor = Brushes.OrangeRed;
                return;
            }

            if (xMinPulse >= xMaxPulse || yMinPulse >= yMaxPulse)
            {
                ThresholdSaveMessage = "保存失败：X/Y轴最小脉冲必须小于最大脉冲";
                ThresholdSaveMessageColor = Brushes.OrangeRed;
                return;
            }

            bool linearParsed = double.TryParse(LinearStdDevMaxUmText, NumberStyles.Float, CultureInfo.InvariantCulture, out double linearUm);
            if (!linearParsed)
            {
                linearParsed = double.TryParse(LinearStdDevMaxUmText, NumberStyles.Float, CultureInfo.CurrentCulture, out linearUm);
            }

            bool smoothnessParsed = double.TryParse(SmoothnessStdDevMaxUmText, NumberStyles.Float, CultureInfo.InvariantCulture, out double smoothnessUm);
            if (!smoothnessParsed)
            {
                smoothnessParsed = double.TryParse(SmoothnessStdDevMaxUmText, NumberStyles.Float, CultureInfo.CurrentCulture, out smoothnessUm);
            }

            if (!linearParsed || !smoothnessParsed)
            {
                ThresholdSaveMessage = "保存失败：参数格式错误（脉冲需为整数，um 阈值需为数字）";
                ThresholdSaveMessageColor = Brushes.OrangeRed;
                return;
            }

            if (linearUm <= 0 || smoothnessUm <= 0)
            {
                ThresholdSaveMessage = "保存失败：um 阈值必须大于 0";
                ThresholdSaveMessageColor = Brushes.OrangeRed;
                return;
            }

            var stageConfig = new MotorTestThresholdConfigModel
            {
                MovementMinDistancePulse = movementPulse,
                EncoderMinDeltaPulse = encoderPulse,
                LimitAccuracyMaxDiffPulse = limitPulse,
                LinearStdDevMaxUm = linearUm,
                SmoothnessStdDevMaxUm = smoothnessUm,
                FullTravelMinPulse = Math.Min(xMinPulse, yMinPulse),
                FullTravelMaxPulse = Math.Max(xMaxPulse, yMaxPulse),
                FullTravelAxisRanges = new Dictionary<string, AxisPulseRange>
                {
                    ["X"] = new AxisPulseRange { MinPulse = xMinPulse, MaxPulse = xMaxPulse },
                    ["Y"] = new AxisPulseRange { MinPulse = yMinPulse, MaxPulse = yMaxPulse }
                }
            };

            MotorTestThresholdConfigManager.LoadConfig();
            MotorTestThresholdConfigManager.SaveForStage(key, stageConfig);

            ThresholdSaveMessage = $"已保存 {key} 测试标准配置文件";
            ThresholdSaveMessageColor = Brushes.Green;
        }

        private static string NormalizeToEnglishInput(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            string normalized = text.Normalize(NormalizationForm.FormKC);
            var sb = new StringBuilder(normalized.Length);

            foreach (char ch in normalized)
            {
                switch (ch)
                {
                    case '，': sb.Append(','); break;
                    case '。': sb.Append('.'); break;
                    case '；': sb.Append(';'); break;
                    case '：': sb.Append(':'); break;
                    case '（': sb.Append('('); break;
                    case '）': sb.Append(')'); break;
                    case '【': sb.Append('['); break;
                    case '】': sb.Append(']'); break;
                    case '｛': sb.Append('{'); break;
                    case '｝': sb.Append('}'); break;
                    case '“':
                    case '”': sb.Append('"'); break;
                    case '‘':
                    case '’': sb.Append('\''); break;
                    case '、':
                    case '／': sb.Append('/'); break;
                    case '－':
                    case '—':
                    case '–': sb.Append('-'); break;
                    case '\u3000': sb.Append(' '); break;
                    default: sb.Append(ch); break;
                }
            }

            return sb.ToString().Trim();
        }
        /// <summary>content 为对象时拼可读摘要。</summary>
        private static string BuildCalibrationContentSummary(JsonElement record)
        {
            if (!record.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Object)
                return string.Empty;
            var lines = new List<string>();
            if (TryGetJsonString(content, "样品台ID", out var sid) && !string.IsNullOrEmpty(sid))
                lines.Add($"样品台ID：{sid}");
            if (TryGetJsonString(content, "开始测试时间", out var t0) && !string.IsNullOrEmpty(t0))
                lines.Add($"开始测试：{t0}");
            if (TryGetJsonString(content, "结束测试时间", out var t1) && !string.IsNullOrEmpty(t1))
                lines.Add($"结束测试：{t1}");
            if (content.TryGetProperty("电机列表", out var motors) && motors.ValueKind == JsonValueKind.Array)
            {
                lines.Add($"电机数：{motors.GetArrayLength()}");
                int i = 0;
                foreach (var m in motors.EnumerateArray())
                {
                    if (i >= 3) break;
                    if (m.ValueKind != JsonValueKind.Object) continue;
                    if (TryGetJsonString(m, "轴类型", out var axis))
                        lines.Add($"  · {axis}");
                    i++;
                }
                if (motors.GetArrayLength() > 3)
                    lines.Add("  · …");
            }
            return lines.Count == 0 ? string.Empty : string.Join(Environment.NewLine, lines);
        }
        private void PopulateCloudDetailFromRecord(JsonElement record)
        {
            CloudContentFields.Clear();
            CloudMotors.Clear();

            // 假设数据结构是最外层数组的第一个元素
            var firstRecord = record.ValueKind == JsonValueKind.Array ? record[0] : record;
            if (!firstRecord.TryGetProperty("content", out var content)) return;

            // 1. 遍历基础字段 (样品台ID, 测试时间等)
            foreach (var prop in content.EnumerateObject())
            {
                if (prop.Name == "电机列表") continue;

                CloudContentFields.Add(new CloudKvDisplayItem
                {
                    Label = prop.Name + "：",
                    Value = prop.Value.ValueKind == JsonValueKind.Null ? "无" : prop.Value.ToString()
                });
            }

            // 2. 遍历电机列表
            // 2. 遍历电机列表
            if (content.TryGetProperty("电机列表", out var motors) && motors.ValueKind == JsonValueKind.Array)
            {
                foreach (var m in motors.EnumerateArray())
                {
                    if (m.ValueKind != JsonValueKind.Object) continue;

                    var row = new CloudMotorAxisDisplay
                    {
                        AxisType = GetJsonString(m, "轴类型"),
                        MinRange = GetJsonScalar(m, "最小量程(um)"),
                        MaxRange = GetJsonScalar(m, "最大量程(um)"),
                        NegLimit = GetJsonScalar(m, "负向限位") == "True" ? "已触发" : "正常",
                        PosLimit = GetJsonScalar(m, "正向限位") == "True" ? "已触发" : "正常",

                        // 恢复这些详细数据
                        ForwardStd = GetJsonScalarAny(m, "正向速度标准差(um)", "正向速度标准差"),
                        ReverseStd = GetJsonScalarAny(m, "反向速度标准差(um)", "反向速度标准差"),
                        PrecisionStd = GetJsonScalarAny(m, "定位精度标准差(um)", "定位精度标准差")
                    };

                    // 恢复解析那几十个甚至上百个的采样点
                    if (m.TryGetProperty("定位精度误差表", out var table) && table.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var pt in table.EnumerateArray())
                        {
                            if (pt.ValueKind != JsonValueKind.Object) continue;
                            row.ErrorPoints.Add(new CloudErrorPointDisplay
                            {
                                TargetUm = GetJsonScalar(pt, "目标位置(um)"),
                                ActualUm = GetJsonScalar(pt, "实际位置(um)")
                            });
                        }
                    }

                    CloudMotors.Add(row);
                }
            }
        }

        private static string JsonScalarToDisplay(JsonElement p)
        {
            return p.ValueKind switch
            {
                JsonValueKind.String => p.GetString() ?? "—",
                JsonValueKind.Number => p.ToString(),
                JsonValueKind.True => "是",
                JsonValueKind.False => "否",
                JsonValueKind.Null => "—",
                _ => p.ToString()
            };
        }

        private static string GetJsonScalar(JsonElement obj, string name)
        {
            return obj.TryGetProperty(name, out var p) ? JsonScalarToDisplay(p) : "—";
        }

        private static string GetJsonScalarAny(JsonElement obj, params string[] names)
        {
            foreach (var name in names)
            {
                if (obj.TryGetProperty(name, out var p))
                    return JsonScalarToDisplay(p);
            }
            return "—";
        }

        private static string GetJsonString(JsonElement obj, string name)
        {
            if (!obj.TryGetProperty(name, out var p) || p.ValueKind != JsonValueKind.String)
                return "—";
            return p.GetString() ?? "—";
        }
        private void Confirm()
        {
            State.InitReport();
            _regionManager.Regions[RegionNames.ContentRegion].RequestNavigate(nameof(Views.MultiAxisRunView));

        }

         
        public bool KeepAlive => true;

        /// <summary>
        /// 从测试页返回时若系统仍是中文输入法，模拟键盘的扫码只会留下数字和 /，英文字段会丢；与「英文输入法下正常」的现象一致。
        /// </summary>
        private static void ApplyEnglishKeyboardForBarcodeScan()
        {
            try
            {
                InputMethod.Current.ImeState = InputMethodState.Off;
                InputLanguageManager.Current.CurrentInputLanguage = new CultureInfo("en-US");
            }
            catch
            {
                // 无 IME 或非 UI 线程时忽略
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            // 立即关闭中文 IME（仅设 InputLanguageManager 为 en-US 往往不够）
            ApplyEnglishKeyboardForBarcodeScan();
            _eventAggregator.GetEvent<BarcodeCaptureEnabledEvent>().Publish(true);
            _token = _eventAggregator.GetEvent<BarcodeScannedEvent>().Subscribe(OnBarcodeReceived);

            // 返回扫码页后，焦点与布局稍晚才稳定，系统可能再次切回中文 IME；延迟再关一次，专防「只有中文时才坏」
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null)
            {
                dispatcher.BeginInvoke(ApplyEnglishKeyboardForBarcodeScan, DispatcherPriority.Input);
                dispatcher.BeginInvoke(ApplyEnglishKeyboardForBarcodeScan, DispatcherPriority.ApplicationIdle);
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            if (_token != null)
            {
                _eventAggregator.GetEvent<BarcodeScannedEvent>().Unsubscribe(_token);

                _token = null;
            }
            _eventAggregator.GetEvent<BarcodeCaptureEnabledEvent>().Publish(false);
           
        }
    }
}
