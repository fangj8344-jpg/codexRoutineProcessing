#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Model
 * 唯一标识：2f3d451f-da0b-4f03-8842-f78b9ff3b8a2
 * 文件名：PieBase
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/3/21 16:10:51
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace UtilityTools.Core.Model
{
    public class PieBase : PieSerise
    {
        /// <summary>
        /// 扇形角度
        /// </summary>
        public double Angle { get; set; }

        /// <summary>
        /// 圆弧
        /// </summary>
        public ArcSegment ArcSegment { get; set; }

        public LineSegment LineSegmentStar { get; set; }

        public LineSegment LineSegmentEnd { get; set; }

        /// <summary>
        /// 圆弧起点
        /// </summary>
        public Point StarPoint { get; set; }

        /// <summary>
        /// 圆弧终点
        /// </summary>
        public Point EndPoint { get; set; }

        /// <summary>
        /// 折线
        /// </summary>
        public Polyline Line { get; set; }

        /// <summary>
        /// 折线终点
        /// </summary>
        public Point PolylineEndPoint { get; set; }

        /// <summary>
        /// 文字
        /// </summary>
        public Path TextPath { get; set; }

    }
}
