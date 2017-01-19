using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UpYun.SDK.Internal
{
    internal static class MD5Helper
    {
        public static string GetMD5(string source)
        {
            var md5 = new MD5CryptoServiceProvider();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(source);
            byte[] sign = md5.ComputeHash(data);
            StringBuilder md5Buff = new StringBuilder(32);

            for (int i = 0; i < sign.Length; i++)
            {
                md5Buff.Append(sign[i].ToString("x2"));
            }

            return md5Buff.ToString();
        }

        public static string GetMD5(byte[] source)
        {
            var md5 = new MD5CryptoServiceProvider();
            byte[] sign = md5.ComputeHash(source);

            StringBuilder md5Buff = new StringBuilder();
            for (int i = 0; i < sign.Length; i++)
            {
                md5Buff.Append(sign[i].ToString("x2"));
            }
            return md5Buff.ToString();
        }

        public static byte[] GetMD5Bytes(string source)
        {
            var md5 = new MD5CryptoServiceProvider();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(source);
            return md5.ComputeHash(data);
        }

        public static byte[] GetMD5Bytes(byte[] source)
        {
            var md5 = new MD5CryptoServiceProvider();
            return md5.ComputeHash(source);
        }
    }
}
