#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2025   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.AutoFocusTest.Model
 * 唯一标识：2e2a6871-2952-4331-a338-289a458558e2
 * 文件名：ImgListBindData
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2025/5/6 9:45:09
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
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Prism.Mvvm;

namespace UtilityTools.Modules.AutoFocusTest.Model
{
    public class ImgListBindData : BindableBase
    {
        #region Property
        private string _fileName;
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; RaisePropertyChanged(); }
        }

        private int _obValue;
        /// <summary>
        /// 物镜值
        /// </summary>
        public int ObValue
        {
            get { return _obValue; }
            set { _obValue = value; RaisePropertyChanged(); }
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

        private int _imgWidth;
        /// <summary>
        /// 图像宽度
        /// </summary>
        public int ImgWidth
        {
            get { return _imgWidth; }
            set { _imgWidth = value; RaisePropertyChanged(); }
        }

        private int _imgHeight;
        /// <summary>
        /// 图像高度
        /// </summary>
        public int ImgHeight
        {
            get { return _imgHeight; }
            set { _imgHeight = value; RaisePropertyChanged(); }
        }

        private BitmapImage _picBitmapImage;
        /// <summary>
        /// 图像位图
        /// </summary>
        public BitmapImage PicBitmapImage
        {
            get { return _picBitmapImage; }
            set { _picBitmapImage = value; RaisePropertyChanged(); }
        }

        private Stretch _stretchMethod;
        /// <summary>
        /// 拉伸方法
        /// </summary>
        public Stretch StretchMethod
        {
            get { return _stretchMethod; }
            set { _stretchMethod = value; RaisePropertyChanged(); }
        }

        #endregion
    }
}
