using System;
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

namespace DateTimer
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class HomePage : System.Windows.Controls.Page
    {
        public HomePage()
        {
            InitializeComponent();
        }
        public static MyViewModel viewModel = new MyViewModel();
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.MyVariable = Brushes.Black;
            DataContext = viewModel;
            if (Theme.GetSkin(MainWindow.Cur) == HandyControl.Data.SkinType.Dark)
                viewModel.MyVariable = Brushes.White; // 检测主题并更改文字颜色
            Reload();
            GetTime();
        }
        public void Reload() // 重载
        {
            try { TargetText.Text = DateTime.ParseExact(App.ConfigData.Target_Time, "yyyy MM dd", null).ToString("yyyy/MM/dd"); }
            catch { TargetText.Text = "未配置"; }
            LoadNotice();
        }
        public void LoadNotice()
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    Notice.Text = App.Notice_Text;
                }));
            }
            catch(Exception ex)
            {
                App.Error(ex.Message, App.ErrorType.UnknownError, false);
            }
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
                    string time = DateTime.Now.ToString("G");
                    await Dispatcher.BeginInvoke(new Action(delegate
                    {
                        TimeText.Text = time;
                    }));
                    await Task.Delay(1000);
                }
            });
        }

        /// <summary>
        /// Config/App.json 获取目标时间
        /// </summary>
        private void GetTargetTime()
        {
            // 未完成，读取Config/App.json
        }

        private void GoToSetting_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Cur.ContentFrame.Navigate(App.Setting);
            MainWindow.Cur.SettingButton.IsSelected = true;
            MainWindow.Cur.HomeButton.IsSelected = false;
        }

        #region 显示/隐藏窗口
        private void HideTimeTable_Click(object sender, RoutedEventArgs e)
        {
            if (App.Timer.IsVisible)
                App.Timer.Hide();
            else if (App.Timer.Visibility == Visibility.Hidden) return;
            else
            {
                App.Timer = new TimerWindow();
                App.Timer.Hide();
            }
        }
        private void ShowTimeTable_Click(object sender, RoutedEventArgs e)
        {
            if (App.Timer.Visibility == Visibility.Hidden || App.Timer.IsVisible)
            {
                try { App.Timer.Show(); }
                catch
                {
                    App.Timer = new TimerWindow();
                    App.Timer.Show();
                }
            }
            else
            {
                App.Timer = new TimerWindow();
                App.Timer.Show();
            }
        }
        #endregion


    }
    public class MyViewModel : INotifyPropertyChanged // 通过 Foreground Binding 实时设置页面文本颜色
    {
        private Brush myVariable;

        public Brush MyVariable
        {
            get { return myVariable; }
            set
            {
                if (myVariable != value)
                {
                    myVariable = value;
                    OnPropertyChanged("MyVariable");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
