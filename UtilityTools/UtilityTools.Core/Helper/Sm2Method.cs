#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2025   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Helper
 * 唯一标识：d7a0b52f-3dbc-4333-a632-841e9f4a9699
 * 文件名：Sm2Method
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2025/6/6 10:42:44
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

using SMCrypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Security.Cryptography.ECCurve;

namespace UtilityTools.Core.Helper
{
    public class Sm2Method
    {
        public static string PrivateKey = "e6d752697ddb0afb758b7e624dce9986db5d94fdb3e924bc9550d7395e32932b";
        public static string PublicKey = "04d171ad9af2a33767344611a29365c41d3e7f15c83d44537fb87e1b629f2f963d44825937e1107a9c203b5c857ac114634fd8b9b6e8f7e473bce6df8b288dca94";

        // 生成SM2密钥对
        public static SM2Key GenerateKeyPair()
        {
            return SM2.GenerateKeyPairHex();
        }

        // SM2加密
        public static string Encrypt(string msg, string publicKey, SM2.CipherMode cipherMode = SM2.CipherMode.C1C3C2)
        {
            return SM2.DoEncrypt(msg, publicKey, cipherMode);
        }

        // SM2解密
        public static string Decrypt(string encryptData, string privateKey, SM2.CipherMode cipherMode = SM2.CipherMode.C1C3C2)
        {
            var result = SM2.DoDecrypt(encryptData, privateKey, cipherMode);
            return result.ToString();
        }
    }
}
