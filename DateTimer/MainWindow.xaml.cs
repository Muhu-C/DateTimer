using System;
using System.Windows;
using HandyControl.Controls;
using HandyControl.Themes;
using DateTimer.View;
using MsgBox = HandyControl.Controls.MessageBox;

namespace DateTimer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : HandyControl.Controls.Window
    {
        // 对 View 中页面的实例化
        public HomePage Home = new HomePage();

        public SettingPage Setting = new SettingPage();

        public TimerPage TimerPg = new TimerPage();

        public TimerWindow Timer = new TimerWindow();

        #region 窗口初始化与关闭操作
        public MainWindow()
        {
            InitializeComponent();

            ShowHideButtonIcon.Text = "\uE727";
            ShowWindowButton.Header = "隐藏主窗口";
            ContentFrame.Navigate(Home);

            App.LoadConfig();
            if (App.ConfigData.Theme == 0) Theme.SetSkin(this, HandyControl.Data.SkinType.Dark);
            else Theme.SetSkin(this, HandyControl.Data.SkinType.Default);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) // 关闭或隐藏
        {
            MessageBoxResult messageBoxResult = MsgBox.Show("是否关闭程序\n按\"是\"关闭程序\n按\"否\"隐藏到任务栏", "提示", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes) 
                Environment.Exit(0);

            else if (messageBoxResult == MessageBoxResult.No)
            {
                e.Cancel = true;
                Hide();
                ShowHideButtonIcon.Text = "\uE737";
                ShowWindowButton.Header = "显示主窗口";
            }
            else e.Cancel = true;
        }

        #endregion

        #region 任务栏图标事件

        private void MenuItemA_Click(object sender, RoutedEventArgs e) 
        {
            if (Visibility == Visibility.Visible) // 隐藏
            {
                Console.WriteLine("MainWindow.Hide");
                Hide();
                ShowHideButtonIcon.Text = "\uE737";
                ShowWindowButton.Header = "显示主窗口";
            }
            else // 显示
            {
                Console.WriteLine("MainWindow.Show");
                Show();
                ShowHideButtonIcon.Text = "\uE727";
                ShowWindowButton.Header = "隐藏主窗口";
            }
        } // 打开或者关闭主窗口

        private void MenuItemB_Click(object sender, RoutedEventArgs e) { Environment.Exit(0); } // 关闭程序

        private void notifyIcon_Click(object sender, RoutedEventArgs e) 
        {
            if (!Timer.IsVisible)
            {
                Console.WriteLine("TimeTable.Show");
                Timer.Show();
                Timer.Reload();
                Home.ShowTimeTable.Style = FindResource("ButtonWarning") as Style;
                Home.ShowTimeTable.Content = "隐藏时间表";
            }
            else if (Timer.IsVisible)
            {
                Console.WriteLine("TimeTable.Hide");
                Timer.Hide();
                Home.ShowTimeTable.Style = FindResource("ButtonSuccess") as Style;
                Home.ShowTimeTable.Content = "显示时间表";
            }
        } // 显示或者隐藏时间表窗口

        #endregion

        private void SideMenu_SelectionChanged(object sender, HandyControl.Data.FunctionEventArgs<object> e) // 选中菜单内容
        {
            SideMenuItem s = e.Info as SideMenuItem;

            if (s != null) // 正常运行
            {
                try
                {
                    string tag = s.Tag.ToString();
                    switch (tag)
                    {
                        case "Home":
                            ContentFrame.Navigate(Home);
                            SettingButton.IsSelected = false;
                            TableButton.IsSelected = false;
                            break;
                        case "Setting":
                            ContentFrame.Navigate(Setting);
                            HomeButton.IsSelected = false;
                            TableButton.IsSelected = false;
                            break;
                        case "Edit":
                            ContentFrame.Navigate(TimerPg);
                            HomeButton.IsSelected = false;
                            SettingButton.IsSelected = false;
                            break;
                        default: // 这选的不对
                            App.Error($"你选了什么?\n选中内容为{tag}",App.ErrorType.ProgramError, null, false);
                            break;
                    }
                }
                catch (Exception ex) { App.Error("无", App.ErrorType.UnknownError, ex, true); } // 未知错误的处理
            }
            else // 错误时
                App.Error("菜单内容为 NULL", App.ErrorType.ProgramError, null, false);
        }
    }
}
