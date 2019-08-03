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
