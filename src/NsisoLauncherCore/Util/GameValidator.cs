using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using NsisoLauncherCore.Util.Checker;
using NsisoLauncherCore.Modules;

namespace NsisoLauncherCore.Util
{
    [Flags]
    public enum ValidateType
    {
        ONLY_CORE,
        ONLY_LIB,
        ONLY_ASSETS,
        DEPEND = ONLY_CORE | ONLY_LIB,
        ALL = ONLY_CORE | ONLY_LIB | ONLY_ASSETS
    }

    public enum FileFailedState
    {
        NOT_EXIST,
        WRONG_HASH
    }

    public static class GameValidator
    {
        public static Task<GameValidateResult> ValidateAsync(LaunchHandler handler, VersionBase version, ValidateType type)
        {

            return Task.Run(() =>
             {
                 return Validate(handler, version, type);
             });
        }

        public static GameValidateResult Validate(LaunchHandler handler, VersionBase version, ValidateType type)
        {
            GameValidateResult result = new GameValidateResult();
            SHA1Checker checker = new SHA1Checker();
            switch (type)
            {
                case ValidateType.ONLY_CORE:
                    string core_path = handler.GetVersionJarPath(version);
                    if (version.Downloads?.Client?.Sha1 != null)
                    {
                        if (!File.Exists(core_path))
                        {
                            result.IsPass = false;
                            result.FailedFiles.Add(core_path, FileFailedState.NOT_EXIST);
                        }
                        checker.CheckSum = version.Downloads.Client.Sha1;
                        checker.FilePath = core_path;
                        if (!checker.CheckFilePass())
                        {
                            result.IsPass = false;
                            result.FailedFiles.Add(core_path, FileFailedState.WRONG_HASH);
                        }
                    }
                    break;
                case ValidateType.ONLY_LIB:
                    List<Library> libraries = version.GetAllLibraries();
                    foreach (var item in libraries)
                    {
                        string lib_path = handler.GetLibraryPath(item);

                        if (!File.Exists(lib_path))
                        {
                            result.IsPass = false;
                            result.FailedFiles.Add(lib_path, FileFailedState.NOT_EXIST);
                            continue;
                        }

                        if (item.LocalDownloadInfo?.Sha1 != null)
                        {
                            checker.CheckSum = item.LocalDownloadInfo.Sha1;
                            checker.FilePath = lib_path;
                            if (!checker.CheckFilePass())
                            {
                                result.IsPass = false;
                                result.FailedFiles.Add(lib_path, FileFailedState.WRONG_HASH);
                            }
                        }
                    }
                    break;
                case ValidateType.ONLY_ASSETS:
                    JAssets assets = handler.GetAssets(version);
                    if (assets == null)
                    {
                        result.IsPass = false;
                        result.FailedFiles.Add(handler.GetAssetsIndexPath(version.Assets), FileFailedState.NOT_EXIST);
                        break;
                    }
                    foreach (var item in assets.Objects)
                    {
                        string asset_path = handler.GetAssetsPath(item.Value);

                        if (!File.Exists(asset_path))
                        {
                            result.IsPass = false;
                            result.FailedFiles.Add(asset_path, FileFailedState.NOT_EXIST);
                            continue;
                        }

                        if (item.Value?.Hash != null)
                        {
                            checker.CheckSum = item.Value.Hash;
                            checker.FilePath = asset_path;
                            if (!checker.CheckFilePass())
                            {
                                result.IsPass = false;
                                result.FailedFiles.Add(asset_path, FileFailedState.WRONG_HASH);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            return result;
        }
    }

    public class GameValidateResult
    {
        public bool IsPass { get; set; }

        public Dictionary<string, FileFailedState> FailedFiles { get; set; }

        public GameValidateResult()
        {
            IsPass = true;
            FailedFiles = new Dictionary<string, FileFailedState>();
        }
    }
}
