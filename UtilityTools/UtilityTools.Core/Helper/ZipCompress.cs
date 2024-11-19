using Microsoft.Win32;
using Newtonsoft.Json;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UtilityTools.Core.Helper
{
    /// <summary>
    /// 压缩文件和提取文件的方法
    /// </summary>
    public static class  ZipCompress
    {
        /// <summary>
        /// 压缩文件,压缩文件在同一个文件夹下
        /// </summary>
        public static string CompressFile(string sourcePath)
        {
            if (sourcePath == "")
            {
                return null;
            }
            string destinationPath = sourcePath + ".zip";
            if (!File.Exists(destinationPath))
            {
                ZipFile.CreateFromDirectory(sourcePath, destinationPath);
                return destinationPath;
            }
            else
            {
                
                return null;
            }
        }
        /// <summary>
        /// 提取文件 ，提取文件在同一个文件夹下
        /// </summary>
        /// <returns> 提取后的的文件地址</returns>     
        public static string ZipExtract(string sourcePath)
        {
            if (!File.Exists(sourcePath))
            {
                return "exist";
            }

            if (sourcePath.Length <= 4 || Path.GetExtension(sourcePath) != ".zip")
            {
                return "Path_False";
            }
            string destinationPath = Path.GetDirectoryName(sourcePath) + @"\" + Path.GetFileNameWithoutExtension(sourcePath);
            ZipFile.ExtractToDirectory(sourcePath, destinationPath);
            return destinationPath;
        }

        public static void ZipFiles(string targetPath, params string[] files) 
        {
            if(files.Length == 0) 
            {
                throw new ArgumentException("缺乏目标文件");
            }

            var file = files[0];
            if(!File.Exists(file)) 
            {
                throw new ArgumentException("压缩文件不存在");
            }

            var fileName = Path.GetFileName(file);

            using (FileStream zipToOpen = new FileStream(targetPath, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    ZipArchiveEntry readmeEntry = archive.CreateEntry(fileName);
                    using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                    {
                        writer.BaseStream.Seek(0, SeekOrigin.Begin);
                        using(StreamReader read = new StreamReader(file)) 
                        {
                            read.BaseStream.Seek(0, SeekOrigin.Begin);
                            read.BaseStream.CopyTo(writer.BaseStream);
                        }
                    }

                    for(int i =1; i < files.Length; i++) 
                    {
                        file = files[i];
                        if (!File.Exists(file))
                        {
                            throw new ArgumentException("压缩文件不存在");
                        }

                        fileName = Path.GetFileName(file);

                        readmeEntry = archive.CreateEntry(fileName);
                        using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                        {
                            using (StreamReader read = new StreamReader(file))
                            {
                                read.BaseStream.Seek(0, SeekOrigin.Begin);
                                read.BaseStream.CopyTo(writer.BaseStream);
                            }
                        }
                    }
                }
            }
        }

       

    }
   
}
