using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing;
using OpenCvSharp;
using System.Windows;
using System.IO;
using OpenCvSharp.Extensions;
using System.Collections.Generic;
using OpenCvSharp.Features2D;

namespace UtilityTools.Modules.ImageComparator.ViewModels
{
    public class ImageComparatorViewModel : BindableBase
    {
        private BitmapSource _image1;
        private BitmapSource _image2;
        private BitmapSource _resultImage;
        private string _statusMessage;
        private double _offsetX;
        private double _offsetY;

        public BitmapSource Image1
        {
            get => _image1;
            set
            { 
                SetProperty(ref _image1, value);
                CanCompare = Image1 != null && Image2 != null;
            }
        }

        public BitmapSource Image2
        {
            get => _image2;
            set
            {
                SetProperty(ref _image2, value);
                CanCompare = Image1 != null && Image2 != null;
            }
        }

        private bool _canCompare;

        public bool CanCompare
        {
            get { return _canCompare; }
            set { _canCompare = value; RaisePropertyChanged(); }
        }

        public BitmapSource ResultImage
        {
            get => _resultImage;
            set => SetProperty(ref _resultImage, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public double OffsetX
        {
            get => _offsetX;
            set => SetProperty(ref _offsetX, value);
        }

        public double OffsetY
        {
            get => _offsetY;
            set => SetProperty(ref _offsetY, value);
        }

        public ICommand SelectImage1Command { get; }
        public ICommand SelectImage2Command { get; }
        public ICommand CompareImagesCommand { get; }

        public ImageComparatorViewModel()
        {
            SelectImage1Command = new DelegateCommand(SelectImage1);
            SelectImage2Command = new DelegateCommand(SelectImage2);
            CompareImagesCommand = new DelegateCommand(CompareImages);
        }

        private void SelectImage1()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                Image1 = LoadImage(openFileDialog.FileName);
            }
        }

        private void SelectImage2()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                Image2 = LoadImage(openFileDialog.FileName);
            }
        }

        private BitmapSource LoadImage(string filePath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(filePath);
            bitmap.EndInit();
            return bitmap;
        }

        private void CompareImages()
        {
            try
            {
                // Convert BitmapSource to Bitmap
                var bitmap1 = BitmapSourceToBitmap(Image1);
                var bitmap2 = BitmapSourceToBitmap(Image2);

                // Convert to OpenCV format
                using var mat1 = OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap1);
                using var mat2 = OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap2);

                // Convert to grayscale for feature detection
                using var gray1 = mat1.CvtColor(ColorConversionCodes.BGR2GRAY);
                using var gray2 = mat2.CvtColor(ColorConversionCodes.BGR2GRAY);

                // Initialize SIFT detector
                using var sift = SIFT.Create();

                // Detect keypoints and compute descriptors
                KeyPoint[] keypoints1, keypoints2;
                using var descriptors1 = new Mat();
                using var descriptors2 = new Mat();
                sift.DetectAndCompute(gray1, null, out keypoints1, descriptors1);
                sift.DetectAndCompute(gray2, null, out keypoints2, descriptors2);

                if (keypoints1.Length == 0 || keypoints2.Length == 0)
                {
                    StatusMessage = "No features detected in one or both images.";
                    ResultImage = null;
                    return;
                }

                // Match descriptors using FLANN matcher
                using var matcher = new FlannBasedMatcher();
                var matches = matcher.KnnMatch(descriptors1, descriptors2, 2);

                // Apply ratio test to find good matches
                var goodMatches = new List<DMatch>();
                foreach (var match in matches)
                {
                    if (match[0].Distance < 0.75 * match[1].Distance)
                    {
                        goodMatches.Add(match[0]);
                    }
                }

                if (goodMatches.Count < 4)
                {
                    StatusMessage = "Not enough good matches found between images.";
                    ResultImage = null;
                    return;
                }

                // Get matched keypoints
                var matchedPoints1 = new List<Point2f>();
                var matchedPoints2 = new List<Point2f>();
                foreach (var match in goodMatches)
                {
                    matchedPoints1.Add(keypoints1[match.QueryIdx].Pt);
                    matchedPoints2.Add(keypoints2[match.TrainIdx].Pt);
                }

                // Convert lists to arrays for FindHomography
                var points1 = matchedPoints1.ToArray();
                var points2 = matchedPoints2.ToArray();

                // Find homography matrix
                using var homography = Cv2.FindHomography(InputArray.Create(points1), InputArray.Create(points2), HomographyMethods.Ransac, 3.0);

                if (homography == null)
                {
                    StatusMessage = "Could not find homography between images.";
                    ResultImage = null;
                    return;
                }

                // Calculate average offset from matched points
                double totalOffsetX = 0;
                double totalOffsetY = 0;
                foreach (var match in goodMatches)
                {
                    var pt1 = keypoints1[match.QueryIdx].Pt;
                    var pt2 = keypoints2[match.TrainIdx].Pt;
                    totalOffsetX += pt2.X - pt1.X;
                    totalOffsetY += pt2.Y - pt1.Y;
                }

                OffsetX = totalOffsetX / goodMatches.Count;
                OffsetY = totalOffsetY / goodMatches.Count;

                // Create result image with matched features
                using var resultImg = mat1.Clone();
                foreach (var match in goodMatches)
                {
                    var pt1 = keypoints1[match.QueryIdx].Pt;
                    var pt2 = keypoints2[match.TrainIdx].Pt;
                    Cv2.Circle(resultImg, (int)pt1.X, (int)pt1.Y, 3, Scalar.Red, -1);
                    Cv2.Line(resultImg, (int)pt1.X, (int)pt1.Y, (int)pt2.X, (int)pt2.Y, Scalar.Green, 1);
                }

                // Convert back to BitmapSource
                ResultImage = ConvertToBitmapSource(resultImg.ToBitmap());
                StatusMessage = $"Found {goodMatches.Count} matching features. Average offset: X={OffsetX:F2}, Y={OffsetY:F2}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error comparing images: {ex.Message}";
            }
        }

        private Bitmap BitmapSourceToBitmap(BitmapSource source)
        {
            var bitmap = new Bitmap(
                source.PixelWidth,
                source.PixelHeight,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            var data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            source.CopyPixels(
                Int32Rect.Empty,
                data.Scan0,
                data.Height * data.Stride,
                data.Stride);

            bitmap.UnlockBits(data);
            return bitmap;
        }

        private BitmapSource ConvertToBitmapSource(Bitmap bitmap)
        {
            var handle = bitmap.GetHbitmap();
            try
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    handle,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(handle);
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr handle);
    }
}