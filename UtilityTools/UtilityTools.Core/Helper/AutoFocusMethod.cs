#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2025   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Helper
 * 唯一标识：ef8b1025-4430-4ad7-8c88-aaaa1904480e
 * 文件名：AutoFocusMethod
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2025/5/6 10:19:44
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

using OpenCvSharp;
using OpenCvSharp.XImgProc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Helper
{
    public class AutoFocusMethod
    {
        public static double CalculateTenengrad(Mat gray) 
        {
            // Sobel算子计算梯度
            Mat gx = new Mat(), gy = new Mat();
            Cv2.Sobel(gray, gx, MatType.CV_64F, 1, 0, 3);
            Cv2.Sobel(gray, gy, MatType.CV_64F, 0, 1, 3);

            // 计算梯度幅值平方和
            Mat magnitude = new Mat();
            Cv2.Magnitude(gx, gy, magnitude);
            Cv2.Pow(magnitude, 2, magnitude);

            // 取平均值作为评分
            Scalar meanValue = Cv2.Mean(magnitude);
            return meanValue.Val0;
        }

        public static double Laplacian(Mat gray)
        {
            Mat laplacian = new Mat();
            Cv2.Laplacian(gray, laplacian, MatType.CV_64F);

            // 计算方差
            Mat mean = new Mat(), stddev = new Mat();
            Cv2.MeanStdDev(laplacian, mean, stddev);
            return stddev.At<double>(0) * stddev.At<double>(0);
        }

        public static double CalculateBrenner(Mat gray) 
        {
            double total = 0;

            // 计算相邻像素差平方和
            for (int y = 0; y < gray.Height; y++)
            {
                for (int x = 0; x < gray.Width - 2; x++)
                {
                    double diff = gray.At<byte>(y, x + 2) - gray.At<byte>(y, x);
                    total += diff * diff;
                }
            }
            return total / (gray.Width * gray.Height);
        }

        public static double MultiScaleSharpness(Mat gray, int levels = 3)
        {
            double totalScore = 0;
            Mat current = gray.Clone();

            for (int i = 0; i < levels; i++)
            {
                // 计算当前尺度分数
                totalScore += CalculateTenengrad(current) * Math.Pow(0.5, i);

                // 下采样
                Mat downsampled = new Mat();
                Cv2.PyrDown(current, downsampled);
                current = downsampled;
            }

            return totalScore;
        }

        public static double ComputeNoise(Mat gray)
        {
            // 计算标准差
            Scalar mean, stddev;
            Cv2.MeanStdDev(gray, out mean, out stddev);
            return stddev.Val0;
        }

        public static double EdgeDensity(Mat gray)
        {
            // 使用Canny边缘检测
            Mat edges = new Mat();
            Cv2.Canny(gray, edges, 100, 200);

            // 计算边缘像素占比
            int edgePixels = Cv2.CountNonZero(edges); // 获取边缘像素数
            int totalPixels = edges.Rows * edges.Cols;  // 图像总像素数
            return (double)edgePixels / totalPixels;    // 返回边缘占比
        }

        public static double SpectralComplexity(Mat gray)
        {
            // 加载图像
            Mat image = gray;

            // 执行傅里叶变换
            Mat fTransform = new Mat();
            Mat[] planes = new Mat[2] { new Mat(), new Mat() };
            Cv2.Split(image, out planes);
            Cv2.Merge(planes, fTransform);

            // 计算频谱的能量
            Mat magnitude = new Mat();
            Cv2.Magnitude(planes[0], planes[1], magnitude);
            magnitude += new Scalar(1);  // 防止log(0)
            Mat logMagnitude = new Mat();
            Cv2.Log(magnitude, logMagnitude);

            // 计算频谱能量
            double spectrumEnergy = Cv2.Sum(logMagnitude)[0];
            return spectrumEnergy / (image.Rows * image.Cols);
        }

        public static double CalculateEntropy(Mat grayMat)
        {
            // 计算直方图
            Mat hist = new Mat();
            int[] channels = { 0 }; // 只处理灰度通道
            int histSize = 0xFFF; // 256级灰度
            Rangef[] ranges = { new Rangef(0, 0xFFF) };
            Cv2.CalcHist(new Mat[] { grayMat }, channels, null, hist, 1, new int[] { histSize }, ranges);

            // 计算图像的熵
            double entropy = 0;
            for (int i = 0; i < hist.Rows; i++)
            {
                double prob = hist.At<float>(i) / grayMat.Total(); // 归一化
                if (prob > 0)
                    entropy -= prob * Math.Log(prob, 2); // 熵公式
            }
            return entropy;
        }
    }
}
