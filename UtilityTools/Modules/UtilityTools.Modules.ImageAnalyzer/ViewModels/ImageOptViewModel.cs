#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.ImageAnalyzer.ViewModels
 * 唯一标识：bb076eaa-a05e-40fb-8e98-1fad527f38a0
 * 文件名：ImageOptViewModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/4/24 13:40:42
 * 版本：V1.0.0
 * 描述：
 *
 * ----------------------------------------------------------------
 * 修改人：
 * 时间：
 * 修改说明：
 *
 * 版本：V1.0.1
 *----------------------------------------------------------------*/
#endregion

using Microsoft.Win32;
using NLog;
using OpenCvSharp.WpfExtensions;
using Prism.Commands;
using Prism.Ioc;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Model;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.ImageAnalyzer.Model;
using XmpCore.Impl;

namespace UtilityTools.Modules.ImageAnalyzer.ViewModels
{
    public class ImageOptViewModel : ViewModelBase
    {
        #region ------------Constructor------------
        public ImageOptViewModel(IContainerProvider containerProvider) : base(containerProvider)
        {
            ExecuteCommand = new DelegateCommand<object>(Execute);

            Items = new ObservableCollection<PropertyInfoModel>();

            Items.Add(new PropertyInfoModel(Property_Name, "Name", string.Empty));
            Items.Add(new PropertyInfoModel(Property_AccVol, "HighVol", string.Empty));
            Items.Add(new PropertyInfoModel(Property_Date, "Date", string.Empty));
            Items.Add(new PropertyInfoModel(Property_Detector, "Detecter", string.Empty));
            Items.Add(new PropertyInfoModel(Property_FieldSize, "FieldSize", string.Empty));
            Items.Add(new PropertyInfoModel(Property_Position, "Position", string.Empty));
            Items.Add(new PropertyInfoModel(Property_Zoom, "Mag", string.Empty));
            Items.Add(new PropertyInfoModel(Property_OB, "OB", string.Empty));
            Items.Add(new PropertyInfoModel(Property_AvgLen, "AveragePoints", string.Empty));
            Items.Add(new PropertyInfoModel(Property_PixelLength, "PixelLength", string.Empty));
            Items.Add(new PropertyInfoModel(Property_Pressure, "Pressure", string.Empty));
            Items.Add(new PropertyInfoModel(Property_Freq, "Frequency", string.Empty));
            Items.Add(new PropertyInfoModel(Property_SignificantBit, "DataFlag", string.Empty));
            Items.Add(new PropertyInfoModel(Property_FirmwareVersion, "FirmwareVersion", string.Empty));
            Items.Add(new PropertyInfoModel(Property_SoftwareVersion, "Version", string.Empty));
            Items.Add(new PropertyInfoModel(Property_Title, "Title", string.Empty));
            Items.Add(new PropertyInfoModel(Property_Note, "Note", string.Empty));
        }
        #endregion

        #region ------------Field------------
        private ZemMetaData _zemMetaData = null;

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

        #region ------------Property------------
        private ObservableCollection<PropertyInfoModel> _items;
        /// <summary>
        /// 图像属性列表
        /// </summary>
        public ObservableCollection<PropertyInfoModel> Items
        {
            get { return _items; }
            set { _items = value; RaisePropertyChanged(); }
        }

        private string _filePath;
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; RaisePropertyChanged(); }
        }

        private WriteableBitmap srcBitmap;
        /// <summary>
        /// 原始图像信息
        /// </summary>
        public WriteableBitmap SrcBitmap
        {
            get { return srcBitmap; }
            set { srcBitmap = value; RaisePropertyChanged(); }
        }

        private WriteableBitmap _filterBitmap;
        /// <summary>
        /// 图像处理结果
        /// </summary>
        public WriteableBitmap FilterBitmap
        {
            get { return _filterBitmap; }
            set { _filterBitmap = value; RaisePropertyChanged(); }
        }

        #endregion

        #region ------------PublicMethod------------
        public DelegateCommand<object> ExecuteCommand { get; set; }
        #endregion

        #region ------------PublicMethod------------
        public void UpdateImage(string imagePath)
        {
            FilePath = imagePath;

            // 创建一个新的 BitmapImage 对象
            var tmp = new BitmapImage();

            // 指定图像文件的路径
            tmp.BeginInit();
            tmp.CacheOption = BitmapCacheOption.OnLoad;
            tmp.UriSource = new Uri(imagePath);
            tmp.EndInit();

            SrcBitmap = new WriteableBitmap(tmp);

            _zemMetaData = Model.ImageMetadata.ReadMetadata(imagePath);

            if (_zemMetaData != null && Math.Abs(_zemMetaData.PixelLength * SrcBitmap.PixelWidth - _zemMetaData.FieldSize) > 0.05)
            {
                if (_zemMetaData.PixelLength * SrcBitmap.PixelWidth > _zemMetaData.FieldSize)
                {
                    _zemMetaData.FieldSize = _zemMetaData.PixelLength * SrcBitmap.PixelWidth;
                }
                else
                {
                    _zemMetaData.PixelLength = _zemMetaData.FieldSize / SrcBitmap.PixelWidth;
                }
            }

            UpdatePropertyList(_zemMetaData, Items);
        }

        public void SaveResult()
        {
            Save();
        }
        #endregion

        #region ------------PrivateMethod------------
        /// <summary>
        /// 更新属性列表
        /// </summary>
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
                                item.Value = metaData.Model;
                                break;
                            case Property_AccVol:
                                item.Value = metaData.AccVoltage.ToString();
                                break;
                            case Property_AvgLen:
                                item.Value = metaData.AvgLength.ToString();
                                break;
                            case Property_Date:
                                item.Value = metaData.Date;
                                break;
                            case Property_Detector:
                                item.Value = metaData.Detector;
                                break;
                            case Property_FieldSize:
                                item.Value = metaData.FieldSize.ToString();
                                break;
                            case Property_FirmwareVersion:
                                item.Value = metaData.FirmwareVersion;
                                break;
                            case Property_SoftwareVersion:
                                item.Value = metaData.Version;
                                break;
                            case Property_Position:
                                item.Value = metaData.Position.ToString();
                                break;
                            case Property_Pressure:
                                item.Value = metaData.Pressure;
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
                                item.Value = metaData.Title;
                                break;
                            case Property_Note:
                                item.Value = metaData.Note;
                                break;
                            case Property_Freq:
                                item.Value = metaData.Frequency.ToString();
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 更新元数据
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="list"></param>
        private void UpdateZemMetaData(ZemMetaData metaData, ObservableCollection<PropertyInfoModel> list)
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
                                metaData.Model = item.Value;
                                break;
                            case Property_AccVol:
                                if (int.TryParse(item.Value, out var accVol))
                                {
                                    metaData.AccVoltage = accVol;
                                }
                                break;
                            case Property_AvgLen:
                                if (int.TryParse(item.Value, out var avgLen))
                                {
                                    metaData.AvgLength = avgLen;
                                }
                                break;
                            case Property_Date:
                                metaData.Date = item.Value;
                                break;
                            case Property_Detector:
                                metaData.Detector = item.Value;
                                break;
                            case Property_FieldSize:
                                if (float.TryParse(item.Value, out float fieldSize))
                                {
                                    metaData.FieldSize = fieldSize;
                                }
                                break;
                            case Property_FirmwareVersion:
                                metaData.FirmwareVersion = item.Value;
                                break;
                            case Property_SoftwareVersion:
                                metaData.Version = item.Value;
                                break;
                            case Property_Position:
                                metaData.Position = item.Value;
                                break;
                            case Property_Pressure:
                                metaData.Pressure = item.Value;
                                break;
                            case Property_Zoom:
                                if (int.TryParse(item.Value, out int zoom))
                                {
                                    metaData.Zoom = zoom;
                                }
                                break;
                            case Property_SignificantBit:
                                if (int.TryParse(item.Value, out int flag))
                                {
                                    metaData.SignificantBit = flag;
                                }
                                break;
                            case Property_OB:
                                if (float.TryParse(item.Value, out float ob))
                                {
                                    metaData.Ob = ob;
                                }
                                break;
                            case Property_PixelLength:
                                if (float.TryParse(item.Value, out float pixelLen))
                                {
                                    metaData.PixelLength = pixelLen;
                                }
                                break;
                            case Property_Title:
                                metaData.Title = item.Value;
                                break;
                            case Property_Note:
                                metaData.Note = item.Value;
                                break;
                            case Property_Freq:
                                if (int.TryParse(item.Value, out int freq))
                                {
                                    metaData.Frequency = freq;
                                }
                                break;
                        }
                    }
                }
            }
        }

        private void Execute(object obj)
        {
            var cmd = obj?.ToString();
            if (string.IsNullOrEmpty(cmd))
            {
                return;
            }

            switch (cmd)
            {
                case "Save":
                    Save();
                    break;
                case "Clear":
                    Clear();
                    break;
                case "Silk":
                    AddSilk();
                    break;
                case "Reset":
                    Reset();
                    break;
            }

        }

        private void Save()
        {
            UpdateZemMetaData(_zemMetaData, Items);
            if (FilterBitmap == null)
                return;
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Image File";
            dialog.Filter = "imageFile|*.png";
            dialog.InitialDirectory = FilePath;

            if (dialog.ShowDialog() == true)
            {
                var path = dialog.FileName;
                BitmapEncoder encoder = new PngBitmapEncoder(); // 选择 PNG 格式作为图像编码格式
                BitmapFrame frame = BitmapFrame.Create(FilterBitmap);
                encoder.Frames.Add(frame);
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }

                XmpMethod.SetMetadatas(path, Items);
            }
        }

        private void Clear()
        { 
            FilterBitmap = SrcBitmap.Clone();
        }

        private void AddSilk()
        {
            var source = FilterBitmap == null ? SrcBitmap : FilterBitmap;
            UpdateZemMetaData(_zemMetaData, Items);
            var bitmap = TransformMethod.WriteableBitmapToBitmap(source);
            var result = GraphSilk(bitmap, _zemMetaData);
            FilterBitmap = TransformMethod.ConvertBitmapToWriteableBitmap(result);
        }

        private void Reset()
        {
            if (SrcBitmap != null)
            {
                var source = FilterBitmap == null ? SrcBitmap : FilterBitmap;
                var mat = source.ToMat();
                mat.Normalize();
                FilterBitmap = mat.ToWriteableBitmap();
            }

        }

        private Bitmap GraphSilk(Bitmap src, ZemMetaData metaData)
        {
            if (src == null)
                return null;

            // 和图像尺寸有关的变量
            int itemWidth = src.Width / 4;
            int silkHeight = 84;
            int bitFontSize = 30;
            int smallFontSize = 26;
            int fontSize = 2;
            int lineSize = 5;
            int margin = 10;

            if (src.Width == 512)
            {
                silkHeight = 24;
                bitFontSize = 10;
                smallFontSize = 8;
                fontSize = 1;
                lineSize = 2;
                margin = 3;
            }
            else if (src.Width == 1024)
            {
                silkHeight = 44;
                bitFontSize = 20;
                smallFontSize = 16;
                fontSize = 2;
                lineSize = 4;
                margin = 6;
            }
            else if (src.Width == 4096)
            {
                silkHeight = 164;
                bitFontSize = 60;
                smallFontSize = 46;
                fontSize = 5;
                lineSize = 8;
                margin = 20;
            }

            // 计算标尺信息
            double staffSize = 0.0;
            double staffWidth = 0.0;
            int staffLineMaxLen = itemWidth - 2 * margin;
            if (metaData.PixelLength == 0.0)
            {
                staffSize = 100;
                staffWidth = staffLineMaxLen;
            }
            else
            {
                double maxStaffSize = staffLineMaxLen * metaData.PixelLength;
                {
                    if (maxStaffSize > 1000)
                    {
                        staffSize = 1000;
                        staffWidth = staffSize / metaData.PixelLength;
                    }
                    else if (maxStaffSize > 500)
                    {
                        staffSize = 500;
                        staffWidth = staffSize / metaData.PixelLength;
                    }
                    else if (maxStaffSize > 200)
                    {
                        staffSize = 200;
                        staffWidth = staffSize / metaData.PixelLength;
                    }
                    else if (maxStaffSize > 100)
                    {
                        staffSize = 100;
                        staffWidth = staffSize / metaData.PixelLength;
                    }
                    else if (maxStaffSize > 50)
                    {
                        staffSize = 50;
                        staffWidth = staffSize / metaData.PixelLength;
                    }
                    else if (maxStaffSize > 20)
                    {
                        staffSize = 20;
                        staffWidth = staffSize / metaData.PixelLength;
                    }
                    else if (maxStaffSize > 10)
                    {
                        staffSize = 10;
                        staffWidth = staffSize / metaData.PixelLength;
                    }
                    else if (maxStaffSize > 5)
                    {
                        staffSize = 5;
                        staffWidth = staffSize / metaData.PixelLength;
                    }
                    else if (maxStaffSize > 2)
                    {
                        staffSize = 2;
                        staffWidth = staffSize / metaData.PixelLength;
                    }
                    else if (maxStaffSize > 1)
                    {
                        staffSize = 1;
                        staffWidth = staffSize / metaData.PixelLength;
                    }
                    else if (maxStaffSize > 0.5)
                    {
                        staffSize = 0.5;
                        staffWidth = staffSize / metaData.PixelLength;
                    }
                    else if (maxStaffSize > 0.2)
                    {
                        staffSize = 0.2;
                        staffWidth = staffSize / metaData.PixelLength;
                    }
                    else if (maxStaffSize > 0.1)
                    {
                        staffSize = 0.1;
                        staffWidth = staffSize / metaData.PixelLength;
                    }
                    else
                    {
                        staffSize = maxStaffSize;
                        staffWidth = staffSize / metaData.PixelLength;
                    }
                }
            }

            // 计算绘图相关数据
            int posY = src.Height - silkHeight;
            var sourceRect = new Rectangle(0, 0, src.Width, posY);
            var staffStrRect = new Rectangle(0, posY + margin, itemWidth, silkHeight - margin * 3);
            var magStrRect = new Rectangle(itemWidth, posY, itemWidth, silkHeight / 2);
            var hvStrRect = new Rectangle(itemWidth, posY + silkHeight / 2, itemWidth, silkHeight / 2);
            var obStrRect = new Rectangle(itemWidth * 2, posY, itemWidth, silkHeight / 2);
            var detStrRect = new Rectangle(itemWidth * 2, posY + silkHeight / 2, itemWidth, silkHeight / 2);
            var machStrRect = new Rectangle(itemWidth * 3, posY, itemWidth, silkHeight / 2);
            var timeStrRect = new Rectangle(itemWidth * 3, posY + silkHeight / 2, itemWidth, silkHeight / 2);
            Font bigFont = new Font("宋体", bitFontSize, System.Drawing.FontStyle.Bold);
            Font smallFont = new Font("宋体", smallFontSize, System.Drawing.FontStyle.Bold);
            StringFormat centerSf = new StringFormat();
            centerSf.Alignment = StringAlignment.Center;
            StringFormat rightSf = new StringFormat();
            rightSf.Alignment = StringAlignment.Far;
            StringFormat leftSf = new StringFormat();
            leftSf.Alignment = StringAlignment.Near;

            var g = Graphics.FromImage(src);

            // 添加底部丝印
            var pen = new Pen(Color.White, fontSize);
            var linePen = new Pen(Color.White, lineSize);
            var foreBrush = new SolidBrush(Color.White);
            var backBrush = new SolidBrush(Color.Black);
            System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo("zh-CN");
            // 填充背景
            g.FillRectangle(backBrush, new Rectangle(0, posY, src.Width, silkHeight));
            // 绘制标尺尺寸
            g.DrawString($"{staffSize}μm", bigFont, foreBrush, staffStrRect, centerSf);
            // 绘制标尺刻度线
            g.DrawLine(linePen, new PointF((float)(staffStrRect.Width / 2 - staffWidth / 2), staffStrRect.Bottom),
                new PointF((float)(staffStrRect.Width / 2 + staffWidth / 2), staffStrRect.Bottom));
            g.DrawLine(linePen, new PointF((float)(staffStrRect.Width / 2 - staffWidth / 2), staffStrRect.Bottom - margin),
                new PointF((float)(staffStrRect.Width / 2 - staffWidth / 2), staffStrRect.Bottom + margin / 2));
            g.DrawLine(linePen, new PointF((float)(staffStrRect.Width / 2 + staffWidth / 2), staffStrRect.Bottom - margin),
                new PointF((float)(staffStrRect.Width / 2 + staffWidth / 2), staffStrRect.Bottom + margin / 2));
            // 绘制放大倍数
            g.DrawString($"X{metaData.Zoom}", smallFont, foreBrush, magStrRect, centerSf);
            // 绘制电压
            g.DrawString($"{metaData.AccVoltage}kV", smallFont, foreBrush, hvStrRect, centerSf);
            // 绘制物镜
            g.DrawString($"{metaData.Ob:F1}mm", smallFont, foreBrush, obStrRect, centerSf);
            // 绘制检测器
            g.DrawString($"{metaData.Detector}", smallFont, foreBrush, detStrRect, centerSf);
            // 绘制机器型号
            g.DrawString($"{metaData.Model}", smallFont, foreBrush, machStrRect, rightSf);
            // 绘制时间信息
            g.DrawString($"{metaData.Date}", smallFont, foreBrush, timeStrRect, rightSf);
            g.Dispose();
            foreBrush.Dispose();
            backBrush.Dispose();
            pen.Dispose();

            return src;
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion

    }
}
