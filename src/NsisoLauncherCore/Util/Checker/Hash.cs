using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace NsisoLauncherCore.Util.Checker
{
    public class Hash
    {
        public HashType Type { get; set; }

        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                if (_value != null)
                {
                    this._value = value.ToLower();
                }
                else
                {
                    this.Value = value;
                }
            }
        }

        public Hash(HashType type, string value)
        {
            this.Type = type;
            this._value = value;
        }

        public HashAlgorithm GetHashAlgorithm()
        {
            return GetHashAlgorithm(this.Type);
        }

        public static HashAlgorithm GetHashAlgorithm(HashType type)
        {
            HashAlgorithm hashAlgorithm;
            switch (type)
            {
                case HashType.MD5:
                    hashAlgorithm = new MD5CryptoServiceProvider();
                    break;
                case HashType.SHA1:
                    hashAlgorithm = new SHA1CryptoServiceProvider();
                    break;
                case HashType.SHA256:
                    hashAlgorithm = new SHA256CryptoServiceProvider();
                    break;
                case HashType.SHA512:
                    hashAlgorithm = new SHA512CryptoServiceProvider();
                    break;
                default:
                    throw new Exception("Unknown hash type.");
            }
            return hashAlgorithm;
        }

        public bool Equals(IHashProvider hash_provider)
        {
            Hash hash = hash_provider.GetHash();
            return Equals(hash);
        }

        public bool Equals(Hash hash)
        {
            return Type.Equals(hash.Type) && Value.Equals(hash.Value);
        }

        public bool Equals(HashType type, string hash)
        {
            return this.Type.Equals(type) && this.Value.Equals(hash, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type.GetHashCode(), Value.GetHashCode());
        }

        public override string ToString()
        {
            return string.Format("[{0}]:{1}", this.Type, this.Value);
        }
    }
}
