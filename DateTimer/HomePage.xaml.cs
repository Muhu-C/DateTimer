using System;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Media;
using HandyControl.Themes;
using MsgBox = HandyControl.Controls.MessageBox;
using DT_Lib;
using Microsoft.Win32;
using System.Net.NetworkInformation;

namespace DateTimer
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class HomePage : System.Windows.Controls.Page
    {
        #region 初始化
        public HomePage()
        {
            InitializeComponent();
            GetTime();
        }
        public static BindContent viewModel = new BindContent(); // HomePage , TimerPage , SettingPage 共用
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Theme.SetSkin(this, Theme.GetSkin(MainWindow.Cur));
            viewModel.TextColor = Brushes.Black;
            DataContext = viewModel;
            if (Theme.GetSkin(MainWindow.Cur) == HandyControl.Data.SkinType.Dark)
                viewModel.TextColor = Brushes.White; // 检测主题并更改文字颜色
            //MsgBox.Show(OtherTools.GetEnvVer());
            //MsgBox.Show(OtherTools.GetWinVer());
            Reload();
        }
        public void Reload() // 重载
        {
            try { TargetText.Text = DateTime.ParseExact(App.ConfigData.Target_Time, "yyyy MM dd", null).ToString("yyyy/MM/dd"); }
            catch (FormatException) { TargetText.Text = "未配置"; }
            LoadNotice();
        }
        /// <summary>
        /// 加载公告
        /// </summary>
        public void LoadNotice()
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    Notice.Text = App.Notice_Text;
                    LinkAdd.Text = App.FNoticeUrl;
                }));
            }
            catch (Exception ex) { App.Error(ex.Message, App.ErrorType.UnknownError, ex, false); }
        }

        /// <summary>
        /// 通过循环异步获取当前时间
        /// </summary>
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
            MainWindow.Cur.ContentFrame.Navigate(App.Setting);
            MainWindow.Cur.SettingButton.IsSelected = true;
            MainWindow.Cur.HomeButton.IsSelected = false;
        }
        #endregion
        #region 显示/隐藏窗口
        private void ShowTimeTable_Click(object sender, RoutedEventArgs e)
        {
            if (!App.Timer.IsVisible)
            {
                App.Timer.Show();
                //App.Timer.Reload();
                ShowTimeTable.Style = FindResource("ButtonWarning") as Style;
                ShowTimeTable.Content = "隐藏时间表";
            }
            else if (App.Timer.IsVisible)
            {
                App.Timer.Hide();
                ShowTimeTable.Style = FindResource("ButtonSuccess") as Style;
                ShowTimeTable.Content = "显示时间表";
            }
        }
        #endregion
    }
}
