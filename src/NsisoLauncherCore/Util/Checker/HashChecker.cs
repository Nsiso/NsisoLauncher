using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Util.Checker
{
    public static class HashChecker
    {

        public static bool CheckFilePass(IHashProvider hash_provider, string file_path)
        {
            return CheckFilePass(hash_provider.GetHash(), file_path);
        }

        public static Task<bool> CheckFilePassAsync(IHashProvider hash_provider, string file_path)
        {
            return Task.Run(() => CheckFilePass(hash_provider, file_path));
        }

        public static bool CheckFilePass(Hash hash, string file_path)
        {
            if (hash == null)
            {
                throw new ArgumentNullException(nameof(hash));
            }
            Hash file_hash = GetFileHash(hash.Type, file_path);
            return hash.Equals(file_hash);
        }

        public static Task<bool> CheckFilePassAsync(Hash hash, string file_path)
        {
            return Task.Run(() => CheckFilePass(hash, file_path));
        }

        public static Hash GetFileHash(HashType type, string file_path)
        {
            if (string.IsNullOrWhiteSpace(file_path))
            {
                throw new ArgumentException("The path of the file is null or white space.");
            }
            if (!File.Exists(file_path))
            {
                throw new FileNotFoundException("the file to get hash is not found", file_path);
            }
            using FileStream file = new FileStream(file_path, FileMode.Open);
            HashAlgorithm algorithm = Hash.GetHashAlgorithm(type);
            using (algorithm)
            {
                byte[] hashBytes = algorithm.ComputeHash(file);//Hash运算
                string result = BitConverter.ToString(hashBytes);//将运算结果转为string类型
                result = result.Replace("-", "");
                Hash hash = new Hash(type, result);
                return hash;
            }
        }
    }
}
