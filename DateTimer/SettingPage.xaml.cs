using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MsgBox = HandyControl.Controls.MessageBox;
using DT_Lib;
using Newtonsoft.Json;
using HandyControl.Themes;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using System.Reflection;

namespace DateTimer
{
    /// <summary>
    /// 设置页面的交互逻辑
    /// </summary>
    public partial class SettingPage : Page
    {
        public SettingPage()
        {
            Console.WriteLine("SettingPage: Load");
            InitializeComponent();
            DataContext = HomePage.viewModel; // 使用 HomePage 的 BindingContent
        }
        private void Page_Loaded(object sender, RoutedEventArgs e) // 页面加载
        {
            AboutText.Text = App.About;
            Reload();
            Theme.SetSkin(this, Theme.GetSkin(MainWindow.Cur));
            if (Theme.GetSkin(MainWindow.Cur) == HandyControl.Data.SkinType.Dark) { BiliImage.Source = new BitmapImage(new Uri("pack://application:,,,/Icon/bilibili-w.png")); GithubImage.Source = new BitmapImage(new Uri("pack://application:,,,/Icon/github-w.png")); }
            else { BiliImage.Source = new BitmapImage(new Uri("pack://application:,,,/Icon/bilibili-d.png")); GithubImage.Source = new BitmapImage(new Uri("pack://application:,,,/Icon/github-d.png")); }

        }
        public void Reload() // 页面重载
        {
            TBTimerConfig.Text = App.ConfigData.Timetable_File; // 时间表文件
            try { TTime.SelectedDate = DateTime.ParseExact(App.ConfigData.Target_Time, "yyyy MM dd", null); } // 尝试设定目标时间
            catch { TTime.SelectedDate = DateTime.Now; } // 错误时将时间设为当天
            if (App.ConfigData.Target_Type != "NULL") TName.Text = App.ConfigData.Target_Type; // 设置目标事件名
        }

        private void ChangeTheme_Click(object sender, RoutedEventArgs e) // 更改主题并重启程序
        {
            int theme;
            if (Theme.GetSkin(MainWindow.Cur) == HandyControl.Data.SkinType.Default)
                theme = 0;
            else theme = 1;
            appconfig NewConfig = new appconfig // 重建配置文件
            {
                Theme = theme,
                Target_Time = App.ConfigData.Target_Time,
                Target_Type = App.ConfigData.Target_Type,
                Timetable_File = App.ConfigData.Timetable_File
            };
            FileProcess.WriteFile(TimeTable.Json_Optimization(JsonConvert.SerializeObject(NewConfig)), App.configPath); // 流写入 json 文件
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Environment.Exit(0);
        }
        private void BTTimerConfig_Click(object sender, RoutedEventArgs e) // 更改时间表位置
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "时间表专用配置文件|*.json";
            if(openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (openFileDialog.FileName.Contains(AppDomain.CurrentDomain.BaseDirectory))
                {
                    string a = openFileDialog.FileName.Substring(AppDomain.CurrentDomain.BaseDirectory.Length, openFileDialog.FileName.Length - AppDomain.CurrentDomain.BaseDirectory.Length);
                    if (a != "Config\\config.json")
                    {
                        appconfig NewConfig = new appconfig // 重建配置文件
                        {
                            Theme = App.ConfigData.Theme,
                            Target_Time = App.ConfigData.Target_Time,
                            Target_Type = App.ConfigData.Target_Type,
                            Timetable_File = a
                        };
                        try
                        {
                            TimeTable.GetTimetables(a);
                            FileProcess.WriteFile(TimeTable.Json_Optimization(JsonConvert.SerializeObject(NewConfig)), App.configPath); // 流写入 json 文件
                            App.LoadConfig(); // 重新加载 json
                            MainWindow.Reload();
                            TimeTipIcon.Text = "\uE73E";
                            TimeTip.Text = "更改成功";
                        }
                        catch (JsonSerializationException)
                        {
                            MsgBox.Error("文件格式错误! ", "用户交互错误");
                            TimeTipIcon.Text = "\uE783";
                            TimeTip.Text = "文件格式错误";
                        }
                    }
                    else
                    {
                        MsgBox.Error("文件选择错误！");
                        TimeTipIcon.Text = "\uE783";
                        TimeTip.Text = "文件格式错误";
                    }
                }
                else
                {
                    MsgBox.Error("文件须在程序目录内！");
                    TimeTipIcon.Text = "\uE783";
                    TimeTip.Text = "文件须在程序目录内";
                }
            }
        }
        private void TTime_SelectedDateChanged(object sender, SelectionChangedEventArgs e) // 更改目标时间
        {
            string DateStr = TimeConverter.DateInt2Str(TTime.SelectedDate.Value.Year, TTime.SelectedDate.Value.Month, TTime.SelectedDate.Value.Day); // 把时间转为字符串
            appconfig NewConfig = new appconfig // 重建配置文件
            {
                Theme = App.ConfigData.Theme,
                Target_Time = DateStr,
                Target_Type = App.ConfigData.Target_Type,
                Timetable_File = App.ConfigData.Timetable_File
            };
            FileProcess.WriteFile(TimeTable.Json_Optimization(JsonConvert.SerializeObject(NewConfig)), App.configPath); // 流写入 json 文件
            App.LoadConfig(); // 重新加载 json
            MainWindow.Reload();
        }
        private void TName_TextChanged(object sender, TextChangedEventArgs e) // 更改目标事件名
        {
            string Name = TName.Text;
            if (!string.IsNullOrEmpty(Name))
            {
                TipIcon.Text = string.Empty;
                Tip.Text = string.Empty;
                appconfig NewConfig = new appconfig // 重建配置文件
                {
                    Theme = App.ConfigData.Theme,
                    Target_Time = App.ConfigData.Target_Time,
                    Target_Type = Name,
                    Timetable_File = App.ConfigData.Timetable_File
                };
                FileProcess.WriteFile(TimeTable.Json_Optimization(JsonConvert.SerializeObject(NewConfig)), App.configPath); // 流写入 json 文件
                App.LoadConfig(); // 重新加载 json
                MainWindow.Reload();
            }
            else
            {
                TipIcon.Text = "\uE783";
                Tip.Text = "未填写内容";
            }
        }

        private void GotoGithubIssue(object sender, RoutedEventArgs e) { System.Diagnostics.Process.Start("https://github.com/Muhu-C/DateTimer/issues"); }
        private void GotoGiteeIssue(object sender, RoutedEventArgs e) { System.Diagnostics.Process.Start("https://gitee.com/zzhkjf/DateTimer/issues"); }
        private void GotoGithub(object sender, RoutedEventArgs e) { System.Diagnostics.Process.Start("https://github.com/Muhu-C/"); }
        private void GotoBilibili(object sender, RoutedEventArgs e) { System.Diagnostics.Process.Start("https://space.bilibili.com/1469137723"); }
        private void CopySystemReport(object sender, RoutedEventArgs e) // 生成系统报告
        {
            string Report = 
                  "生成时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") 
                + "\n系统版本: " + OtherTools.GetWinVer()
                + "\n系统位数: " + OtherTools.GetBit().ToString() 
                + "\n运行时版本: " + OtherTools.GetEnvVer()
                + "\nDateTimer 版本: " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            MessageBoxResult r = MsgBox.Show(Report + "\n是否复制到剪贴板？", "系统报告", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
            if(r == MessageBoxResult.Yes) Clipboard.SetDataObject(Report);
        }
    }
}
