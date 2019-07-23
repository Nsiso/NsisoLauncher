using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NsisoLauncherCore.Util.Checker
{
    public interface IChecker
    {
        string CheckSum { get; set; }
        string FilePath { get; set; }

        bool CheckFilePass();
        string GetFileChecksum();
    }
}
