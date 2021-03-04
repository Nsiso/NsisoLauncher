using NsisoLauncherCore.Util;
using System.Collections.Generic;

namespace NsisoLauncherCore.LaunchException
{
    public class GameValidateFailedException : LaunchException
    {
        public GameValidateFailedException(Dictionary<string, FileFailedState> failed) : base("The validate of the game files did not pass.",
            "The game file might be broken or missing, please check the game file.")
        {
            this.FailedFiles = failed;
        }

        public Dictionary<string, FileFailedState> FailedFiles { get; set; }
    }
}
