using System;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Media;
using HandyControl.Themes;
using MsgBox = HandyControl.Controls.MessageBox;
using System.Linq;
using System.Net;
using System.Text;
using System.Reflection;

namespace DateTimer.View
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class HomePage : System.Windows.Controls.Page
    {
        #region 初始化
        public static BindContent viewModel = new BindContent(); // HomePage , TimerPage , SettingPage 共用

        public HomePage()
        {
            // 初始化
            InitializeComponent();
            viewModel.VersionTxt = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            DataContext = viewModel;
            GetTime();

            // 获取公告
            LoadNotice();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LogTool.WriteLog("主页 -> 加载", LogTool.LogType.Info);
            // 获取主题
            MainWindow mw = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;
            Theme.SetSkin(this, Theme.GetSkin(mw));

            // 设置主题
            viewModel.TextColor = Brushes.Black; 
            if (Theme.GetSkin(mw) == HandyControl.Data.SkinType.Dark) viewModel.TextColor = Brushes.White; // 检测主题并更改文字颜色

            // 加载配置文件
            Reload();
        }

        public void Reload()
        {
            LogTool.WriteLog("主页 -> 获取配置", LogTool.LogType.Info);
            // 获取目标时间
            try 
            {
                TargetText.Text = DateTime.ParseExact(App.ConfigData.Target_Time, "yyyy MM dd", null).ToString("yyyy/MM/dd");
            }
            catch (FormatException) 
            {
                TargetText.Text = "未配置";
            }
        }

        /// <summary> 加载公告 </summary>
        public async void LoadNotice()
        {
            LogTool.WriteLog("主页 -> 获取公告", LogTool.LogType.Info);
            string FNoticeUrl = string.Empty;
            string Notice_Text = string.Empty;

            try
            {
                await Task.Run(() =>
                {
                    WebClient webClient = new WebClient { Encoding = Encoding.UTF8 };
                    string url = Utils.NetTool.Pings(App.NoticeUrl);

                    switch (url.Split('/')[2]) // 显示的公告地址
                    {
                        case "gitee.com": FNoticeUrl = "Gitee Raw"; break;
                        case "raw.gitmirror.com": FNoticeUrl = "Gitmirror Raw"; break;
                        case "mirror.ghproxy.com": FNoticeUrl = "GitProxy"; break;
                        case "raw.githubusercontent.com": FNoticeUrl = "Github Raw"; break;
                        default: break;
                    }

                    Notice_Text = webClient.DownloadString(url);
                    webClient.Dispose();
                });
                Notice.Text = Notice_Text;
                LinkAdd.Text = FNoticeUrl;
            }
            catch (Exception ex) { Notice_Text = $"公告接收失败\n请检查网络\n或联系程序作者 MC118CN\n加载错误: {ex.Message}"; }
        }

        /// <summary> 通过循环异步获取当前时间 </summary>
        private async void GetTime()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    await Dispatcher.BeginInvoke(new Action(delegate { TimeText.Text = DateTime.Now.ToString("yyyy/MM/dd ddd"); }));
                    await Task.Delay(10000);
                }
            });
        }

        private void GoToSetting_Click(object sender, RoutedEventArgs e)
        {
            // 切换到设置页面
            (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow).ContentFrame.Navigate((Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow).Setting);
            (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow).SettingButton.IsSelected = true;
            (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow).HomeButton.IsSelected = false;
        }

        #endregion

        #region 显示/隐藏窗口

        private void ShowTimeTable_Click(object sender, RoutedEventArgs e)
        {
            if (!(Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow).Timer.IsVisible)
            {
                LogTool.WriteLog("主页 -> 显示时间表", LogTool.LogType.Info);
                // 显示窗口
                (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow).Timer.Show();
                ShowTimeTable.Style = FindResource("ButtonWarning") as Style;
                ShowTimeTable.Content = "隐藏时间表";
            }
            else if ((Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow).Timer.IsVisible)
            {
                LogTool.WriteLog("主页 -> 隐藏时间表", LogTool.LogType.Info);
                // 隐藏窗口
                (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow).Timer.Hide();
                ShowTimeTable.Style = FindResource("ButtonSuccess") as Style;
                ShowTimeTable.Content = "显示时间表";
            }
        }
        #endregion
    }
}
