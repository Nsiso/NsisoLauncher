using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static NsisoLauncherCore.Util.JsonTools;

namespace NsisoLauncherCore.Modules
{
    [JsonConverter(typeof(ArtifactJsonConverter))]
    public class Artifact
    {
        /// <summary>
        /// 包名
        /// </summary>
        public string Package { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Classifier分类
        /// </summary>
        public string Classifier { get; set; }

        /// <summary>
        /// 扩展名
        /// </summary>
        public string Extension { get; set; } = "jar";

        /// <summary>
        /// 原字符串
        /// </summary>
        public string Descriptor { get; set; }

        public Artifact(string descriptor)
        {
            if (string.IsNullOrEmpty(descriptor))
            {
                throw new ArgumentNullException("The artifact description must not be null or empty");
            }

            this.Descriptor = descriptor;
            string[] parts = descriptor.Split(':');

            int length = parts.Length;

            this.Package = parts[0];
            this.Name = parts[1];
            this.Version = parts[2];

            int last = parts.Length - 1;
            int idx = parts[last].IndexOf('@');
            if (idx != -1)
            {
                this.Extension = parts[last].Substring(idx + 1);
                parts[last] = parts[last].Substring(0, idx);
            }
            this.Version = parts[2];
            if (parts.Length > 3)
            {
                this.Classifier = parts[3];
            }

            //int last = parts.Length - 1;
            //int idx = parts[last].IndexOf('@');
            //if (idx != -1)
            //{
            //    this.Extension = parts[last].Substring(idx + 1);
            //    parts[last] = parts[last].Substring(0, idx);
            //}


            //if (parts.Length > 3)
            //{
            //    this.Classifier = parts[3];
            //}
        }

        public static Artifact From(string descriptor)
        {
            return new Artifact(descriptor);
        }

        public string Path
        {
            get
            {
                if (Classifier == null)
                {
                    return string.Format(@"{0}\{1}\{2}\{1}-{2}.{3}", Package.Replace(".", "\\"), Name, Version, Extension);
                }
                else
                {
                    return string.Format(@"{0}\{1}\{2}\{1}-{2}-{3}.{4}", Package.Replace(".", "\\"), Name, Version, Classifier, Extension);
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
