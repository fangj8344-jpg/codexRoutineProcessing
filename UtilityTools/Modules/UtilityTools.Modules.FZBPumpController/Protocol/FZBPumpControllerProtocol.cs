using OpenCvSharp;
using OpenCvSharp.ML;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Xml.Linq;

namespace UtilityTools.Modules.FZBPumpController.Protocol
{
    internal class FZBPumpControllerProtocol
    {        
        public static string desAddress = "001";
        public static string ControlCommand = "10";

        /// <summary>
        /// 接收数据检验
        /// </summary>
        /// <param name="frame">接收的数据帧</param>
        /// <returns></returns>
        public static bool Examine(byte[] frame)
        {
            if (frame[frame.Length - 1] != (byte)13) return false;

            if (frame.Length < 14) return false;


            var res = frame.Skip(0).Take(frame.Length - 4).ToArray();
            //int sum = 0;
            //for (int i = 0; i <= frame.Length - 5; i++)
            //{
            //    int asciicode = (int)(frame[i]);
            //    sum += asciicode;
            //}

            //int result = sum - (256 * (sum / 256));
            string number = ASCIIModulo(Encoding.UTF8.GetString(res));
            var examinenumber = frame.Skip(frame.Length-4).Take(3).ToArray();
            try
            {
                int resnumber = int.Parse(Encoding.UTF8.GetString(examinenumber));
                int result = int.Parse(number);

                return resnumber == result;
            }
            catch(FormatException)
            {
                return false;
            }
        } 

        /// <summary>
        ///  校验和
        /// </summary>
        /// <param name="Modulo">校验数据源</param>
        /// <returns></returns>
        public static string ASCIIModulo(string Modulo)
        {
            byte[] array = System.Text.Encoding.ASCII.GetBytes(Modulo);  //数组array为对应的ASCII数组 
            int sum = 0;
            for (int i = 0; i < array.Length; i++)
            {
                int asciicode = (int)(array[i]);
                sum += asciicode;
            }
            int result = sum - (256 * (sum / 256));
            return result.ToString("D3");
        }


        /// <summary>
        ///  开关控制
        /// </summary>
        /// <param name="IsOpen">开关</param>
        /// <param name="Destination">目的地址</param>
        /// <param name="Param">功能码</param>
        /// <returns></returns>
        public static byte[] SendSwitchControlMessage(bool IsOpen, string Destination, string Param)
        {
            string message = "";
            string data = "";
            if (Param.Equals("026"))
            {
                data += (IsOpen ? (ushort)1 :(ushort)0).ToString();
            }
            else
            {
                data += IsOpen ? "111111" : "000000";
            }
            string length = data.Length.ToString("D2");
            message += Destination + ControlCommand + Param + length + data;
            message += ASCIIModulo(message);
            message += "\r";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);
            return bytes;
        }

        /// <summary>
        /// 数据回读
        /// </summary>
        /// <param name="Destination">目的地址</param>
        /// <param name="Param">功能码</param>
        /// <returns></returns>
        public static byte[] DataReadbackControlMessage(string Destination, string Param)
        {
            string message = "";
            string data = "=?";
            string length = data.Length.ToString("D2");
            message = Destination + ControlCommand + Param + length+ data;
            message += ASCIIModulo(message);
            message += "\r";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);
            return bytes;
        }

        /// <summary>
        /// 数据下发
        /// </summary>
        /// <param name="Data">下发数据</param>
        /// <param name="Destination">目的地址</param>
        /// <param name="Param">功能码</param>
        /// <returns></returns>
        public static byte[] DataDeliveryControlMessage(string Data, string Destination, string Param)
        {
            string message = "";
            string data = Data;
            string length = data.Length.ToString("D2");
            message = Destination + ControlCommand + Param + length + data;
            message += ASCIIModulo(message);
            message += "\r";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);
            return bytes;
        }
    }
}
