using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Util.Checker
{
    class SHA256Checker : IChecker
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

        public Task<bool> CheckFilePassAsync()
        {
            return Task.Run(() => CheckFilePass());
        }

        public string GetFileChecksum()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
            {
                throw new ArgumentException("检验器校验目标文件路径为空");
            }
            FileStream file = new FileStream(FilePath, FileMode.Open);
            SHA256 sha256 = new SHA256CryptoServiceProvider();//创建SHA1对象
            byte[] sha256Bytes = sha256.ComputeHash(file);//Hash运算
            sha256.Dispose();//释放当前实例使用的所有资源
            file.Dispose();
            string result = BitConverter.ToString(sha256Bytes);//将运算结果转为string类型
            result = result.Replace("-", "");
            return result;
        }
    }
}
