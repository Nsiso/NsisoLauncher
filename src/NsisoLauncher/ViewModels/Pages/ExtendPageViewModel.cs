using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Utils;
using NsisoLauncherCore.Component.Mod;
using NsisoLauncherCore.Component.ResourcePack;
using NsisoLauncherCore.Component.Save;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace NsisoLauncher.ViewModels.Pages
{
    public class ExtendPageViewModel : INotifyPropertyChanged
    {
        public VersionBase SelectedVersion { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        public ObservableCollection<VersionBase> VersionList { get; private set; }

        #region 存档数据
        public SaveManager SaveManager { get; set; }

        /// <summary>
        /// 版本存档
        /// </summary>
        public ObservableCollection<SaveInfo> VerSaves { get => SaveManager?.Items; }

        /// <summary>
        /// 选中的存档
        /// </summary>
        public SaveInfo SelectedSave { get; set; }

        /// <summary>
        /// 删除存档指令
        /// </summary>
        public ICommand DeleteSaveCmd { get; set; }

        /// <summary>
        /// 添加存指令
        /// </summary>
        public ICommand AddSaveCmd { get; set; }
        #endregion

        #region Mod数据
        public ModManager ModManager { get; set; }

        /// <summary>
        /// 版本mod
        /// </summary>
        public ObservableCollection<ModInfo> VerMods { get => ModManager?.Items; }

        /// <summary>
        /// 选中的mod实例
        /// </summary>
        public ModInfo SelectedMod { get; set; }

        public ICommand AddModCmd { get; set; }

        public ICommand DeleteModCmd { get; set; }
        #endregion

        #region resource packs数据
        public ResourcePackManager ResourcePackManager { get; set; }

        /// <summary>
        /// 版本mod
        /// </summary>
        public ObservableCollection<ResourcePackInfo> VerResourcePacks { get => ResourcePackManager?.Items; }

        /// <summary>
        /// 选中的mod实例
        /// </summary>
        public ResourcePackInfo SelectedResourcePack { get; set; }

        public ICommand AddResourcePackCmd { get; set; }

        public ICommand DeleteResourcePackCmd { get; set; }
        #endregion

        public ICommand ValidateDependCmd { get; set; }

        public ICommand ValidateAssetsCmd { get; set; }


        public ExtendPageViewModel()
        {
            if (App.VersionList != null)
            {
                this.VersionList = App.VersionList;
            }
            if (App.LauncherData != null)
            {
                App.LauncherData.PropertyChanged += LauncherData_PropertyChanged;
                this.SelectedVersion = App.LauncherData.SelectedVersion;
                UpdateVersion();
            }

            DeleteSaveCmd = new DelegateCommand(async (a) =>
            {
                await DeleteSave();
            });
            AddSaveCmd = new DelegateCommand(async (a) =>
            {
                await AddSave();
            });

            DeleteModCmd = new DelegateCommand(async (a) =>
            {
                await DeleteMod();
            });
            AddModCmd = new DelegateCommand(async (a) =>
            {
                await AddMod();
            });

            DeleteResourcePackCmd = new DelegateCommand(async (a) =>
            {
                await DeleteResourcePack();
            });
            AddResourcePackCmd = new DelegateCommand(async (a) =>
            {
                await AddResourcePack();
            });

            ValidateDependCmd = new DelegateCommand(async (a) =>
            {
                await ValidateDepend();
            });

            this.PropertyChanged += ExtendPageViewModel_PropertyChanged;
        }

        private async Task ValidateDepend()
        {
            if (SelectedVersion == null)
            {
                await App.MainWindowVM.ShowMessageAsync("未选择版本", "选择的版本为空");
                return;
            }
            var progress = await App.MainWindowVM.ShowProgressAsync("正在检查游戏依赖文件完整性...", "这不需要太长的时间");
            progress.SetIndeterminate();
            var result = await GameValidator.ValidateAsync(App.Handler, SelectedVersion, ValidateType.DEPEND);
            await progress.CloseAsync();

            if (result.IsPass)
            {
                await App.MainWindowVM.ShowMessageAsync("游戏依赖文件检查通过", "所有的文件都存在且完整");
            }
            else
            {
                var choose = await App.MainWindowVM.ShowMessageAsync("游戏依赖文件检查未通过", string.Format("存在{0}个文件缺失或不完整，是否进行修复？", result.FailedFiles.Count),
                    MessageDialogStyle.AffirmativeAndNegative, null);
                switch (choose)
                {
                    case MessageDialogResult.Negative:
                        break;
                    case MessageDialogResult.Affirmative:
                        foreach (var item in result.FailedFiles)
                        {
                            if (item.Value == FileFailedState.WRONG_HASH)
                            {
                                File.Delete(item.Key);
                            }
                        }
                        App.LogHandler.AppendInfo("检查丢失的依赖库文件中...");
                        var lostDepend = await FileHelper.GetLostDependDownloadTaskAsync(
                            App.Handler,
                            SelectedVersion, App.NetHandler.Mirrors.VersionListMirrorList);
                        if (lostDepend.Count != 0)
                        {
                            if (!App.NetHandler.Downloader.IsBusy)
                            {
                                App.NetHandler.Downloader.AddDownloadTask(lostDepend);
                                App.MainPageVM.NavigateToDownloadPage();
                                await App.NetHandler.Downloader.StartDownload();
                            }
                            else
                            {
                                await App.MainWindowVM.ShowMessageAsync("无法下载补全：当前有正在下载中的任务", "请等待其下载完毕或取消下载");
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private async Task DeleteSave()
        {
            if (SelectedSave == null)
            {
                await App.MainWindowVM.ShowMessageAsync("未选择存档", "选择删除的存档为空");
                return;
            }

            var result = await App.MainWindowVM.ShowMessageAsync(string.Format("确定删除存档{0}？", SelectedSave.Name),
                "这个存档会彻底消失且无法找回！", MessageDialogStyle.AffirmativeAndNegative, null);
            switch (result)
            {
                case MessageDialogResult.Canceled:
                    break;
                case MessageDialogResult.Negative:
                    break;
                case MessageDialogResult.Affirmative:
                    SaveInfo save = SelectedSave;
                    SaveManager.Remove(save);
                    await App.MainWindowVM.ShowMessageAsync("删除成功", "成功删除所选的存档");
                    break;
                default:
                    break;
            }
        }

        private async Task AddSave()
        {
            if (SelectedVersion == null)
            {
                await App.MainWindowVM.ShowMessageAsync("未选择版本", "选择的版本为空");
                return;
            }

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "请选择存档文件夹路径";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string foldPath = dialog.SelectedPath;
                    if (string.IsNullOrWhiteSpace(foldPath))
                    {
                        await App.MainWindowVM.ShowMessageAsync("选择了空路径", "无法将空路径存档添加至版本存档中");
                        return;
                    }

                    try
                    {
                        SaveManager.Add(foldPath);
                    }
                    catch (Exception ex)
                    {
                        await App.MainWindowVM.ShowMessageAsync("添加存档失败", string.Format("添加存档时出现意外：{0}", ex.ToString()));
                        return;
                    }
                    await App.MainWindowVM.ShowMessageAsync("添加存档成功", "快去看看吧");
                }
            }
        }

        private async Task DeleteMod()
        {
            if (SelectedMod == null)
            {
                await App.MainWindowVM.ShowMessageAsync("未选择版本", "选择的版本为空");
                return;
            }

            var result = await App.MainWindowVM.ShowMessageAsync(string.Format("确定删除Mod:{0}？", SelectedMod.Name),
                "这个Mod会彻底消失且无法找回！", MessageDialogStyle.AffirmativeAndNegative, null);
            switch (result)
            {
                case MessageDialogResult.Canceled:
                    break;
                case MessageDialogResult.Negative:
                    break;
                case MessageDialogResult.Affirmative:
                    ModInfo mod = SelectedMod;
                    ModManager.Remove(mod);
                    await App.MainWindowVM.ShowMessageAsync("删除成功", "成功删除所选的Mod");
                    break;
                default:
                    break;
            }
        }

        private async Task AddMod()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "选择Mod路径";
                dialog.Filter = "JarMod(*.jar)|*.jar|ZipMod(*.zip)|*.zip";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string modPath = dialog.FileName;
                    if (string.IsNullOrWhiteSpace(modPath))
                    {
                        await App.MainWindowVM.ShowMessageAsync("选择了空路径", "无法将空路径Mod添加至版本Mod中");
                        return;
                    }

                    string dest_path = string.Format("{0}\\{1}", App.Handler.GetVersionModsDir(SelectedVersion), Path.GetFileName(modPath));
                    if (File.Exists(dest_path))
                    {
                        await App.MainWindowVM.ShowMessageAsync("添加Mod失败", "Mod列表中已经存在这个Mod");
                        return;
                    }
                    try
                    {
                        ModManager.Add(modPath);
                    }
                    catch (Exception ex)
                    {
                        await App.MainWindowVM.ShowMessageAsync("添加Mod失败", string.Format("添加Mod时出现意外：{0}", ex.ToString()));
                        return;
                    }
                    await App.MainWindowVM.ShowMessageAsync("添加Mod成功", "快去看看吧");
                }
            }
        }

        private async Task DeleteResourcePack()
        {
            if (SelectedMod == null)
            {
                await App.MainWindowVM.ShowMessageAsync("未选择版本", "选择的版本为空");
                return;
            }

            var result = await App.MainWindowVM.ShowMessageAsync(string.Format("确定删除ResourcePack:{0}？", SelectedMod.Name),
                "这个ResourcePack会彻底消失且无法找回！", MessageDialogStyle.AffirmativeAndNegative, null);
            switch (result)
            {
                case MessageDialogResult.Canceled:
                    break;
                case MessageDialogResult.Negative:
                    break;
                case MessageDialogResult.Affirmative:
                    ResourcePackInfo resourcePack = SelectedResourcePack;
                    ResourcePackManager.Remove(resourcePack);
                    await App.MainWindowVM.ShowMessageAsync("删除成功", "成功删除所选的ResourcePack");
                    break;
                default:
                    break;
            }
        }

        private async Task AddResourcePack()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "选择ResourcePack路径";
                dialog.Filter = "ResourcePack(*.zip)|*.zip";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string resourcePackPath = dialog.FileName;
                    if (string.IsNullOrWhiteSpace(resourcePackPath))
                    {
                        await App.MainWindowVM.ShowMessageAsync("选择了空路径", "无法将空路径ResourcePack添加至版本ResourcePack中");
                        return;
                    }

                    string dest_path = string.Format("{0}\\{1}", App.Handler.GetVersionResourcePacksDir(SelectedVersion), Path.GetFileName(resourcePackPath));
                    if (File.Exists(dest_path))
                    {
                        await App.MainWindowVM.ShowMessageAsync("添加ResourcePack失败", "Mod列表中已经存在这个ResourcePack");
                        return;
                    }
                    try
                    {
                        ResourcePackManager.Add(resourcePackPath);
                    }
                    catch (Exception ex)
                    {
                        await App.MainWindowVM.ShowMessageAsync("添加ResourcePack失败", string.Format("添加ResourcePack时出现意外：{0}", ex.ToString()));
                        return;
                    }
                    await App.MainWindowVM.ShowMessageAsync("添加ResourcePack成功", "快去看看吧");
                }
            }
        }

        private void LauncherData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedVersion")
            {
                this.SelectedVersion = App.LauncherData.SelectedVersion;
            }
        }

        private void ExtendPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedVersion")
            {
                App.LauncherData.SelectedVersion = this.SelectedVersion;
                UpdateVersion();
            }
        }

        private void UpdateVersion()
        {
            if (SelectedVersion != null)
            {
                ModManager = new ModManager(SelectedVersion, App.Handler);
                SaveManager = new SaveManager(SelectedVersion, App.Handler);
                ResourcePackManager = new ResourcePackManager(SelectedVersion, App.Handler);
                ModManager.Refresh();
                SaveManager.Refresh();
                ResourcePackManager.Refresh();
            }
            else
            {
                ModManager = null;
                SaveManager = null;
                ResourcePackManager = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
