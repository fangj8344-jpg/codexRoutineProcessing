using MetadataExtractor;
using MetadataExtractor.Formats.Png;
using Newtonsoft.Json;
using OpenCvSharp;
using OpenCvSharp.Dnn;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using UtilityTools.Modules.ImageAnalyzer.Model;

namespace UtilityTools.Modules.ImageAnalyzer.Tests
{

    public class TestPngMetadata
    {
        string imagePath = @"E:\repos\tribf\meta-learn\jobs\2023_tribf_at_zeptools\proj\202310_zem_utils\data\x202310_02_image_analyzer\设备保存PNG文件格式查看\21-07-19-09-49-51.6801-1.png";
        string imagePath2 = @"E:\repos\tribf\meta-learn\jobs\2023_tribf_at_zeptools\proj\202310_zem_utils\data\x202310_02_image_analyzer\csharp_save_demo.png";

        string imagePath3 = @"E:\repos\tribf\meta-learn\jobs\2023_tribf_at_zeptools\proj\202310_zem_utils\data\x202310_02_image_analyzer\SEM20设备保存的PNG文件\20231023093351.png";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestOpnecv()
        {
            Mat img = Cv2.ImRead(imagePath, ImreadModes.Unchanged);
            Cv2.ImWrite(imagePath2, img);
        }

        [Test]
        public void TestPngMeta()
        {
            var directories = ImageMetadataReader.ReadMetadata(imagePath);

            var m = PngMetadataReader.ReadMetadata(imagePath2);

            {
                var d3 = ImageMetadataReader.ReadMetadata(imagePath3);

            }


            using (Stream stream = File.Open(imagePath3, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                BitmapDecoder decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                BitmapFrame bitmapFrame = decoder.Frames[0];
                BitmapMetadata metaData = (BitmapMetadata)bitmapFrame.Metadata.Clone();
                foreach (var _m in metaData)
                {
                    Console.WriteLine(_m);
                    if (!_m.Contains("tEXt") && !_m.Contains("zTxt") && !_m.Contains("unknown")) continue;

                    foreach (var _t in metaData.GetQuery(_m) as BitmapMetadata)
                    {
                        string key = $"{_m}{_t}";
                        string value = metaData.GetQuery(key).ToString();
                        Console.WriteLine($"\t{key}: {value}");
                    }
                }
                Console.WriteLine(metaData.GetQuery("/Text/Description"));
            }
        }


        [Test]
        public void TestPngSaveMeta()
        {
            Stream pngStream = new System.IO.FileStream(imagePath2, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            PngBitmapDecoder pngDecoder = new PngBitmapDecoder(pngStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            BitmapFrame pngFrame = pngDecoder.Frames[0];
            InPlaceBitmapMetadataWriter pngInplace = pngFrame.CreateInPlaceBitmapMetadataWriter();
            if (pngInplace.TrySave() == true)
            {
                Console.WriteLine("SetQuery...");
                //pngInplace.SetQuery("/Text/Description", "Have a nice day.");
                pngInplace.SetQuery("/tEXt/NI Image Type", "7");
                pngInplace.SetQuery("/tEXt/AvLenth", "8");

                //pngInplace.SetQuery("/tEXt/{str=NI Image Type}", "7");
                //pngInplace.SetQuery("/tEXt/{str=AvLenth}", "8");
            }
            pngStream.Close();
        }

        [Test]
        public void TestPngSaveMeta2()
        {
            var img = Cv2.ImRead(imagePath, ImreadModes.Unchanged);

            int width = img.Width;
            int height = img.Height;
            int stride = (int)img.Step();

            WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray16, null);
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), img.Data, height * stride, stride);


            var metadata = new BitmapMetadata("png");
            metadata.SetQuery("/tEXt/NI Image Type", "7");
            metadata.SetQuery("/[1]tEXt/AvLenth", "8");

            var frame = BitmapFrame.Create(bitmap, null, metadata, null);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(frame);
            using (var stream = File.OpenWrite(imagePath2))
            {
                encoder.Save(stream);
            }
        }


        [Test]
        public void TestSignificantBitParser()
        {
            var sbp = new SignificantBitParser(244);
            Assert.That(sbp.SignificantHigh, Is.EqualTo(15));
            Assert.That(sbp.SignificantLow, Is.EqualTo(4));
            Assert.That(sbp.SignificantMask, Is.EqualTo(0b1111111111110000));
        }
    }
}