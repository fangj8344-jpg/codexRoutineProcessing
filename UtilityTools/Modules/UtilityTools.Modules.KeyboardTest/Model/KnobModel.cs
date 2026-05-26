using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Mvvm;
using System;

namespace UtilityTools.Modules.KeyboardTest.Model
{
    public class KnobModel : BindableBase
    {
        private const int MaxDataPoints = 300;

        public KnobModel(string id, string name, int byteIndex, bool isLarge, int pressByteIndex, int pressBitIndex)
        {
            Id = id;
            Name = name;
            ByteIndex = byteIndex;
            IsLarge = isLarge;
            PressByteIndex = pressByteIndex;
            PressBitIndex = pressBitIndex;
            InitPlotModel();
        }

        public string Id { get; }
        public string Name { get; }
        public int ByteIndex { get; }
        public bool IsLarge { get; }
        public int PressByteIndex { get; }
        public int PressBitIndex { get; }

        private int _accumulatedValue;
        public int AccumulatedValue
        {
            get => _accumulatedValue;
            set => SetProperty(ref _accumulatedValue, value);
        }

        private string _direction = "—";
        public string Direction
        {
            get => _direction;
            set => SetProperty(ref _direction, value);
        }

        private int _rotationCount;
        public int RotationCount
        {
            get => _rotationCount;
            set => SetProperty(ref _rotationCount, value);
        }

        private bool _isPressed;
        public bool IsPressed
        {
            get => _isPressed;
            set => SetProperty(ref _isPressed, value);
        }

        private PlotModel _plotModel;
        public PlotModel PlotModel
        {
            get => _plotModel;
            set => SetProperty(ref _plotModel, value);
        }

        private LineSeries _lineSeries;

        private void InitPlotModel()
        {
            var model = new PlotModel
            {
                PlotMargins = new OxyThickness(36, 4, 8, 20),
                PlotAreaBackground = OxyColor.FromRgb(30, 30, 30),
                Background = OxyColor.FromRgb(40, 40, 40),
            };

            model.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "mm:ss",
                FontSize = 9,
                TextColor = OxyColor.FromRgb(180, 180, 180),
                AxislineColor = OxyColor.FromRgb(80, 80, 80),
                MajorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.FromRgb(60, 60, 60),
                TicklineColor = OxyColor.FromRgb(80, 80, 80),
            });

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                FontSize = 9,
                TextColor = OxyColor.FromRgb(180, 180, 180),
                AxislineColor = OxyColor.FromRgb(80, 80, 80),
                MajorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.FromRgb(60, 60, 60),
                TicklineColor = OxyColor.FromRgb(80, 80, 80),
            });

            _lineSeries = new LineSeries
            {
                Color = OxyColor.FromRgb(0, 200, 120),
                StrokeThickness = 1.5,
                MarkerType = MarkerType.None,
            };

            model.Series.Add(_lineSeries);
            PlotModel = model;
        }

        /// <summary>
        /// 根据HID报文增量值更新(4=正转, 252=反转, 0=静止)
        /// </summary>
        public bool UpdateRotation(byte byteValue)
        {
            if (byteValue == 0) return false;

            int delta;
            if (byteValue == 4)
            {
                delta = 1;
                Direction = "↻ 正转";
            }
            else if (byteValue == 252)
            {
                delta = -1;
                Direction = "↺ 反转";
            }
            else
            {
                return false;
            }

            AccumulatedValue += delta;
            RotationCount++;

            var timeStamp = DateTimeAxis.ToDouble(DateTime.Now);
            _lineSeries.Points.Add(new DataPoint(timeStamp, AccumulatedValue));

            if (_lineSeries.Points.Count > MaxDataPoints)
                _lineSeries.Points.RemoveAt(0);

            PlotModel.InvalidatePlot(true);
            return true;
        }

        /// <summary>
        /// 更新旋钮按下状态
        /// </summary>
        public bool UpdatePress(byte byteValue)
        {
            bool pressed = (byteValue & (1 << PressBitIndex)) == 0;
            if (pressed == IsPressed) return false;
            IsPressed = pressed;
            return true;
        }

        public void Reset()
        {
            AccumulatedValue = 0;
            Direction = "—";
            RotationCount = 0;
            IsPressed = false;
            _lineSeries.Points.Clear();
            PlotModel.InvalidatePlot(true);
        }
    }
}
