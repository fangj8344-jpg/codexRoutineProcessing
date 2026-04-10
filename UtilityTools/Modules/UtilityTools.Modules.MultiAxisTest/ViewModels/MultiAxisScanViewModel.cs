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
using UtilityTools.Core;
using UtilityTools.Core.Event;
using UtilityTools.Core.Helper;
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

        public MultiAxisScanViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, MultiAxisWorkflowState state, IContainerProvider containerProvider )
            : base(containerProvider)
        {
            _regionManager = regionManager;
            State = state;
            _eventAggregator = eventAggregator;
            ConfirmCommand = new DelegateCommand(Confirm, CanConfirm);
            InputLanguageManager.Current.CurrentInputLanguage = new CultureInfo("en-US");

            _thingboardService = containerProvider.Resolve<IServiceFactory>().GetThingboardService();
            OpenEngineerConfigCommand = new DelegateCommand(ExecuteOpenEngineerConfig);
            // 兜底：扫码页VM创建后先打开一次扫码捕获，避免首帧导航时机差导致失效
            _eventAggregator.GetEvent<BarcodeCaptureEnabledEvent>().Publish(true);
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
            State.CurrentScanDisplay = new ScanDisplayModel
            {
                // 前三个固定不变
                PurchaseOrder = barcodeParts[0],
                ProductionOrder = barcodeParts[1],
                OperatorId = barcodeParts[2],

                // 日期固定取前 6 位，取不到 6 位有多少取多少
                ProductionDate = lastPart.Length >= 6 ? lastPart.Substring(0, 6) : lastPart,

                // 序列号取 6 位之后的所有内容
                SerialNumber = lastPart.Length > 6 ? lastPart.Substring(6) : string.Empty
            };
            if (barcodeParts.Length >= 6)
            {
                State.CurrentScanDisplay.ElectronMicroscopeModel = barcodeParts[3]; // 比如 ZEM20
                string stageType = barcodeParts[4];                                 // 比如 SampleMini
                State.CurrentScanDisplay.StageType = stageType;
                //给上传的信息赋值
                State.UploadInformation.Content.ElectronMicroscopeModel = State.CurrentScanDisplay.ElectronMicroscopeModel;
                State.UploadInformation.Content.StageType = State.CurrentScanDisplay.StageType;
                State.UploadInformation.Content.SerialNumber = State.CurrentScanDisplay.SerialNumber;
                State.UploadInformation.Content.OperatorId = State.CurrentScanDisplay.OperatorId;
                State.UploadInformation.Content.PurchaseOrder = State.CurrentScanDisplay.PurchaseOrder;
                State.UploadInformation.Content.ProductionOrder = State.CurrentScanDisplay.ProductionOrder;
                State.UploadInformation.Content.ProductionDate = State.CurrentScanDisplay.ProductionDate;
                // 如果你的 State.UploadInformation 里也有这两个字段，也可以在这里一并赋值：
                // State.UploadInformation.ElectronMicroscopeModel = barcodeParts[3];
                // State.UploadInformation.StageType = barcodeParts[4];
                // 🌟 核心映射：识别样品台，装箱存入全局状态
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
                        // 碰到不认识的标准型号兜底
                        State.MotorKindObj = MachineProfile.StandardTwoAxis;
                        break;
                }


            }
            else
            {
                // 如果是旧码（只有4段），赋个空值或者默认值，防止空引用
                State.CurrentScanDisplay.ElectronMicroscopeModel = string.Empty;
                State.CurrentScanDisplay.StageType = string.Empty;
            }
            bool isDateOk = State.CurrentScanDisplay.ProductionDate.Length == 6;
            bool isSnOk = State.CurrentScanDisplay.SerialNumber.Length >= 3;

            if (!isDateOk || !isSnOk)
            {
                ScanErrorNotice();
                return; // 校验失败直接中断，保护后续逻辑
            }
            if (State.UploadInformation != null)
            {
                State.UploadInformation.SampleStageId = State.CurrentScanText;
                if (State.UploadInformation.Content != null)
                {
                    State.UploadInformation.Content.StageId = State.CurrentScanText;
                }
                if (barcodeParts.Length >= 6)
                {
                    _ = CheckCloudCalibrationAsync(State.UploadInformation.SampleStageId);
                }

            }
            ConfirmCommand.RaiseCanExecuteChanged();
        }


        private void ScanErrorNotice() 
        {
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
                    Text = $"请检查输入是否是英文",
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

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            // 将当前输入法的语言强制切换为美式英文
            InputLanguageManager.Current.CurrentInputLanguage = new CultureInfo("en-US");
            _eventAggregator.GetEvent<BarcodeCaptureEnabledEvent>().Publish(true);
            _token = _eventAggregator.GetEvent<BarcodeScannedEvent>().Subscribe(OnBarcodeReceived);
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
