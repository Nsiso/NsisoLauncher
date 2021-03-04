using NsisoLauncherCore.Util;
using System;
using System.Diagnostics;

namespace NsisoLauncherCore.Net.Tools
{
    /// <summary>
    /// An java installer getter for open jdk api.
    /// </summary>
    public static class GetJavaInstaller
    {
        public static DownloadTask GetDownloadTask(string feature_version, ArchEnum arch, JavaImageType image_type, Action closed_todo)
        {
            string arch_str;
            switch (arch)
            {
                case ArchEnum.x32:
                    arch_str = "32";
                    break;
                case ArchEnum.x64:
                    arch_str = "64";
                    break;
                default:
                    arch_str = "32";
                    break;
            }

            string image_type_str;
            switch (image_type)
            {
                case JavaImageType.JRE:
                    image_type_str = "jre";
                    break;
                case JavaImageType.JDK:
                    image_type_str = "jdk";
                    break;
                default:
                    image_type_str = "jre";
                    break;
            }

            string url = string.Format("https://api.azul.com/zulu/download/community/v1.0/bundles/latest/binary/?jdk_version={0}&os=windows&arch=x86&hw_bitness={1}&ext=msi&bundle_type={2}&release_status=ga&support_term=lts",
                feature_version, arch_str, image_type_str);

            string to = string.Format("{0}\\zulu_{1}_x{2}.msi", PathManager.TempDirectory, image_type_str, arch_str);

            DownloadTask task = new DownloadTask(string.Format("Java (zulu_{0}_x{1})", image_type_str, arch_str), new StringUrl(url), to);

            task.DownloadObject.Todo = new Func<ProgressCallback, System.Threading.CancellationToken, Exception>((a, b) =>
            {
                try
                {
                    Process process = Process.Start(to);
                    process.WaitForExit();
                    closed_todo();
                }
                catch (Exception ex)
                {
                    return ex;
                }
                return null;
            });

            return task;
        }
    }
}
