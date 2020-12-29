﻿using Newtonsoft.Json;
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
            this.Descriptor = descriptor;
            string[] parts = descriptor.Split(':');

            this.Package = parts[0];
            this.Name = parts[1];

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
        }

        public static Artifact From(string descriptor)
        {
            return new Artifact(descriptor);
        }
    }
}