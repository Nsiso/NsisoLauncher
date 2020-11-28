using System.Threading.Tasks;

namespace NsisoLauncherCore.Util.FileChecker
{
    public interface IChecker
    {
        string CheckSum { get; set; }
        string FilePath { get; set; }

        bool CheckFilePass();
        Task<bool> CheckFilePassAsync();
        string GetFileChecksum();
        //Task<string> GetFileChecksumAsync();
    }
}
