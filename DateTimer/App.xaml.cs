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
using DateTimer.View.CustomControls;

namespace DateTimer
{
    /// <summary> App.xaml 交互逻辑 </summary>
    public partial class App : Application
    {

        public static string FeedBackUrl = "https://github.com/Muhu-C/DateTimer/issues";
        public static bool isLogOpened = false; // 支持 Log (调试时)
        #region 静态变量
        public static CustomNotice NoticeWindow;


        public static List<string> NoticeUrl = new List<string> 
        {
            "https://gitee.com/zzhkjf/NoticePage/raw/main/DATETIMER.NOTICE",
            "https://raw.gitmirror.com/Muhu-C/NoticePage/main/DATETIMER.NOTICE",
            "https://mirror.ghproxy.com/https://raw.githubusercontent.com/Muhu-C/NoticePage/main/DATETIMER.NOTICE",
            "https://raw.githubusercontent.com/Muhu-C/NoticePage/main/DATETIMER.NOTICE"
        };

        public static string Notice_Text = string.Empty;

        /// <summary> 配置数据 </summary>
        public static Appconfig ConfigData;

        public static string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Config", "config.json");
        public static string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Logs", $"DTLOG-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.log");

        public static Mutex AppMutex;

        public static List<int> WindowCnt = new List<int>(); // 消息窗口编号
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
            if (ex != null) message = $"报错信息: {ex.Message} \n报错位置: {ex.Source}\n报错代码: \n{ex.StackTrace}\n报错数据类型: {ex.Data}\n提示: {ErrorMessage}\n{ex.InnerException}";
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
            LoadConfig();

            LogTool.InitLog();
            LogTool.WriteLog("启动 DateTimer 程序", LogTool.LogType.Info);
            // 异常处理
            DispatcherUnhandledException += (_ , e) =>
            {
                LogTool.WriteLog(e.Exception.ToString(), LogTool.LogType.Error);
                if (e.Exception is Win32Exception)
                {
                    Console.WriteLine(e.Exception.ToString());
                    e.Handled = true; // 防止程序原地升天
                    return;
                }
                e.Handled = true; // 防止程序原地升天
                Error("无", ErrorType.UnknownError, e.Exception, false, WindowType: false, FeedBack: true);
            };
        }

        public static void LoadConfig()
        {
            try
            {
                ConfigData = JsonConvert.DeserializeObject<Appconfig>(Utils.FileProcess.ReadFile(configPath));
                if (ConfigData.Enable_Log == 1) isLogOpened = true;
                LogTool.WriteLog("读取 config.json 完成", LogTool.LogType.Info);
            }
            catch(Exception ex) // 未找到文件
            {
                LogTool.WriteLog("未找到或无法加载 config.json", LogTool.LogType.Fatal);
                Error("请检查下载文件是否完整, ", ErrorType.RunTimeError, ex, true, WindowType: false, FeedBack: false);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            AppMutex = new Mutex(true, Assembly.GetExecutingAssembly().GetName().Name, out var createNew);

            if (!createNew)
            {
                LogTool.WriteLog("已有该程序启动! ", LogTool.LogType.Fatal);
                Error("已经有该程序启动！", ErrorType.ProgramError, null, true, WindowType: false, FeedBack: false);
            }

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
                LogTool.WriteLog("程序结束", LogTool.LogType.Info);
                // 退出程序
                base.OnExit(e);
            }
        }
        #endregion
    }

    #region 其他类

    public class Appconfig // Config/config.json 解析内容
    {
        public int Theme { get; set; } // 0 黑 1 白

        public string Target_Time { get; set; }

        public string Target_Type {  get; set; }

        public string Timetable_File { get; set; }

        public int Front_Min { get; set; }

        public int Enable_Log { get; set; }
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
        private string version;

        public string VersionTxt
        {
            get { return version; }

            set { if (version != value) { version = value; OnPropertyChanged("VersionTxt"); } }
        }

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