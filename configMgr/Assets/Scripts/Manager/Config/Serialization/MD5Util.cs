using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Kernel
{
    public class MD5Util
    {
        private static readonly MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
        private static readonly StringBuilder str = new StringBuilder(16);

        public static byte[] MD5(byte[] data, int offset, int count)
        {
            return md5Provider.ComputeHash(data, offset, count);
        }

        public static byte[] MD5(byte[] data)
        {
            return md5Provider.ComputeHash(data);
        }

        public static byte[] MD5(Stream s)
        {
            return md5Provider.ComputeHash(s);
        }

        public static byte[] MD5(string data)
        {
            //Widnows Phone 没有Encoding.Default
            return md5Provider.ComputeHash(Encoding.UTF8.GetBytes(data));
        }

        public static string Md5ToString(byte[] md5)
        {
            str.Length = 0;
            foreach (var b in md5)
            {
                str.Append(b.ToString("x2"));
            }
            return str.ToString();
        }

        public static bool MD5(string path, out string md5)
        {
            try
            {
                using (var fs = PlatformManager.Instance.OpenRead(path))
                {
                    md5 = Md5ToString(MD5(fs));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("CheckMd5 failed {0}", ex);
                md5 = string.Empty;
                return false;
            }
        }

        public static bool CheckMd5(string path, string md5Expect)
        {
            try
            {
                using (var fs = PlatformManager.Instance.OpenRead(path))
                {
                    var fileMd5 = Md5ToString(MD5(fs));
                    if (fileMd5 != md5Expect)
                    {
                        Logger.Error("{0} md5 doesn't match, current is {1}, expect {2}", path, fileMd5, md5Expect);
                        return false;
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("CheckMd5 failed {0}", ex);
                return false;
            }
        }
    }
}