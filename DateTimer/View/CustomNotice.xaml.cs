using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DateTimer.View
{
    /// <summary>
    /// CustomNotice.xaml 的交互逻辑
    /// </summary>
    public partial class CustomNotice : HandyControl.Controls.Window
    {
        public BindContent Ctt = new BindContent();
        public string MediaFile = "Data/Media/notice.wav";

        public CustomNotice()
        {
            // 初始化弹出窗口
            InitializeComponent();
            DataContext = Ctt;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.ConfigData.Theme == 0)
            {
                HandyControl.Themes.Theme.SetSkin(this, HandyControl.Data.SkinType.Dark);
                Ctt.NT2Fore = Brushes.White;
            }
            else
            {
                HandyControl.Themes.Theme.SetSkin(this, HandyControl.Data.SkinType.Default);
                Ctt.NT2Fore = Brushes.Black;
            }
            // 显示窗口行为
            Left = SystemParameters.WorkArea.Right - Width;
            Top = SystemParameters.WorkArea.Bottom;
        }


        public async void Init()
        {
            LogTool.WriteLog($"消息 -> 弹出消息", LogTool.LogType.Info);
            Show();
            // 等待窗口加载完毕
            await Task.Run(async () => { await Task.Delay(100); });
            var animation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                To = SystemParameters.WorkArea.Bottom - Height,
            };

            // 弹出窗口
            MediaPlayer player = new MediaPlayer();
            player.Open(new Uri(MediaFile, UriKind.Relative));
            player.Play();
            BeginAnimation(TopProperty, animation);

            // 关闭窗口
            await Task.Run(async() => { await Task.Delay(5000); });
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e) { Close(); }

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            var animation2 = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(0.15)),
                To = SystemParameters.WorkArea.Bottom,
            };
            BeginAnimation(TopProperty, animation2);
            await Task.Run(async () => { await Task.Delay(300); });

            Ctt.NoticeText1 = "";
            Ctt.NoticeText2 = "";
            if (App.ConfigData.Theme == 0) Ctt.NT2Fore = Brushes.White;
            else Ctt.NT2Fore = Brushes.Black;
            Hide();
        }



        public class BindContent : INotifyPropertyChanged // 通过 Foreground Binding 实时设置页面文本颜色
        {
            private Brush ntxt2fore;
            public Brush NT2Fore
            {
                get { return ntxt2fore; }
                set { if (ntxt2fore != value) { ntxt2fore = value; OnPropertyChanged("NT2Fore"); } }
            }

            private string ntxt1;
            public string NoticeText1
            {
                get { return ntxt1; }
                set { if (ntxt1 != value) { ntxt1 = value; OnPropertyChanged("NoticeText1"); } }
            }

            private string ntxt2;
            public string NoticeText2
            {
                get { return ntxt2; }
                set { if (ntxt2 != value) { ntxt2 = value; OnPropertyChanged("NoticeText2"); } }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        }
    }
}
