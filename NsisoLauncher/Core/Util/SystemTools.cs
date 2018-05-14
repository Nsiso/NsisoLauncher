using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace NsisoLauncher.Core.Util
{
    public enum ArchEnum
    {
        x32,
        x64
    }

    public class SystemTools
    {
        /// <summary>
        /// 获取匹配JAVA位数的最佳内存
        /// </summary>
        /// <param name="arch">JAVA位数</param>
        /// <returns>最佳内存大小</returns>
        public static int GetBestMemory(Java java)
        {
            if (java != null)
            {
                int rm = Convert.ToInt32(Math.Floor(GetRunmemory() * 0.6));
                switch (java.Arch)
                {
                    case ArchEnum.x32:
                        if (rm > 1024) { return 1024; }
                        else { return rm; }

                    case ArchEnum.x64:
                        if (rm > 4096) { return 4096; }
                        else { return rm; }

                    default:
                        return rm;
                }
            }
            else
            {
                return 1024;
            }

        }

        /// <summary>
        /// 获取电脑总内存(MB)
        /// </summary>
        /// <returns>物理内存</returns>
        public static ulong GetTotalMemory()
        {
            return new Computer().Info.TotalPhysicalMemory / 1048576;
        }

        /// <summary>
        ///     获取系统位数
        /// </summary>
        /// <returns>32 or 64</returns>
        public static ArchEnum GetSystemArch()
        {
            if (Environment.Is64BitOperatingSystem)
            {
                return ArchEnum.x64;
            }
            else
            {
                return ArchEnum.x32;
            }
        }

        /// <summary>
        /// 获取系统剩余内存(MB)
        /// </summary>
        /// <returns>剩余内存</returns>
        public static ulong GetRunmemory()
        {
            ComputerInfo ComputerMemory = new ComputerInfo();
            return ComputerMemory.AvailablePhysicalMemory / 1048576;
        }

        /// <summary>
        /// 生成机器码
        /// </summary>
        /// <returns></returns>
        public static string GetMachineUUID()
        {
            string code = null;
            SelectQuery query = new SelectQuery("select * from Win32_ComputerSystemProduct");
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (var item in searcher.Get())
                {
                    using (item) code = item["UUID"].ToString();
                }
            }
            code = code.Replace("0", "").Replace("-", "");
            if (code.Length == 8)
            {
                return code;
            }
            else if (code.Length < 8)
            {
                return code.PadRight(8, '0');
            }
            else
            {
                return code.Remove(0, 8);
            }
        }

        /// <summary>
        /// DES机器UUID加密
        /// </summary>
        /// <param name="encryptString"></param>
        /// <returns></returns>
        public static string DesEncrypt(string encryptString)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(GetMachineUUID());
            byte[] keyIV = keyBytes;
            byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
            DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
            MemoryStream mStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(mStream, provider.CreateEncryptor(keyBytes, keyIV), CryptoStreamMode.Write);
            cStream.Write(inputByteArray, 0, inputByteArray.Length);
            cStream.FlushFinalBlock();
            return Convert.ToBase64String(mStream.ToArray());
        }

        /// <summary>
        /// DES机器UUID解密
        /// </summary>
        /// <param name="decryptString"></param>
        /// <returns></returns>
        public static string DesDecrypt(string decryptString)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(GetMachineUUID());
            byte[] keyIV = keyBytes;
            byte[] inputByteArray = Convert.FromBase64String(decryptString);
            DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
            MemoryStream mStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(mStream, provider.CreateDecryptor(keyBytes, keyIV), CryptoStreamMode.Write);
            cStream.Write(inputByteArray, 0, inputByteArray.Length);
            cStream.FlushFinalBlock();
            return Encoding.UTF8.GetString(mStream.ToArray());
        }

        /// <summary>
        /// 获取显卡信息
        /// </summary>
        /// <returns></returns>
        public static string GetVideoCardInfo()
        {
            try
            {
                var sb = new StringBuilder();
                var i = 0;
                foreach (var mo in new ManagementClass("Win32_VideoController").GetInstances().Cast<ManagementObject>())
                {
                    sb.Append('#').Append(i++).Append(mo["Name"].ToString().Trim()).Append(' ');
                }
                return sb.ToString();
            }
            catch
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// 获取CPU信息
        /// </summary>
        /// <returns></returns>
        public static string GetProcessorInfo()
        {
            try
            {
                var sb = new StringBuilder();
                var i = 0;
                foreach (var mo in new ManagementClass("WIN32_Processor").GetInstances().Cast<ManagementObject>())
                {
                    sb.Append('#').Append(i++).Append(mo["Name"].ToString().Trim()).Append(' ');
                }
                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
