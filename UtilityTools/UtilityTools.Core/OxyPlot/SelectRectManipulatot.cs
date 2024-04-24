#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.OxyPlot
 * 唯一标识：51cc986f-f921-4232-8164-984c3a1b6150
 * 文件名：SelectRectManipulatot
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/4/22 16:09:09
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

using OxyPlot;
using OxyPlot.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Annotations;

namespace UtilityTools.Core.OxyPlot
{
    public class SelectRectManipulatot : MouseManipulator
    {
        #region ------------Constructor------------
        public SelectRectManipulatot(IPlotView plotView)
            : base(plotView)
        {
            _rectAnnotations = new List<RectangleAnnotation>();
        }
        #endregion

        #region ------------Field------------
        /// <summary>
        /// Multiple select or not, control by pressed Ctrl key
        /// </summary>
        private bool _isMultipleSelect;

        /// <summary>
        /// 选中区域蒙层
        /// </summary>
        private RectangleAnnotation _rectAnnotation;

        /// <summary>
        /// 所有选中区域蒙层
        /// </summary>
        private IList<RectangleAnnotation> _rectAnnotations;
        #endregion

        #region ------------Property------------
        #endregion

        #region ------------PublicMethod------------
        /// <summary>
        /// Occurs when a manipulation is complete.
        /// </summary>
        /// <param name="e">The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.</param>
        public override void Completed(OxyMouseEventArgs e)
        {
            base.Completed(e);

            this.PlotView.SetCursorType(CursorType.Default);
            ShowSelectedRects();

            e.Handled = true;
        }

        /// <summary>
        /// Occurs when the input device changes position during a manipulation.
        /// </summary>
        /// <param name="e">The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.</param>
        public override void Delta(OxyMouseEventArgs e)
        {
            base.Delta(e);

            if (this.XAxis == null)
            {
                return;
            }

            var plotArea = this.PlotView.ActualModel.PlotArea;

            var x = Math.Min(this.StartPosition.X, e.Position.X);
            var w = Math.Abs(this.StartPosition.X - e.Position.X);
            var y = Math.Min(this.StartPosition.Y, e.Position.Y);
            var h = Math.Abs(this.StartPosition.Y - e.Position.Y);



            ShowSelectedRects();

            e.Handled = true;
        }

        /// <summary>
        /// Occurs when an input device begins a manipulation on the plot.
        /// </summary>
        /// <param name="e">The <see cref="OxyPlot.OxyMouseEventArgs" /> instance containing the event data.</param>
        public override void Started(OxyMouseEventArgs e)
        {
            base.Started(e);

            _isMultipleSelect = (e.ModifierKeys == OxyModifierKeys.Control && this.XAxis != null);

            if(!this._isMultipleSelect) 
            {
                HideSelectedRects();
            }

            // 创建选择区域的注释
            _rectAnnotation = new RectangleAnnotation
            {
                Fill = OxyColor.FromArgb(50, 255, 0, 0), // 设置选中区域的填充颜色
                Stroke = OxyColors.Transparent
            };

            this.PlotView.SetCursorType(this.GetCursorType());

            e.Handled = true;
        }

        #endregion

        #region ------------PrivateMethod------------
        /// <summary>
        /// Gets the cursor for the manipulation.
        /// </summary>
        /// <returns>The cursor.</returns>
        private CursorType GetCursorType()
        {
            if (this.XAxis == null)
            {
                return CursorType.ZoomVertical;
            }

            if (this.YAxis == null)
            {
                return CursorType.ZoomHorizontal;
            }

            return CursorType.ZoomRectangle;
        }

        private void ShowSelectedRects()
        {
            var model = this.View.ActualModel as PlotModel;
            if (model == null)
                return;

            foreach (var annotation in _rectAnnotations)
            {
                if (!model.Annotations.Contains(annotation))
                    model.Annotations.Add(annotation);
            }

            // 刷新绘图
            model.InvalidatePlot(false);
        }

        private void HideSelectedRects()
        {
            var model = this.View.ActualModel as PlotModel;
            if (model == null)
                return;

            foreach (var annotation in _rectAnnotations)
            {
                if(model.Annotations.Contains(annotation))
                    model.Annotations.Remove(annotation);
            }

            // 刷新绘图
            model.InvalidatePlot(false);
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
