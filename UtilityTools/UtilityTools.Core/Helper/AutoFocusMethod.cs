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

        // 计算相邻像素相关系数
        private static double CalculateAdjacentPixelCorrelation(Mat grayImage)
        {
            int rows = grayImage.Rows;
            int cols = grayImage.Cols;
            int totalPixels = rows * (cols - 1);

            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0, sumY2 = 0;

            unsafe
            {
                byte* ptr = (byte*)grayImage.Data;
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols - 1; c++)
                    {
                        int idx = r * cols + c;
                        byte x = ptr[idx];       // 当前像素
                        byte y = ptr[idx + 1];   // 右侧相邻像素

                        sumX += x;
                        sumY += y;
                        sumXY += x * y;
                        sumX2 += x * x;
                        sumY2 += y * y;
                    }
                }
            }

            // 计算相关系数
            double numerator = totalPixels * sumXY - sumX * sumY;
            double denominator = Math.Sqrt(
                (totalPixels * sumX2 - sumX * sumX) *
                (totalPixels * sumY2 - sumY * sumY));

            return Math.Abs(denominator > 1e-10 ? numerator / denominator : 0);
        }

        // 计算归一化标准差
        private static double CalculateNormalizedStdDev(Mat grayImage)
        {
            // 计算原始标准差
            Mat mean = new Mat();
            Mat stdDev = new Mat();
            Cv2.MeanStdDev(grayImage, mean, stdDev);
            double rawStdDev = stdDev.At<double>(0);

            // 归一化到0-100范围
            double maxPossibleStdDev = Math.Sqrt(Math.Pow(255, 2) / 12); // 均匀分布标准差
            return (rawStdDev / maxPossibleStdDev) * 100;
        }

        public static double CalculateMinMaxDiff(Mat grayImage) 
        {
            Cv2.MinMaxIdx(grayImage, out double min, out double max);
            return max - min;
        }

        // 可选：频率分析（更精确但较慢），计算高频成分占比
        public static double CalculateHighFrequencyRatio(Mat grayImage)
        {
            // 调整到适合FFT的大小
            Mat padded = new Mat();
            int optWidth = 2 * (grayImage.Cols / 2);
            int optHeight = 2 * (grayImage.Rows / 2);
            Cv2.Resize(grayImage, padded, new OpenCvSharp.Size(optWidth, optHeight));

            // 转换为浮点
            padded.ConvertTo(padded, MatType.CV_32F);

            // 执行FFT
            Mat[] planes = { padded, Mat.Zeros(padded.Size(), MatType.CV_32F) };
            Mat complex = new Mat();
            Cv2.Merge(planes, complex);
            Cv2.Dft(complex, complex);

            // 计算幅度谱
            Cv2.Split(complex, out planes);
            Cv2.Magnitude(planes[0], planes[1], planes[0]);
            Mat spectrum = planes[0];

            // 低频区域（中心区域）
            Rect lowFreqROI = new Rect(
                optWidth / 4,
                optHeight / 4,
                optWidth / 2,
                optHeight / 2);

            // 计算高频能量比例
            double totalEnergy = Cv2.Sum(spectrum)[0];

            // 创建高频掩码：整个频谱区域减去中心低频区域
            Mat mask = new Mat(spectrum.Size(), MatType.CV_32F, Scalar.All(1.0f));
            mask.SubMat(lowFreqROI).SetTo(0.0f);

            // 计算高频能量
            Mat highFreqSpectrum = new Mat();
            Cv2.Multiply(spectrum, mask, highFreqSpectrum);
            double highFreqEnergy = Cv2.Sum(highFreqSpectrum)[0];

            return highFreqEnergy / totalEnergy;
        }

        /// <summary>
        /// 评估图像清晰度
        /// </summary>
        public static double AssessSharpness(Mat grayImage, int kernelSize = 5, double sigma = 1.0)
        {
            // 1. 应用高斯模糊（再模糊）
            Mat reblurred = new Mat();
            Cv2.GaussianBlur(grayImage, reblurred, new OpenCvSharp.Size(kernelSize, kernelSize), sigma);

            // 2. 使用拉普拉斯算子提取边缘
            Mat edgesOriginal = new Mat();
            Mat edgesReblurred = new Mat();
            Cv2.Laplacian(grayImage, edgesOriginal, MatType.CV_64F);
            Cv2.Laplacian(reblurred, edgesReblurred, MatType.CV_64F);

            // 3. 计算边缘差异
            Mat edgeDiff = new Mat();
            Cv2.Absdiff(edgesOriginal, edgesReblurred, edgeDiff);

            // 4. 计算清晰度指标（差异图像的标准差）
            Cv2.MeanStdDev(edgeDiff, out _, out var stdDev);
            double sharpnessIndex = stdDev.ToDouble();

            // 释放资源
            reblurred.Dispose();
            edgesOriginal.Dispose();
            edgesReblurred.Dispose();
            edgeDiff.Dispose();

            return sharpnessIndex;
        }

        /// <summary>
        /// 评估图像噪声水平
        /// </summary>
        public static double AssessNoise(Mat grayImage, int kernelSize = 5, double sigma = 1.0)
        {
            // 1. 应用高斯模糊（再模糊）
            Mat reblurred = new Mat();
            Cv2.GaussianBlur(grayImage, reblurred, new OpenCvSharp.Size(kernelSize, kernelSize), sigma);

            // 2. 计算残差图像（原图与再模糊图的差异）
            Mat residual = new Mat();
            Cv2.Absdiff(grayImage, reblurred, residual);

            // 3. 计算噪声指标（残差图像的标准差）
            Cv2.MeanStdDev(residual, out _, out var stdDev);
            double noiseLevel = stdDev.ToDouble();

            // 4. 可选：计算残差图像的熵值作为辅助指标
            double entropy = CalculateEntropy(residual);

            // 释放资源
            reblurred.Dispose();
            residual.Dispose();

            // 返回综合噪声指标（可以根据需要调整权重）
            return noiseLevel * 0.7 + entropy * 0.3;
        }

    }
}
