using MahApps.Metro.Controls.Dialogs;
using NsisoLauncher.Utils;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace NsisoLauncher.ViewModels.Pages
{
    public class ExtendPageViewModel : INotifyPropertyChanged
    {
        public NsisoLauncherCore.Modules.Version SelectedVersion { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        public ObservableCollection<NsisoLauncherCore.Modules.Version> VersionList { get; private set; }

        #region 存档数据
        /// <summary>
        /// 版本存档
        /// </summary>
        public ObservableCollection<SaveInfo> VerSaves { get; set; }

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


        public ExtendPageViewModel()
        {
            VerSaves = new ObservableCollection<SaveInfo>();
            if (App.VersionList != null)
            {
                this.VersionList = App.VersionList;
            }
            if (App.LauncherData != null)
            {
                App.LauncherData.PropertyChanged += LauncherData_PropertyChanged;
                this.SelectedVersion = App.LauncherData.SelectedVersion;
                UpdateVersionSaves();
            }

            DeleteSaveCmd = new DelegateCommand(new Action<object>(DeleteSave));
            AddSaveCmd = new DelegateCommand(new Action<object>(AddSave));

            this.PropertyChanged += ExtendPageViewModel_PropertyChanged;
        }

        private async void DeleteSave(object obj)
        {
            if (SelectedSave == null)
            {
                await App.MainWindowVM.ShowMessageAsync("未选择存档", "选择删除的存档为空");
                return;
            }

            var result = await App.MainWindowVM.ShowMessageAsync(string.Format("确定删除存档{0}？", SelectedSave.LevelName),
                "这个存档会彻底消失且无法找回！", MessageDialogStyle.AffirmativeAndNegative, null);
            switch (result)
            {
                case MessageDialogResult.Canceled:
                    break;
                case MessageDialogResult.Negative:
                    break;
                case MessageDialogResult.Affirmative:
                    SaveInfo save = SelectedSave;
                    VerSaves.Remove(save);
                    save.DeleteSave();
                    break;
                default:
                    break;
            }
        }

        private async void AddSave(object obj)
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
                        FileHelper.DirectoryCopy(foldPath, App.Handler.GetVersionSavesDir(SelectedVersion), true, false);
                    }
                    catch (Exception ex)
                    {
                        await App.MainWindowVM.ShowMessageAsync("添加存档失败", string.Format("添加存档时出现意外：{0}", ex.ToString()));
                        return;
                    }

                    UpdateVersionSaves();
                    await App.MainWindowVM.ShowMessageAsync("添加存档成功", "快去看看吧");
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
                UpdateVersionSaves();
            }
        }

        private void UpdateVersionSaves()
        {
            VerSaves.Clear();
            if (this.SelectedVersion != null)
            {
                List<SaveInfo> saves = App.Handler.SaveHandler.GetSaves(this.SelectedVersion);
                if (saves != null)
                {
                    foreach (var item in saves)
                    {
                        VerSaves.Add(item);
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
