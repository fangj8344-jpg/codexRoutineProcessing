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

using NLog;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.ImageAnalyzer.Model;

namespace UtilityTools.Modules.ImageAnalyzer.ViewModels
{
    public class ImageOptViewModel : ViewModelBase
    {
        #region ------------Constructor------------
        public ImageOptViewModel(IContainerProvider containerProvider) : base(containerProvider)
        {

        }
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
        private Zem15MetaData _imgMetaData;
        /// <summary>
        /// 图像元数据
        /// </summary>
        public Zem15MetaData ImgMetaData
        {
            get { return _imgMetaData; }
            set { _imgMetaData = value; RaisePropertyChanged(); }
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

        #endregion

        #region ------------PublicMethod------------
        public void UpdateImage(string imagePath)
        {
            FilePath = imagePath;

            try 
            {
                ImgMetaData = ImageMetadata.ReadMetadata(imagePath);
            }
            catch(Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error($"读取文件（{imagePath}）元数据失败: {ex.Message}");
                return;
            }
        }
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion

    }
}
