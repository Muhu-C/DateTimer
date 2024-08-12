using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using Newtonsoft.Json;
using MsgBox = HandyControl.Controls.MessageBox;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Reflection;
using System.Linq;

namespace DateTimer
{
    /// <summary> App.xaml 交互逻辑 </summary>
    public partial class App : Application
    {
        #region 静态变量
        public static string FeedBackUrl = "https://github.com/Muhu-C/DateTimer/issues";

        public static List<string> NoticeUrl = new List<string> 
        {
            "https://gitee.com/zzhkjf/NoticePage/raw/main/DATETIMER.NOTICE",
            "https://raw.gitmirror.com/Muhu-C/NoticePage/main/DATETIMER.NOTICE",
            "https://mirror.ghproxy.com/https://raw.githubusercontent.com/Muhu-C/NoticePage/main/DATETIMER.NOTICE",
            "https://raw.githubusercontent.com/Muhu-C/NoticePage/main/DATETIMER.NOTICE"
        };

        public static string About = "DateTimer " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " By MC118CN\n使用 C#(.NET Framework) WPF HandyControls 编写\n本软件使用 MIT LICENSE, 转载请标明出处!"; // 关于程序
        
        public static string Notice_Text = string.Empty;

        /// <summary> 配置数据 </summary>
        public static appconfig ConfigData;

        public static string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Config", "config.json");

        public static Mutex AppMutex;
        #endregion

        #region 错误文本显示以及处理

        public enum ErrorType
        {
            ProgramError = 0,
            UnknownError = 1, 
            RunTimeError = 2,
            ProgresError = 3,
            FatalError = 4
        };
        private static readonly List<string> ErrorLabel = new List<string>
        {
            "程序运行错误 - Program Error",
            "未知错误 - Unknown Error",
            "运行时错误 - Runtime Error",
            "处理错误 - Progress Error",
            "灾难性错误 - Fatal Error"
        };

        /// <summary> 显示错误信息 </summary>
        /// <param name="ErrorMessage"> 正文错误提示 </param>
        /// <param name="Type"> 错误类型 </param>
        /// <param name="ex"> 错误信息 </param>
        /// <param name="ShutDown"> 关闭程序 </param>
        /// <param name="WindowType"> true: HandyControl, false: WinForm </param>
        /// <param name="FeedBack"> 反馈作者 </param>
        public static void Error(string ErrorMessage, ErrorType Type, Exception ex, bool ShutDown, bool WindowType = true, bool FeedBack = true)
        {
            // 定义消息并复制到剪贴板
            string message;
            if (ex != null) message = $"报错信息: {ex.Message} \n报错位置: {ex.Source}\n报错代码: \n{ex.StackTrace}\n报错数据类型: {ex.Data}\n提示: {ErrorMessage}";
            else message = ErrorMessage;
            Clipboard.SetDataObject(message);

            // 转到 Github 反馈
            if (FeedBack) { System.Diagnostics.Process.Start(FeedBackUrl); message += "\n请告知程序作者, "; Thread.Sleep(100); }
            if (ex != null) message += "已复制到剪贴板";

            // 显示错误窗口
            if (WindowType) MsgBox.Error(message, ErrorLabel[Convert.ToInt32(Type)]);
            else System.Windows.Forms.MessageBox.Show(message, ErrorLabel[Convert.ToInt32(Type)], 0, System.Windows.Forms.MessageBoxIcon.Error);

            // 关闭程序
            if (ShutDown) Environment.Exit(0);
        }
        #endregion

        #region 加载配置

        public App()
        {
            // 异常处理
            DispatcherUnhandledException += (_ , e) =>
            {
                e.Handled = true; // 防止程序原地升天
                Error("无", ErrorType.UnknownError, e.Exception, false, WindowType: false, FeedBack: true);
            };
        }

        public static void LoadConfig()
        {
            try
            {
                ConfigData = JsonConvert.DeserializeObject<appconfig>(Utils.FileProcess.ReadFile(configPath));

                (Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow).Home?.Reload();

                (Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow).Setting?.Reload();
            }
            catch(Exception ex) // 未找到文件
            {
                Error("请检查下载文件是否完整, ", ErrorType.RunTimeError, ex, true, WindowType: false, FeedBack: false);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            AppMutex = new Mutex(true, Assembly.GetExecutingAssembly().GetName().Name, out var createNew);

            if (!createNew)
                Error("已经有该程序启动！", ErrorType.ProgramError, null, true, WindowType: false, FeedBack: false);

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                // 关闭程序监视
                AppMutex.ReleaseMutex();
            }
            finally
            {
                // 退出程序
                base.OnExit(e);
            }
        }
        #endregion
    }

    #region 其他类

    public class appconfig // Config/config.json 解析内容
    {
        public int Theme { get; set; } // 0 黑 1 白

        public string Target_Time { get; set; }

        public string Target_Type {  get; set; }

        public string Timetable_File { get; set; }
    }

    public class TimerWindowViewModel : Utils.TimeTable.ViewModelBase // 实现实时更改时间表内容
    {
        private ObservableCollection<Utils.TimeTable.TableEntry> tables;

        public ObservableCollection<Utils.TimeTable.TableEntry> TableEntries
        {
            get { return tables; }

            set { tables = value; RaisePropertyChangedEvent("TableEntries"); }
        }
    }

    /// <summary> 静态类, 管理显示时间表的 ViewModel </summary>
    public static class CurrentTableEntry
    {
        /// <summary> [公用]当前时间表 </summary>
        public static TimerWindowViewModel model = new TimerWindowViewModel();
    }

    /// <summary> HomePage 以及其他页面调用的 Binding ViewModel </summary>
    public class BindContent : INotifyPropertyChanged // 通过 Foreground Binding 实时设置页面文本颜色
    {
        private Brush textColor;

        public Brush TextColor
        {
            get { return textColor; }

            set { if (textColor != value) { textColor = value; OnPropertyChanged("TextColor"); } }
        }

        private ObservableCollection<Utils.TimeTable.TableEntry> tables;

        public ObservableCollection<Utils.TimeTable.TableEntry> TableEntries
        {
            get { return tables; }

            set { tables = value; OnPropertyChanged("TableEntries"); }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }
    #endregion
}