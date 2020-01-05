using NsisoLauncher.Config;
using NsisoLauncher.Views.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NsisoLauncherCore.Modules;
using Version = NsisoLauncherCore.Modules.Version;

namespace NsisoLauncher.Views.Controls
{
    /// <summary>
    /// MainPanelControl.xaml 的交互逻辑
    /// </summary>
    public partial class MainPanelControl : UserControl
    {

        #region Propdp(Command)
        public ICommand LaunchCommand
        {
            get { return (ICommand)GetValue(LaunchCommandProperty); }
            set { SetValue(LaunchCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LaunchCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LaunchCommandProperty =
            DependencyProperty.Register("LaunchCommand", typeof(ICommand), typeof(MainPanelControl), new PropertyMetadata(null));



        public ICommand OpenSettingCommand
        {
            get { return (ICommand)GetValue(OpenSettingCommandProperty); }
            set { SetValue(OpenSettingCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OpenSettingCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpenSettingCommandProperty =
            DependencyProperty.Register("OpenSettingCommand", typeof(ICommand), typeof(MainPanelControl), new PropertyMetadata(null));




        public ICommand OpenDownloadingCommand
        {
            get { return (ICommand)GetValue(OpenDownloadingCommandProperty); }
            set { SetValue(OpenDownloadingCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OpenDownloadingCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpenDownloadingCommandProperty =
            DependencyProperty.Register("OpenDownloadingCommand", typeof(ICommand), typeof(MainPanelControl), new PropertyMetadata(null));
        #endregion

        #region Propdp(Sources)
        public IEnumerable<KeyValuePair<string, UserNode>> UsersSource
        {
            get { return (IEnumerable<KeyValuePair<string, UserNode>>)GetValue(UsersSourceProperty); }
            set { SetValue(UsersSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UsersSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UsersSourceProperty =
            DependencyProperty.Register("UsersSource", typeof(IEnumerable<KeyValuePair<string, UserNode>>), typeof(MainPanelControl), new PropertyMetadata(null));



        public IEnumerable<KeyValuePair<string, AuthenticationNode>> AuthNodesSource
        {
            get { return (IEnumerable<KeyValuePair<string, AuthenticationNode>>)GetValue(AuthNodesSourceProperty); }
            set { SetValue(AuthNodesSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AuthNodesSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AuthNodesSourceProperty =
            DependencyProperty.Register("AuthNodesSource", typeof(IEnumerable<KeyValuePair<string, AuthenticationNode>>), typeof(MainPanelControl), new PropertyMetadata(null));



        public IEnumerable<Version> VersionsSource
        {
            get { return (IEnumerable<Version>)GetValue(VersionsSourceProperty); }
            set { SetValue(VersionsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VersionsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VersionsSourceProperty =
            DependencyProperty.Register("VersionsSource", typeof(IEnumerable<Version>), typeof(MainPanelControl), new PropertyMetadata(null));


        #endregion

        #region Propdp(Selection)
        public Version SelectedVersion
        {
            get { return (Version)GetValue(SelectedVersionProperty); }
            set { SetValue(SelectedVersionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedVersion.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedVersionProperty =
            DependencyProperty.Register("SelectedVersion", typeof(Version), typeof(MainPanelControl), new PropertyMetadata(null));



        public KeyValuePair<string, UserNode>? SelectedUserNode
        {
            get { return (KeyValuePair<string, UserNode>?)GetValue(SelectedUserNodeProperty); }
            set { SetValue(SelectedUserNodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedUser.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedUserNodeProperty =
            DependencyProperty.Register("SelectedUserNode", typeof(KeyValuePair<string, UserNode>?), typeof(MainPanelControl), new PropertyMetadata(null));



        public KeyValuePair<string, AuthenticationNode>? SelectedAuthNode
        {
            get { return (KeyValuePair<string, AuthenticationNode>?)GetValue(SelectedAuthNodeProperty); }
            set { SetValue(SelectedAuthNodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedAuthNode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedAuthNodeProperty =
            DependencyProperty.Register("SelectedAuthNode", typeof(KeyValuePair<string, AuthenticationNode>?), typeof(MainPanelControl), new PropertyMetadata(null));



        public string UserNameText
        {
            get { return (string)GetValue(UserNameTextProperty); }
            set { SetValue(UserNameTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UserNameText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserNameTextProperty =
            DependencyProperty.Register("UserNameText", typeof(string), typeof(MainPanelControl), new PropertyMetadata(null));



        #endregion

        #region Propdp(data)


        public int DownloadTaskCount
        {
            get { return (int)GetValue(DownloadTaskCountProperty); }
            set { SetValue(DownloadTaskCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DownloadTaskCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DownloadTaskCountProperty =
            DependencyProperty.Register("DownloadTaskCount", typeof(int), typeof(MainPanelControl), new PropertyMetadata(0));


        #endregion

        //public event Action<object, LaunchEventArgs> Launch;

        //private ObservableCollection<KeyValuePair<string, UserNode>> userList = new ObservableCollection<KeyValuePair<string, UserNode>>();
        //private ObservableCollection<KeyValuePair<string, AuthenticationNode>> authNodeList = new ObservableCollection<KeyValuePair<string, AuthenticationNode>>();
        //private ObservableCollection<NsisoLauncherCore.Modules.Version> versionList = new ObservableCollection<NsisoLauncherCore.Modules.Version>();

        public MainPanelControl()
        {
            InitializeComponent();
        }

        //private UserNode GetSelectedAuthNode()
        //{
        //    string userID = (string)userComboBox.SelectedValue;
        //    if ((userID != null) && App.config.MainConfig.User.UserDatabase.ContainsKey(userID))
        //    {
        //        return App.config.MainConfig.User.UserDatabase[userID];
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //public async void Refresh()
        //{
        //    try
        //    {
        //        //更新用户列表
        //        userList.Clear();
        //        foreach (var item in App.config.MainConfig.User.UserDatabase)
        //        {
        //            userList.Add(item);
        //        }

        //        //更新验证模型列表
        //        authNodeList.Clear();
        //        authNodeList.Add(new KeyValuePair<string, AuthenticationNode>("offline", new AuthenticationNode()
        //        {
        //            AuthType = AuthenticationType.OFFLINE,
        //            Name = App.GetResourceString("String.MainWindow.Auth.Offline")
        //        }));
        //        authNodeList.Add(new KeyValuePair<string, AuthenticationNode>("mojang", new AuthenticationNode()
        //        {
        //            AuthType = AuthenticationType.MOJANG,
        //            Name = App.GetResourceString("String.MainWindow.Auth.Mojang")
        //        }));
        //        foreach (var item in App.config.MainConfig.User.AuthenticationDic)
        //        {
        //            authNodeList.Add(item);
        //        }

        //        //更新版本列表
        //        List<NsisoLauncherCore.Modules.Version> versions = await App.handler.GetVersionsAsync();
        //        versionList.Clear();
        //        foreach (var item in versions)
        //        {
        //            versionList.Add(item);
        //        }

        //        this.launchVersionCombobox.Text = App.config.MainConfig.History.LastLaunchVersion;
        //        if ((App.config.MainConfig.History.SelectedUserNodeID != null) &&
        //            (App.config.MainConfig.User.UserDatabase.ContainsKey(App.config.MainConfig.History.SelectedUserNodeID)))
        //        {
        //            this.userComboBox.SelectedValue = App.config.MainConfig.History.SelectedUserNodeID;
        //            //await RefreshIcon();
        //        }

        //        //锁定验证模型处理
        //        if (!string.IsNullOrWhiteSpace(App.config.MainConfig.User.LockAuthName))
        //        {
        //            if (App.config.MainConfig.User.AuthenticationDic.ContainsKey(App.config.MainConfig.User.LockAuthName))
        //            {
        //                authTypeCombobox.SelectedValue = App.config.MainConfig.User.LockAuthName;
        //                authTypeCombobox.IsEnabled = false;
        //            }
        //        }
        //        else
        //        {
        //            authTypeCombobox.IsEnabled = true;
        //        }

        //        App.logHandler.AppendDebug("启动器主窗体数据重载完毕");
        //    }
        //    catch (Exception e)
        //    {
        //        App.CatchAggregateException(this, new AggregateExceptionArgs() { AggregateException = new AggregateException("启动器致命错误", e) });
        //    }
        //}

        //todo 添加头像支持
        //public async Task RefreshIcon()
        //{
        //    //头像自定义显示皮肤
        //    UserNode node = GetSelectedAuthNode();
        //    if (node == null)
        //    {
        //        return;
        //    }
        //    bool isNeedRefreshIcon = (!string.IsNullOrWhiteSpace(node.SelectProfileUUID)) &&
        //        node.AuthModule == "mojang";
        //    if (isNeedRefreshIcon)
        //    {
        //        await headScul.RefreshIcon(node.SelectProfileUUID);
        //    }
        //}

        #region button click event

        //启动游戏按钮点击
        //private void launchButton_Click(object sender, RoutedEventArgs e)
        //{
        //    //获取启动版本
        //    NsisoLauncherCore.Modules.Version launchVersion = null;
        //    if (launchVersionCombobox.SelectedItem != null)
        //    {
        //        launchVersion = (NsisoLauncherCore.Modules.Version)launchVersionCombobox.SelectedItem;
        //    }

        //    //获取验证方式
        //    AuthenticationNode authNode = null;
        //    string authNodeName = null;
        //    if (authTypeCombobox.SelectedItem != null)
        //    {
        //        KeyValuePair<string, AuthenticationNode> node = (KeyValuePair<string, AuthenticationNode>)authTypeCombobox.SelectedItem;
        //        authNode = node.Value;
        //        authNodeName = node.Key;
        //    }

        //    //获取用户信息
        //    string userName = userComboBox.Text;
        //    string selectedUserUUID = (string)userComboBox.SelectedValue;
        //    bool isNewUser = string.IsNullOrWhiteSpace(selectedUserUUID);
        //    UserNode userNode = null;
        //    if (!string.IsNullOrWhiteSpace(userName))
        //    {
        //        if (!isNewUser)
        //        {
        //            userNode = ((KeyValuePair<string, UserNode>)userComboBox.SelectedItem).Value;
        //        }
        //        else
        //        {
        //            userNode = new UserNode() { AuthModule = authNodeName, UserName = userName };
        //        }
        //    }
        //    else
        //    {
        //        userNode = null;
        //    }


        //    this.Launch?.Invoke(this, new LaunchEventArgs() { AuthNode = authNode, UserNode = userNode, LaunchVersion = launchVersion, IsNewUser = isNewUser });
        //}

        ////下载按钮点击
        //private void downloadButton_Click(object sender, RoutedEventArgs e)
        //{
        //    new DownloadWindow().ShowDialog();
        //    //Refresh();
        //}

        ////配置按钮点击
        //private void configButton_Click(object sender, RoutedEventArgs e)
        //{
        //    new SettingWindow().ShowDialog();
        //    //Refresh();
        //    ((MainWindow)Window.GetWindow(this)).CustomizeRefresh();
        //}


        //private void addAuthButton_Click(object sender, RoutedEventArgs e)
        //{
        //    var a = new SettingWindow();
        //    a.ShowAddAuthModule();
        //    a.ShowDialog();
        //    Refresh();
        //}
        #endregion

        //private /*async*/ void UserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    //await RefreshIcon();
        //}
    }

    public class LaunchEventArgs : EventArgs
    {
        public Version LaunchVersion { get; set; }
        public AuthenticationNode AuthNode { get; set; }
        public UserNode UserNode { get; set; }
        public bool IsNewUser { get; set; }
    }
}