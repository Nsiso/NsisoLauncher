using System;
using System.IO;
using System.Security.Cryptography;

namespace NsisoLauncherCore.Util.Checker
{
    public class MD5Checker : IChecker
    {
        public string CheckSum { get; set; }
        public string FilePath { get; set; }

        public bool CheckFilePass()
        {
            if (string.IsNullOrWhiteSpace(CheckSum))
            {
                throw new ArgumentException("检验器缺少校验值");
            }
            return string.Equals(CheckSum, GetFileChecksum(), StringComparison.OrdinalIgnoreCase);
        }

        public string GetFileChecksum()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
            {
                throw new ArgumentException("检验器校验目标文件路径为空");
            }
            FileStream file = new FileStream(FilePath, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();//创建SHA1对象
            byte[] md5Bytes = md5.ComputeHash(file);//Hash运算
            md5.Dispose();//释放当前实例使用的所有资源
            file.Dispose();
            string result = BitConverter.ToString(md5Bytes);//将运算结果转为string类型
            result = result.Replace("-", "");
            return result;
        }
    }
}
