using System;
using System.Windows;
using System.Windows.Controls;
using MsgBox = HandyControl.Controls.MessageBox;
using Newtonsoft.Json;
using HandyControl.Themes;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;

namespace DateTimer.View
{
    /// <summary>
    /// 设置页面的交互逻辑
    /// </summary>
    public partial class SettingPage : Page
    {
        private static DateTime LastChange = DateTime.MinValue;
        private static Appconfig CurrentConfig;

        #region 页面加载
        public SettingPage()
        {
            // 初始化
            InitializeComponent();
            CurrentConfig = new Appconfig();
            DataContext = HomePage.viewModel; // 使用 HomePage 的 BindingContent
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) // 页面加载
        {
            LogTool.WriteLog("设置 -> 加载", LogTool.LogType.Info);
            // 设置主题
            MainWindow mw = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;
            Reload();
            Theme.SetSkin(this, Theme.GetSkin(mw));
        }

        public void Reload()
        {
            LogTool.WriteLog("设置 -> 设置临时配置", LogTool.LogType.Info);
            // 设置临时配置内容
            CurrentConfig.Front_Min = App.ConfigData.Front_Min;
            CurrentConfig.Target_Type = App.ConfigData.Target_Type;
            CurrentConfig.Target_Time = App.ConfigData.Target_Time;
            CurrentConfig.Theme = App.ConfigData.Theme;
            CurrentConfig.Timetable_File = App.ConfigData.Timetable_File;

            // 设置初始文本
            TBTimerConfig.Text = CurrentConfig.Timetable_File;
            TFrontTime.Text = CurrentConfig.Front_Min.ToString();
            try { TTime.SelectedDate = DateTime.ParseExact(CurrentConfig.Target_Time, "yyyy MM dd", null); }
            catch { TTime.SelectedDate = DateTime.Now; }
            if (CurrentConfig.Target_Type != "NULL") TName.Text = CurrentConfig.Target_Type;
        }
        #endregion

        #region 设置更改
        private void ChangeTheme_Click(object sender, RoutedEventArgs e)
        {
            LogTool.WriteLog("设置 -> 切换主题", LogTool.LogType.Info);
            // 切换主题
            MainWindow mw = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;
            int theme;
            if (Theme.GetSkin(mw) == HandyControl.Data.SkinType.Default) theme = 0;
            else theme = 1;

            // 写入配置文件
            App.ConfigData.Theme = theme;
            Utils.FileProcess.WriteFile(Utils.TimeTable.Json_Optimization(JsonConvert.SerializeObject(App.ConfigData)), App.configPath); // 流写入 json 文件

            // 重启程序
            App.AppMutex.Dispose();
            (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow).Hide();
            (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow).Timer.Hide();
            System.Diagnostics.Process.Start(Assembly.GetExecutingAssembly().Location);
            Environment.Exit(0);
        }

        private async void BTTimerConfig_Click(object sender, RoutedEventArgs e) 
        {
            LogTool.WriteLog("设置 -> 设置时间表位置", LogTool.LogType.Info);
            // 更改时间表位置
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "时间表专用配置文件|*.json";

                // 判断路径合法
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (openFileDialog.FileName.Contains(AppDomain.CurrentDomain.BaseDirectory))
                    {
                        string a = openFileDialog.FileName.Substring(AppDomain.CurrentDomain.BaseDirectory.Length, openFileDialog.FileName.Length - AppDomain.CurrentDomain.BaseDirectory.Length);
                        if (a != "Data\\Config\\config.json")
                        {
                            // 写入配置文件
                            try
                            {
                                Utils.TimeTable.GetTimetables(a);
                                CurrentConfig.Timetable_File = a;
                                TimeTipIcon.Text = "\uE73E";
                                TimeTip.Text = "更改成功";
                            }
                            catch
                            {
                                TimeTipIcon.Text = "\uE783";
                                TimeTip.Text = "文件格式错误";
                                LogTool.WriteLog("设置 -> 文件格式错误", LogTool.LogType.Error);
                            }

                            
                            if (CurrentConfig.Timetable_File != App.ConfigData.Timetable_File) IsChangeSaved.Text = "设置未保存";
                            else if (MatchConfig()) IsChangeSaved.Text = string.Empty;

                            // 等待后清空
                            await Task.Run(async () => { await Task.Delay(3000); });
                            TimeTipIcon.Text = "";
                            TimeTip.Text = "";
                            return;
                        }
                    }
                    else
                    {
                        System.IO.File.Copy(openFileDialog.FileName, "Data\\Config\\timetable_cur.json", true);

                        // 写入配置文件 (复制到 Config 文件夹内)
                        try
                        {
                            Utils.TimeTable.GetTimetables("Data\\Config\\timetable_cur.json");
                            CurrentConfig.Timetable_File = "Data\\Config\\timetable_cur.json";
                            TimeTipIcon.Text = "\uE73E";
                            TimeTip.Text = "更改成功";
                        }
                        catch
                        {
                            TimeTipIcon.Text = "\uE783";
                            TimeTip.Text = "文件格式错误";
                            LogTool.WriteLog("设置 -> 文件格式错误", LogTool.LogType.Error);
                        }
                        if (CurrentConfig.Timetable_File != App.ConfigData.Timetable_File) IsChangeSaved.Text = "设置未保存";
                        else if (MatchConfig()) IsChangeSaved.Text = string.Empty;

                        // 等待后清空
                        await Task.Run(async () => { await Task.Delay(3000); });
                        TimeTipIcon.Text = "";
                        TimeTip.Text = "";
                    }
                }
            }
        }

        private void TTime_SelectedDateChanged(object sender, SelectionChangedEventArgs e) // 更改目标时间
        {
            LogTool.WriteLog("设置 -> 设置目标时间", LogTool.LogType.Info);
            string DateStr = Utils.TimeConverter.DateInt2Str(TTime.SelectedDate.Value.Year, TTime.SelectedDate.Value.Month, TTime.SelectedDate.Value.Day); // 把时间转为字符串
            CurrentConfig.Target_Time = DateStr;
            if (CurrentConfig.Target_Time != App.ConfigData.Target_Time) IsChangeSaved.Text = "设置未保存";
            else if (MatchConfig()) IsChangeSaved.Text = string.Empty;
        }

        private void TName_TextChanged(object sender, TextChangedEventArgs e) // 更改目标事件名
        {
            string Name = TName.Text;
            if (!string.IsNullOrEmpty(Name))
            {
                TipIcon.Text = string.Empty;
                Tip.Text = string.Empty;
                CurrentConfig.Target_Type = Name;
                if (CurrentConfig.Target_Type != App.ConfigData.Target_Type) IsChangeSaved.Text = "设置未保存";
                else if (MatchConfig()) IsChangeSaved.Text = string.Empty;
            }
            else
            {
                TipIcon.Text = "\uE783";
                Tip.Text = "未填写内容";
            }
        }

        private void TFrontTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (TFrontTime.Text == string.Empty)
                {
                    TipIcon2.Text = "\uE783";
                    Tip2.Text = "未填写内容";
                    return;
                }
                int front_T = Convert.ToInt32(TFrontTime.Text);
                if (front_T <= 0)
                {
                    TFrontTime.Text = "1";
                    front_T = 1;
                }
                else if (front_T >= 60)
                {
                    TFrontTime.Text = "60";
                    front_T = 60;
                }
                TipIcon2.Text = string.Empty;
                Tip2.Text = string.Empty;
                CurrentConfig.Front_Min = front_T;
                if (CurrentConfig.Front_Min != App.ConfigData.Front_Min) IsChangeSaved.Text = "设置未保存";
                else if (MatchConfig()) IsChangeSaved.Text = string.Empty;
            }
            catch
            {
                TipIcon2.Text = "\uE783";
                Tip2.Text = "格式错误";
            }
        }
        #endregion


        private void GotoGithubIssue(object sender, RoutedEventArgs e) { System.Diagnostics.Process.Start("https://github.com/Muhu-C/DateTimer/issues"); }
        private void GotoGiteeIssue(object sender, RoutedEventArgs e) { System.Diagnostics.Process.Start("https://gitee.com/zzhkjf/DateTimer/issues"); }
        private void GotoGithub(object sender, RoutedEventArgs e) { System.Diagnostics.Process.Start("https://github.com/Muhu-C/"); }
        private void GotoBilibili(object sender, RoutedEventArgs e) { System.Diagnostics.Process.Start("https://space.bilibili.com/1469137723"); }

        private bool MatchConfig() // 修改后文件和源文件一致
        {
            if (CurrentConfig.Front_Min == App.ConfigData.Front_Min
                    && CurrentConfig.Target_Type == App.ConfigData.Target_Type
                    && CurrentConfig.Target_Time == App.ConfigData.Target_Time
                    && CurrentConfig.Timetable_File == App.ConfigData.Timetable_File)
                return true;
            else return false;
        }

        private void CopySystemReport(object sender, RoutedEventArgs e) // 生成系统报告
        {
            LogTool.WriteLog("设置 -> 生成系统报告", LogTool.LogType.Info);
            TimeSpan timeSpan = DateTime.Now - LastChange;
            if (timeSpan.TotalSeconds < 5)
            {
                MsgBox.Error("请勿频繁操作! ");
                return;
            }
            LastChange = DateTime.Now;
            string Report = 
                  "生成时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") 
                + "\n系统版本: " + Utils.OtherTools.GetWinVer()
                + "\n系统位数: " + Utils.OtherTools.GetBit().ToString() 
                + "\n运行时版本: " + Utils.OtherTools.GetEnvVer()
                + "\nDateTimer 版本: " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            MessageBoxResult r = MsgBox.Show(Report + "\n是否复制到剪贴板？", "系统报告", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes);
            if (r == MessageBoxResult.Yes)
            {
                Clipboard.SetDataObject(Report);
                HandyControl.Controls.Growl.Success("系统报告复制成功！");
            }
        }

        

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (MatchConfig()) return;
            LogTool.WriteLog("设置 -> 保存设置", LogTool.LogType.Info);
            string NewJson = Utils.TimeTable.Json_Optimization(JsonConvert.SerializeObject(CurrentConfig));
            Utils.FileProcess.WriteFile(NewJson, App.configPath);
            App.LoadConfig();
            Reload();
            (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow).Timer.LoadJson();
            IsChangeSaved.Text = string.Empty;
            HandyControl.Controls.Growl.Success("保存设置成功! ");
        }
    }
}
