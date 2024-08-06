﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HandyControl;
using HandyControl.Themes;
using HandyControl.Tools;
using MsgBox = HandyControl.Controls.MessageBox;
using DT_Lib;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Collections.ObjectModel;

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
            viewModel.TextColor = Brushes.Black;
            DataContext = viewModel;
            if (Theme.GetSkin(MainWindow.Cur) == HandyControl.Data.SkinType.Dark)
                viewModel.TextColor = Brushes.White; // 检测主题并更改文字颜色
            Theme.SetSkin(this, Theme.GetSkin(MainWindow.Cur));
            Reload();
        }
        public void Reload() // 重载
        {
            try
            {
                string t = DateTime.ParseExact(App.ConfigData.Target_Time, "yyyy MM dd", null).ToString("yyyy/MM/dd");
                TargetText.Text = t;
            }
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
            catch (Exception ex) { App.Error(ex.Message, App.ErrorType.UnknownError, false); }
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
                    string time = DateTime.Now.ToString("yyyy/MM/dd ddd");
                    await Dispatcher.BeginInvoke(new Action(delegate { TimeText.Text = time; }));
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
    public class BindContent : INotifyPropertyChanged // 通过 Foreground Binding 实时设置页面文本颜色
    {
        private Brush textColor;
        public Brush TextColor
        {
            get { return textColor; }
            set { if (textColor != value) { textColor = value; OnPropertyChanged("TextColor"); } }
        }
        private ObservableCollection<TimeTable.TableEntry> tables;
        public ObservableCollection<TimeTable.TableEntry> TableEntries
        {
            get { return tables; }
            set { tables = value; OnPropertyChanged("TableEntries"); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
