using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenCvSharp.Stitcher;
using UtilityTools.Core.Helper;
using Microsoft.Win32;
using Prism.Services.Dialogs;
using System.Windows.Forms;
using System.IO;
using Microsoft.ML.OnnxRuntime;
using OxyPlot;
using System.Windows.Input;
using OpenCvSharp;
using Microsoft.ML.OnnxRuntime.Tensors;
using UtilityTools.Core.Mvvm;
using Prism.Ioc;
using Prism.Commands;

namespace UtilityTools.Modules.ApplicationOfCoarseFocusingModelTool.ViewModels
{
    internal class ApplicationOfCoarseFocusingModelToolViewModel : RegionViewModelBase
    {
        public ApplicationOfCoarseFocusingModelToolViewModel(IContainerProvider containerProvider) : base(containerProvider)
        {
            _containerProvider = containerProvider;
            Init();
        }
        private readonly IContainerProvider _containerProvider;
      
        private readonly string  modelPath = "..\\..\\..\\..\\Modules\\UtilityTools.Modules.ApplicationOfCoarseFocusingModelTool\\CoarseFocusModelResources\\model.onnx";

        private string _targetPictureFolder;
        public string TargetPictureFolder
        {
            get { return _targetPictureFolder; }
            set
            {
                _targetPictureFolder = value;
                RaisePropertyChanged();
            }
        }
        private string _log;
        private string Log
        {
            get { return _log; }
            set
            {
                _log = value;
                RaisePropertyChanged();
            }
        }
        private float _bestObValue;

        

        public float BestObValue
        {
            get { return _bestObValue; }
            set
            {
                _bestObValue = value;
                RaisePropertyChanged();
            }
        }
        private int _predicitionResule;
        public int PredicitionResule
        {
            get { return _predicitionResule;}
            set 
            {
                _predicitionResule = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand ModelPutIntoUseCommand { get; set; }
        private void Init()
        {
            ModelPutIntoUseCommand = new DelegateCommand(ModelPutIntoUse);
        }
        private void x()
        {
            
            PictureHandle(SelectPicturePath());
        }

        private void ModelPutIntoUse()
        {
            var sessionOptions = new SessionOptions();
            var session = new InferenceSession(modelPath, sessionOptions);

            // 获取模型的输入和输出信息
            var inputMetadata = session.InputNames;
            var outputMetadata = session.OutputNames;

            //准备输入
            var intObjobj = ModelInput();

            //加载模型
            var input = intObjobj as List<NamedOnnxValue>;
            if (input == null) 
            {
                return;
            }
            var outputObj = LoadModel(input);
           
            //输出处理
            var output = outputObj as Tensor<float>;
            if (outputObj == null)
            {
                return;
            }
             PredicitionResule = (int)output[0, 0];

        }

        private string SelectPicturePath()
        {
            // 创建一个OpenFileDialog实例
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            // 设置对话框的标题
            openFileDialog.Title = "选择图片文件";

            // 设置文件筛选器，只显示图片文件
            openFileDialog.Filter = "图片文件 (*.jpg;*.jpeg;*.png;*.tiff)|*.jpg;*.jpeg;*.png;*.tiff";

            // 显示对话框并获取用户的选择
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // 获取用户选择的文件路径
                string selectedFilePath = openFileDialog.FileName;

                // 检查文件是否存在
                if (File.Exists(selectedFilePath))
                {
                    return selectedFilePath;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 选择图片所在文件夹返回其中所有图片地址
        /// </summary>
        /// <returns></returns>
        private string[] SelectAllPicturePath()
        {
            // 创建 FolderBrowserDialog 实例
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            // 设置对话框的描述信息
            folderBrowserDialog.Description = "请选择图片存放的文件夹";

            // 显示对话框并获取用户选择
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // 获取用户选择的文件夹路径
                string selectedFolder = folderBrowserDialog.SelectedPath;
                try
                {
                    // 定义图片文件的扩展名
                    string[] imageExtensions = { "*.jpg", "*.jpeg", "*.png", "*.gif", "*.bmp" };

                    // 存储所有图片文件路径
                    string[] allImageFiles = Array.Empty<string>();

                    // 遍历每种扩展名，获取对应图片文件
                    foreach (string extension in imageExtensions)
                    {
                        string[] files = Directory.GetFiles(selectedFolder, extension, SearchOption.AllDirectories);
                        allImageFiles = allImageFiles.Concat(files).ToArray();
                    }
                  

                    return allImageFiles;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"发生错误: {ex.Message}");
                    return null;
                }

            }

            else
            {
                return null;
            }
        }
        /// <summary>
        /// 获取图片对应的文件夹
        /// </summary>
        /// <param name="picturePath"></param>
        /// <returns></returns>
        private string GetFolder(string picturePath)
        {
            if (File.Exists(picturePath))
            {
                return System.IO.Path.GetDirectoryName(picturePath);
            }
            else
            {
                return null;
            }
        }

        private void PictureHandle(string picturePath)
        {
            Mat originalImage = Cv2.ImRead(picturePath, ImreadModes.Unchanged);
            if (originalImage.Empty())
            {
                Log += "cv读取图像错误" + "\n\r";
                return;
            }
            //
            // 归一化图像
            Mat normalizedImage = NormalizeImage(originalImage);

            // 保存归一化后的图像
            Cv2.ImWrite(picturePath, normalizedImage);

            // 释放资源
            originalImage.Dispose();
            normalizedImage.Dispose();

        }
      
       
        private object ModelInput()
        {
           
            string[] picturePath = SelectAllPicturePath();
            if (picturePath == null)
            {
                Log = "选择的文件夹为空";
                return null;
            }
            if (picturePath.Length < 80) 
            {
                Log = "文件夹中的图片数量不足80";
                return null;
            }
            TargetPictureFolder = GetFolder(picturePath[0]);
            /*
            try
            {
                //获取文件夹名称下的最佳物镜值
                BestObValue = float.Parse(TargetPictureFolder.Split('_')[1]);
            }
            catch (Exception e)
            {
                Log = e.ToString + "获取最佳物镜值失败";
                return null ;
            }
            */
            //创建输入张量
            var inputObTensor = new DenseTensor<float>(new[] { 1, 80 });
            var inputMaskTensor = new DenseTensor<float>(new[] { 1, 80});
            var inputPixelChanneValuelTensor = new DenseTensor<float>(new[] { 1, 80,3,128,128 });
            var inputBestObTensor = new DenseTensor<float>(new[] { 1 });
           
            //目标均值和和方差
            double targetMean = 0.5;
            double targetStd = 0.5;
            //像素通道值张量赋值
            for (int i = 0; i < inputPixelChanneValuelTensor.Dimensions[0]; i++)
            {
                for (int j = 0; j < inputPixelChanneValuelTensor.Dimensions[1]; j++)
                {
                    string currentPhotoPath = picturePath[j];
                    Mat image = Cv2.ImRead(currentPhotoPath);
                    //计算图像的均值和标注差
                    for (int k = 0; k < inputPixelChanneValuelTensor.Dimensions[2]; k++)
                    {
                        for (int m = 0; m < inputPixelChanneValuelTensor.Dimensions[3]; m++)
                        {
                            for (int n = 0; n < inputPixelChanneValuelTensor.Dimensions[4]; n++)
                            {
                                Vec3b pixel = image.Get<Vec3b>(m, n);
                                if (k == 0)
                                {

                                    inputPixelChanneValuelTensor[i, j, k, m, n] = (float)((pixel.Item0 / 255.0 -0.5) / 0.5);
                                }
                                else if (k == 1)
                                {
                                    inputPixelChanneValuelTensor[i, j, k, m, n] = (float)((pixel.Item1 / 255.0 - 0.5) /0.5) ;
                                }
                                else if (k == 2)
                                {
                                    inputPixelChanneValuelTensor[i, j, k, m, n] = (float)((pixel.Item2  /255.0 -0.5) / 0.5) ;
                                }            
                            }
                        }
                    }

                }
            }
            //掩码张量赋值
            for (int i = 0; i < inputMaskTensor.Dimensions[0]; i++)
            {
                for (int j = 0; j < inputMaskTensor.Dimensions[1]; j++)
                {
                     inputMaskTensor[i, j] = 1.0f;
                }
            }
            //物镜值张量赋值
            if (picturePath.Length < 80)
            {
                return null;
            }
            for (int i = 0; i < inputObTensor.Dimensions[0]; i++)
            {
                for (int j = 0; j < inputObTensor.Dimensions[1]; j++)
                {
                    try
                    {
                        string pictureName = Path.GetFileName(picturePath[j]);
                        inputObTensor[i, j] = (float.Parse(pictureName.Split('_')[1]));
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
           
            var session = new InferenceSession(modelPath);
            //获取输入名称
            var inputName = session.InputMetadata.Keys;
            var inputName0 = inputName.ElementAt(0);
            var inputName1 = inputName.ElementAt(1);
            var inputName2 = inputName.ElementAt(2);
           


            //创建NamedOnnxValue对象
            var inputPixelChanneOnnxValuel = NamedOnnxValue.CreateFromTensor(inputName0, inputPixelChanneValuelTensor);
            var inputMaskNamedOnnxValue = NamedOnnxValue.CreateFromTensor(inputName1, inputMaskTensor);
            var inputObNamedOnnxValue = NamedOnnxValue.CreateFromTensor(inputName2, inputObTensor);
            


            //准备输入集合
            var inputs = new List<NamedOnnxValue> { inputPixelChanneOnnxValuel,inputObNamedOnnxValue, inputMaskNamedOnnxValue, };
            return inputs;

        }

        private object LoadModel(List<NamedOnnxValue> inputList)
        {
           
            var session = new InferenceSession(modelPath);
            if (inputList == null)
            {
                return null;
            }
            using (var outputs = session.Run(inputList))
            {
                //获取输出
                var outputNames = session.OutputMetadata.Keys;
                var outputResult = outputNames.ElementAt(0);
                var mul240 = outputNames.ElementAt(1);

                //查找输出的NamedOnnxValue对象
                var outputResultValue = outputs.FirstOrDefault(x => x.Name == outputResult);

                //将输出转换为张量
                if (outputResultValue != null)
                {
                    var outputTensor = outputResultValue.AsTensor<float>();
                    return outputTensor;
                }
                else
                {
                    return null;
                }
            }

        }
        /// <summary>
        /// 对图像每个像素值的均值处理
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public  Mat NormalizeImage(Mat src)
        {
            Mat dst = new Mat();
            // 把图像转换为浮点型
            src.ConvertTo(dst, MatType.CV_32F, 1.0 / 255.0);

            double[] meanValues = { 0.5, 0.5, 0.5 };
            double[] stdValues = { 0.5, 0.5, 0.5 };

            Mat[] channels = dst.Split();
            for (int i = 0; i < channels.Length; i++)
            {
                // 减去均值
                channels[i] -= meanValues[i];
                // 除以标准差
                channels[i] /= stdValues[i];
            }

            // 合并通道
            Cv2.Merge(channels, dst);

            // 释放通道资源
            foreach (var channel in channels)
            {
                channel.Dispose();
            }

            return dst;
        }

    }
}
