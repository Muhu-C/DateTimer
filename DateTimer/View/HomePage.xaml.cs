using System;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Media;
using HandyControl.Themes;
using MsgBox = HandyControl.Controls.MessageBox;
using DT_Lib;
using System.Linq;
using System.Net;
using System.Text;

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
            DataContext = viewModel;
            GetTime();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取主题
            MainWindow mw = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;
            Theme.SetSkin(this, Theme.GetSkin(mw));

            // 设置主题
            viewModel.TextColor = Brushes.Black; 
            if (Theme.GetSkin(mw) == HandyControl.Data.SkinType.Dark) viewModel.TextColor = Brushes.White; // 检测主题并更改文字颜色

            // 获取公告
            LoadNotice();
        }

        public void Reload()
        {
            // 获取目标时间
            try { TargetText.Text = DateTime.ParseExact(App.ConfigData.Target_Time, "yyyy MM dd", null).ToString("yyyy/MM/dd"); }
            catch (FormatException) { TargetText.Text = "未配置"; }
        }

        /// <summary> 加载公告 </summary>
        public async void LoadNotice()
        {
            string FNoticeUrl = string.Empty;
            string Notice_Text = string.Empty;
            try
            {
                await Task.Run(() =>
                {
                    WebClient webClient = new WebClient { Encoding = Encoding.UTF8 };
                    string url = NetTool.Pings(App.NoticeUrl);
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
            MainWindow mw = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;
            mw.ContentFrame.Navigate(mw.Setting);
            mw.SettingButton.IsSelected = true;
            mw.HomeButton.IsSelected = false;
        }

        #endregion

        #region 显示/隐藏窗口
        private void ShowTimeTable_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mw = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;
            if (!mw.Timer.IsVisible)
            {
                // 显示窗口
                mw.Timer.Show();
                mw.Timer.Reload();
                ShowTimeTable.Style = FindResource("ButtonWarning") as Style;
                ShowTimeTable.Content = "隐藏时间表";
            }
            else if (mw.Timer.IsVisible)
            {
                // 隐藏窗口
                mw.Timer.Hide();
                ShowTimeTable.Style = FindResource("ButtonSuccess") as Style;
                ShowTimeTable.Content = "显示时间表";
            }
        }
        #endregion
    }
}
