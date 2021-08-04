using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using NsisoLauncherCore.Modules;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NsisoLauncherCore.Util
{
    public static class Unzip
    {

        public static void UnZipNativeFile(string zipPath, string outFolder, Extract extract, bool check)

        {
            List<string> exclude = null;
            if (extract != null)
            {
                exclude = extract.Exculde;
            }

            ZipFile zf = null;
            try
            {
                FileStream fs = File.OpenRead(zipPath);
                zf = new ZipFile(fs);
                if (!Directory.Exists(outFolder))
                {
                    Directory.CreateDirectory(outFolder);
                }

                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;
                    }
                    if (exclude != null && exclude.Any(zipEntry.Name.StartsWith))
                    {
                        continue;
                    }

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    string fullZipToPath = Path.Combine(outFolder, zipEntry.Name);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (!Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    if (check)
                    {
                        if (File.Exists(fullZipToPath))
                        {
                            FileInfo info = new FileInfo(fullZipToPath);
                            if (info.Length == zipEntry.Size)
                            {
                                continue;
                            }
                        }
                    }

                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }

        public static void UnZipFile(string zipPath, string outFolder)
        {
            (new FastZip()).ExtractZip(zipPath, outFolder, null);
        }
    }
}