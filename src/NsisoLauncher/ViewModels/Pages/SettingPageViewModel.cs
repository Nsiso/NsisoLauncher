using ControlzEx.Theming;
using NsisoLauncher.Config;
using NsisoLauncher.Utils;
using NsisoLauncher.ViewModels.Windows;
using NsisoLauncher.Views.Windows;
using NsisoLauncherCore;
using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net;
using NsisoLauncherCore.Net.Mirrors;
using NsisoLauncherCore.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace NsisoLauncher.ViewModels.Pages
{
    public class SettingPageViewModel : INotifyPropertyChanged
    {
        MainWindowViewModel mainWindowVM;

        public IEnumerable<AccentColorData> ColorSchemes { get; set; } = ThemeManager.Current.Themes
                                            .GroupBy(x => x.ColorScheme)
                                            .OrderBy(a => a.Key)
                                            .Select(a => new AccentColorData { Name = a.Key, ColorBrush = a.First().ShowcaseBrush });
        public IEnumerable<AppThemeData> BaseColors { get; set; } = ThemeManager.Current.Themes
                                         .GroupBy(x => x.BaseColorScheme)
                                         .Select(x => x.First())
                                         .Select(a => new AppThemeData() { Name = a.BaseColorScheme, BorderColorBrush = a.Resources["MahApps.Brushes.ThemeForeground"] as Brush, ColorBrush = a.Resources["MahApps.Brushes.ThemeBackground"] as Brush });
        public string LauncherVersion { get; set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public ObservableCollection<Java> Javas { get; set; }
        public Java SelectedJava { get; set; }
        public string SelectedJavaInfo { get; set; }

        public ObservableCollection<VersionBase> MinecraftVersions { get; set; }

        public VersionBase SelectedSettingVersion { get; set; }
        public ObservableCollection<VersionOption> VersionOptions { get; set; } = new ObservableCollection<VersionOption>();

        public MainConfig MainConfig { get; set; }

        public ulong MaxMemory { get; set; } = SystemTools.GetTotalMemory();

        public int TabIndex { get; set; } = 0;

        #region 命令
        public ICommand ChooseJavaButtonClickCmd { get; set; }
        public ICommand ChooseGameDirButtonClickCmd { get; set; }

        public ICommand SelectedVersionChangeCmd { get; set; }

        public ICommand RefreshVersionListCmd { get; set; }

        public ICommand OpenImageDirCmd { get; set; }
        public ICommand OpenMusicDirCmd { get; set; }
        public ICommand OpenVideoDirCmd { get; set; }

        public ICommand OpenUriCmd { get; set; }

        public ICommand OpenAuthNodeWindowCmd { get; set; }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingPageViewModel()
        {
            if (App.JavaList != null)
            {
                Javas = App.JavaList;
            }
            if (App.VersionList != null)
            {
                MinecraftVersions = App.VersionList;
            }
            if (App.Config?.MainConfig != null)
            {
                MainConfig = App.Config.MainConfig;
                MainConfig.Customize.PropertyChanged += Customize_PropertyChanged;

                App.Config.MainConfig.Environment.PropertyChanged += Environment_PropertyChanged;
                App.Config.MainConfig.Net.PropertyChanged += Download_PropertyChanged;
            }
            if (App.MainWindowVM != null)
            {
                mainWindowVM = App.MainWindowVM;
            }

            #region 命令定义
            ChooseJavaButtonClickCmd = new DelegateCommand(async (a) =>
            {
                await ChooseJava();
            });
            ChooseGameDirButtonClickCmd = new DelegateCommand((a) =>
            {
                ChooseGameDir();
            });
            RefreshVersionListCmd = new DelegateCommand(async (a) =>
            {
                await App.RefreshVersionListAsync();
            });
            OpenImageDirCmd = new DelegateCommand((a) =>
            {
                Process.Start(PathManager.ImageDirectory);
            });
            OpenMusicDirCmd = new DelegateCommand((a) =>
            {
                Process.Start(PathManager.MusicDirectory);
            });
            OpenVideoDirCmd = new DelegateCommand((a) =>
            {
                Process.Start(PathManager.VideoDirectory);
            });
            OpenUriCmd = new DelegateCommand((a) =>
            {
                Process.Start(new ProcessStartInfo(a.ToString()));
            });
            OpenAuthNodeWindowCmd = new DelegateCommand((a) =>
            {
                new AuthNodeWindow().Show();
            });
            #endregion

            PropertyChanged += SettingPageViewModel_PropertyChanged;
        }

        private void Customize_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AccentColor")
            {
                if (!string.IsNullOrWhiteSpace(App.Config.MainConfig.Customize.AccentColor))
                {
                    ThemeManager.Current.ChangeThemeColorScheme(System.Windows.Application.Current, App.Config.MainConfig.Customize.AccentColor);
                }
            }
            if (e.PropertyName == "AppTheme")
            {
                if (!string.IsNullOrWhiteSpace(App.Config.MainConfig.Customize.AppTheme))
                {
                    ThemeManager.Current.ChangeThemeBaseColor(System.Windows.Application.Current, App.Config.MainConfig.Customize.AppTheme);
                }
            }
        }

        private async void SettingPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedSettingVersion")
            {
                await ChangeSettingVersion();
            }
            if (e.PropertyName == "SelectedJava")
            {
                ChangeJava();
            }
        }

        private async Task ChooseJava()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "选择Java",
                Filter = "Java应用程序(无窗口)|javaw.exe|Java应用程序(含窗口)|java.exe|Windows可执行文件|*.exe",
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Java java = await Java.GetJavaInfoAsync(dialog.FileName);
                if (java != null)
                {
                    MainConfig.Environment.JavaPath = java.Path;
                    SelectedJavaInfo = string.Format("Java版本：{0}，位数：{1}", java.Version, java.Arch);
                }
                else
                {
                    MainConfig.Environment.JavaPath = dialog.FileName;
                    await mainWindowVM.ShowMessageAsync("选择的Java无法正确获取信息", "请确认您选择的是正确的Java应用");
                }
            }
        }

        private void ChooseGameDir()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog()
            {
                Description = "选择游戏运行根目录",
                ShowNewFolderButton = true
            };
            var result = dialog.ShowDialog();
            if (result == DialogResult.Cancel)
            {
                return;
            }
            else
            {
                App.Config.MainConfig.Environment.GamePath = dialog.SelectedPath.Trim();
            }
        }

        private async Task ChangeSettingVersion()
        {
            if (SelectedSettingVersion != null)
            {
                VersionOptions.Clear();
                List<VersionOption> versionOptions = await GameHelper.GetOptionsAsync(App.Handler, SelectedSettingVersion);
                if (versionOptions != null)
                {
                    foreach (var item in versionOptions)
                    {
                        VersionOptions.Add(item);
                    }
                }
            }
            else
            {
                VersionOptions.Clear();
            }
        }

        private void ChangeJava()
        {
            if (SelectedJava != null)
            {
                SelectedJavaInfo = string.Format("Java版本：{0}，位数：{1}", SelectedJava.Version, SelectedJava.Arch);
            }
            else
            {
                SelectedJavaInfo = null;
            }
        }

        private void Download_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CheckDownloadFileHash")
            {
                App.NetHandler.Downloader.CheckFileHash = App.Config.MainConfig.Net.CheckDownloadFileHash;
            }
            if (e.PropertyName == "DownloadSource")
            {
                if (App.NetHandler.Downloader.MirrorList == null)
                {
                    App.NetHandler.Downloader.MirrorList = new List<IDownloadableMirror>();
                }
                App.NetHandler.Mirrors.DownloadableMirrorList.Clear();
                switch (App.Config.MainConfig.Net.DownloadSource)
                {
                    case DownloadSource.Auto:
                        App.NetHandler.Mirrors.DownloadableMirrorList.Add(App.NetHandler.Mirrors.GetBmclApi());
                        App.NetHandler.Mirrors.DownloadableMirrorList.Add(App.NetHandler.Mirrors.GetMcbbsApi());
                        break;
                    case DownloadSource.Mojang:
                        break;
                    case DownloadSource.BMCLAPI:
                        App.NetHandler.Mirrors.DownloadableMirrorList.Add(App.NetHandler.Mirrors.GetBmclApi());
                        break;
                    case DownloadSource.MCBBS:
                        App.NetHandler.Mirrors.DownloadableMirrorList.Add(App.NetHandler.Mirrors.GetMcbbsApi());
                        break;
                    default:
                        break;
                }
            }
            if (e.PropertyName == "DownloadThreadsSize")
            {
                App.NetHandler.Downloader.ProcessorSize = App.Config.MainConfig.Net.DownloadThreadsSize;
            }
        }

        private void Environment_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "GamePathType")
            {
                switch (App.Config.MainConfig.Environment.GamePathType)
                {
                    case GameDirEnum.ROOT:
                        App.Handler.GameRootPath = Path.GetFullPath(".minecraft");
                        break;
                    case GameDirEnum.APPDATA:
                        App.Handler.GameRootPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\.minecraft";
                        break;
                    case GameDirEnum.PROGRAMFILES:
                        App.Handler.GameRootPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles) + "\\.minecraft";
                        break;
                    case GameDirEnum.CUSTOM:
                        App.Handler.GameRootPath = App.Config.MainConfig.Environment.GamePath + "\\.minecraft";
                        break;
                    default:
                        throw new ArgumentException("判断游戏目录类型时出现异常，请检查配置文件中GamePathType节点");
                }
            }
            if (e.PropertyName == "VersionIsolation")
            {
                App.Handler.VersionIsolation = App.Config.MainConfig.Environment.VersionIsolation;
            }
        }


    }

    public class AccentColorData
    {
        public string Name { get; set; }

        public Brush BorderColorBrush { get; set; }

        public Brush ColorBrush { get; set; }
    }

    public class AppThemeData : AccentColorData
    { }
}
