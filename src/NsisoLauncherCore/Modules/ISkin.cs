using System;
using System.Collections.Generic;
using System.Text;

namespace NsisoLauncherCore.Modules
{
    public interface ISkin
    {
        string Url { get; }

        SkinType Type { get; }

        string Name { get; }
    }

    public enum SkinType
    {
        /// <summary>
        /// aka steve
        /// </summary>
        NORMAL,

        /// <summary>
        /// aka alex
        /// </summary>
        SLIM
    }
}
