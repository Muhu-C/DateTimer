using System;
using System.Windows;
using System.Windows.Controls;
using MsgBox = HandyControl.Controls.MessageBox;
using Newtonsoft.Json;
using HandyControl.Themes;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using System.Reflection;
using System.Linq;

namespace DateTimer.View
{
    /// <summary>
    /// 设置页面的交互逻辑
    /// </summary>
    public partial class SettingPage : Page
    {
        #region 页面加载
        public SettingPage()
        {
            // 初始化
            InitializeComponent();
            DataContext = HomePage.viewModel; // 使用 HomePage 的 BindingContent
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) // 页面加载
        {
            // 设置主题
            MainWindow mw = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;
            AboutText.Text = App.About;
            Reload();
            Theme.SetSkin(this, Theme.GetSkin(mw));
        }

        public void Reload()
        {
            // 设置初始文本
            TBTimerConfig.Text = App.ConfigData.Timetable_File;
            try { TTime.SelectedDate = DateTime.ParseExact(App.ConfigData.Target_Time, "yyyy MM dd", null); }
            catch { TTime.SelectedDate = DateTime.Now; }
            if (App.ConfigData.Target_Type != "NULL") TName.Text = App.ConfigData.Target_Type;
        }
        #endregion

        #region 设置更改
        private void ChangeTheme_Click(object sender, RoutedEventArgs e)
        {
            // 切换主题
            MainWindow mw = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;
            int theme;
            if (Theme.GetSkin(mw) == HandyControl.Data.SkinType.Default) theme = 0;
            else theme = 1;

            // 写入配置文件
            appconfig NewConfig = new appconfig
            {
                Theme = theme,
                Target_Time = App.ConfigData.Target_Time,
                Target_Type = App.ConfigData.Target_Type,
                Timetable_File = App.ConfigData.Timetable_File
            };
            Utils.FileProcess.WriteFile(Utils.TimeTable.Json_Optimization(JsonConvert.SerializeObject(NewConfig)), App.configPath); // 流写入 json 文件
            App.LoadConfig();

            // 重启程序
            App.AppMutex.Dispose();
            (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow).Hide();
            System.Diagnostics.Process.Start(Assembly.GetExecutingAssembly().Location);
            Environment.Exit(0);
        }

        private void BTTimerConfig_Click(object sender, RoutedEventArgs e) 
        {
            // 更改时间表位置
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "时间表专用配置文件|*.json";

            // 判断路径合法
            if(openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (openFileDialog.FileName.Contains(AppDomain.CurrentDomain.BaseDirectory))
                {
                    string a = openFileDialog.FileName.Substring(AppDomain.CurrentDomain.BaseDirectory.Length, openFileDialog.FileName.Length - AppDomain.CurrentDomain.BaseDirectory.Length);
                    if (a != "Config\\config.json")
                    {
                        // 写入配置文件
                        appconfig NewConfig = new appconfig
                        {
                            Theme = App.ConfigData.Theme,
                            Target_Time = App.ConfigData.Target_Time,
                            Target_Type = App.ConfigData.Target_Type,
                            Timetable_File = a
                        };

                        // 检验配置
                        try
                        {
                            Utils.TimeTable.GetTimetables(a);
                            Utils.FileProcess.WriteFile(Utils.TimeTable.Json_Optimization(JsonConvert.SerializeObject(NewConfig)), App.configPath); // 流写入 json 文件

                            App.LoadConfig(); // 重新加载 json
                            TimeTipIcon.Text = "\uE73E";
                            TimeTip.Text = "更改成功";
                        }
                        catch (JsonSerializationException)
                        {
                            TimeTipIcon.Text = "\uE783";
                            TimeTip.Text = "文件格式错误";
                        }
                        return;
                    }
                }
                TimeTipIcon.Text = "\uE783";
                TimeTip.Text = "文件错误";
            }
        }

        private void TTime_SelectedDateChanged(object sender, SelectionChangedEventArgs e) // 更改目标时间
        {
            string DateStr = Utils.TimeConverter.DateInt2Str(TTime.SelectedDate.Value.Year, TTime.SelectedDate.Value.Month, TTime.SelectedDate.Value.Day); // 把时间转为字符串
            appconfig NewConfig = new appconfig // 重建配置文件
            {
                Theme = App.ConfigData.Theme,
                Target_Time = DateStr,
                Target_Type = App.ConfigData.Target_Type,
                Timetable_File = App.ConfigData.Timetable_File
            };
            Utils.FileProcess.WriteFile(Utils.TimeTable.Json_Optimization(JsonConvert.SerializeObject(NewConfig)), App.configPath); // 流写入 json 文件
            App.LoadConfig(); // 重新加载 json
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
                Utils.FileProcess.WriteFile(Utils.TimeTable.Json_Optimization(JsonConvert.SerializeObject(NewConfig)), App.configPath); // 流写入 json 文件
                App.LoadConfig(); // 重新加载 json
            }
            else
            {
                TipIcon.Text = "\uE783";
                Tip.Text = "未填写内容";
            }
        }
        #endregion

        private void GotoGithubIssue(object sender, RoutedEventArgs e) { System.Diagnostics.Process.Start("https://github.com/Muhu-C/DateTimer/issues"); }
        private void GotoGiteeIssue(object sender, RoutedEventArgs e) { System.Diagnostics.Process.Start("https://gitee.com/zzhkjf/DateTimer/issues"); }
        private void GotoGithub(object sender, RoutedEventArgs e) { System.Diagnostics.Process.Start("https://github.com/Muhu-C/"); }
        private void GotoBilibili(object sender, RoutedEventArgs e) { System.Diagnostics.Process.Start("https://space.bilibili.com/1469137723"); }
        private void CopySystemReport(object sender, RoutedEventArgs e) // 生成系统报告
        {
            string Report = 
                  "生成时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") 
                + "\n系统版本: " + Utils.OtherTools.GetWinVer()
                + "\n系统位数: " + Utils.OtherTools.GetBit().ToString() 
                + "\n运行时版本: " + Utils.OtherTools.GetEnvVer()
                + "\nDateTimer 版本: " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            MessageBoxResult r = MsgBox.Show(Report + "\n是否复制到剪贴板？", "系统报告", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
            if(r == MessageBoxResult.Yes) Clipboard.SetDataObject(Report);
        }
    }
}
