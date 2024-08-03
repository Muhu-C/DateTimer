using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HandyControl;
using HandyControl.Controls;
using HandyControl.Themes;
using HandyControl.Tools;
using HandyControl.Tools.Extension;
using MsgBox = HandyControl.Controls.MessageBox;

namespace DateTimer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : HandyControl.Controls.Window
    {
        public static MainWindow Cur;
        #region 窗口初始化与关闭操作
        public MainWindow()
        {
            InitializeComponent();
            Cur = this;
            ContentFrame.Navigate(App.Home);
            Closing += Window_Closing;
            ShowHideButtonIcon.Text = "\uE727";
            ShowWindowButton.Header = "隐藏主窗口";
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) // 关闭或隐藏
        {
            App.Timer.isExist = false;
            App.Timer.Close();
            MessageBoxResult messageBoxResult = MsgBox.Show("是否关闭程序\n按\"是\"关闭程序\n按\"否\"隐藏到任务栏", "提示", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes) Environment.Exit(0);
            else if(messageBoxResult == MessageBoxResult.No)
            {
                e.Cancel = true;
                this.Hide();
                ShowWindowButton.Header = "显示主窗口";
            }
            else e.Cancel = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) 
        {
            Reload();
            NotifyIcon notifyIcon = new NotifyIcon();
            notifyIcon.Icon = Icon;
            notifyIcon.Visibility = Visibility.Visible;
            notifyIcon.Show();
        }
        public static void Reload() // 主窗口重载
        {
            try
            {
                if (Cur != null) // 判断主窗口是否存在
                {
                    if (App.ConfigData.Theme == 0) Theme.SetSkin(Cur, HandyControl.Data.SkinType.Dark);
                    else Theme.SetSkin(Cur, HandyControl.Data.SkinType.Default);
                }
            }
            catch(Exception ex){ App.Error("主窗口重载时发现未知错误\n"+ex.Message,App.ErrorType.UnknownError,true); }
        }
        #endregion
        #region 任务栏图标事件
        private void MenuItemA_Click(object sender, RoutedEventArgs e) 
        {
            if (Visibility == Visibility.Visible) // 隐藏
            {
                Hide();
                ShowHideButtonIcon.Text = "\uE737";
                ShowWindowButton.Header = "显示主窗口";
            }
            else // 显示
            {
                Show();
                ShowHideButtonIcon.Text = "\uE727";
                ShowWindowButton.Header = "隐藏主窗口";
            }
        } // 打开或者关闭主窗口
        private void MenuItemB_Click(object sender, RoutedEventArgs e) { Environment.Exit(0); } // 关闭程序
        private void notifyIcon_Click(object sender, RoutedEventArgs e) 
        {
            if (App.Timer.IsVisible) App.Timer.Hide();
            else if(App.Timer.Visibility == Visibility.Hidden) App.Timer.Show();
            else
            {
                App.Timer = new TimerWindow();
                App.Timer.Show();
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
                            ContentFrame.Navigate(App.Home);
                            SettingButton.IsSelected = false;
                            break;
                        case "Setting":
                            ContentFrame.Navigate(App.Setting);
                            HomeButton.IsSelected = false;
                            break;
                        default: // 这选的不对
                            App.Error("选中内容为 "+tag,App.ErrorType.ProgramError,false);
                            break;
                    }
                }
                catch (Exception ex) { App.Error( "切换页面时出现错误\n"+ex.ToString(), App.ErrorType.UnknownError, true); } // 未知错误的处理
            }
            else // 错误时
            {
                System.Diagnostics.Process.Start(App.FeedBackUrl);
                App.Error("菜单内容为 NULL", App.ErrorType.ProgramError, false);
            }
        }
    }
}
