using DateTimer;
using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using MsgBox = HandyControl.Controls.MessageBox;
using DT_Lib;
using System.Net;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Reflection;

namespace DateTimer
{
    /// <summary>
    /// 程序根交互逻辑
    /// </summary>
    public partial class App : Application
    {
        #region 静态变量
        /// <summary>
        /// 反馈链接
        /// </summary>
        public static string FeedBackUrl = "https://github.com/Muhu-C/DateTimer/issues"; // 错误反馈链接
        /// <summary>
        /// 公告链接
        /// </summary>
        public static List<string> NoticeUrl = new List<string> 
        {
            "https://gitee.com/zzhkjf/NoticePage/raw/main/DATETIMER.NOTICE", // Gitee Raw 【首选】
            "https://raw.gitmirror.com/Muhu-C/NoticePage/main/DATETIMER.NOTICE", // Gitmirror(Github) Raw
            "https://mirror.ghproxy.com/https://raw.githubusercontent.com/Muhu-C/NoticePage/main/DATETIMER.NOTICE", // Gitproxy(Github) Raw
            "https://raw.githubusercontent.com/Muhu-C/NoticePage/main/DATETIMER.NOTICE" // Github Raw
        };
        /// <summary>
        /// 当前公告地址
        /// </summary>
        public static string FNoticeUrl;
        /// <summary>
        /// 关于程序的文本
        /// </summary>
        public static string About = "DateTimer " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " By MC118CN\n使用 C#(.NET Framework) WPF HandyControls 编写\n本软件使用 MIT LICENSE, 转载请标明出处!"; // 关于程序
        #endregion
        #region 错误文本显示以及处理
        /// <summary>
        /// 错误类型
        /// </summary>
        public enum ErrorType
        {
            /// <summary>
            /// 程序运行错误
            /// </summary>
            ProgramError = 0,
            /// <summary>
            /// 未知错误
            /// </summary>
            UnknownError = 1, 
            /// <summary>
            /// 运行时错误
            /// </summary>
            RunTimeError = 2,
            /// <summary>
            /// 处理错误
            /// </summary>
            ProgresError = 3,
            /// <summary>
            /// 灾难性错误
            /// </summary>
            FatalError = 4
        };
        /// <summary>
        /// 显示错误信息
        /// </summary>
        /// <param name="ErrorMessage">正文错误信息</param>
        /// <param name="Type">错误类型</param>
        /// <param name="ShutDown">是否关闭程序</param>
        /// <param name="WindowType">true 为程序运行中的错误, false 为程序运行前的错误</param>
        /// <param name="FeedBack">是否反馈作者</param>
        public static void Error(string ErrorMessage, ErrorType Type, Exception ex, bool ShutDown, bool WindowType = true, bool FeedBack = true)
        {
            string ErrorText = "灾难性错误 - Fatal Error"; // 根据错误类型制定标题
            if (Type == ErrorType.ProgramError)
                ErrorText = "程序运行错误 - Program Error";
            else if (Type == ErrorType.UnknownError)
                ErrorText = "未知错误 - Unknown Error";
            else if (Type == ErrorType.RunTimeError)
                ErrorText = "运行时错误 - Runtime Error";
            else if (Type == ErrorType.ProgresError)
                ErrorText = "处理错误 - Progress Error";

            string message = string.Empty;
            if (ex != null) message = "报错信息: " + ex.Message + "\n报错位置: " + ex.Source + "\n报错代码: \n" + ex.StackTrace + "\n报错数据类型: " + ex.Data + "\n提示: " + ErrorMessage ;
            else message = ErrorMessage;
            Clipboard.SetDataObject(message);
            Console.Write(message + "\n");

            if (FeedBack) { System.Diagnostics.Process.Start(FeedBackUrl); message += "\n请告知程序作者, "; Thread.Sleep(100); } // 反馈
            if (WindowType) MsgBox.Error(message + "已复制到剪贴板", ErrorText); // 程序运行中发生错误
            else System.Windows.Forms.MessageBox.Show(message + "已复制到剪贴板", ErrorText, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);// 程序运行前发生错误
            if (ShutDown) Environment.Exit(0); // 是否关闭程序
        }
        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) // 未知错误处理
        {
            e.Handled = true; //使用e.Handled能防止程序崩溃
            Error("未知的错误", ErrorType.UnknownError, e.Exception, false, false, true);
        }
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) // 未知错误处理
        {
            MessageBox.Show($"CurrentDomain_UnhandledException：" + (e.ExceptionObject as Exception).Message);
            Error("未知的错误\n"+e.ToString(), ErrorType.UnknownError, null, false, false, true);
        }
        #endregion
        #region 程序配置
        public static HomePage Home = new HomePage();
        public static SettingPage Setting = new SettingPage();
        public static TimerPage TimerPg = new TimerPage();
        public static TimerWindow Timer = new TimerWindow();
        /// <summary>
        /// 配置数据
        /// </summary>
        public static appconfig ConfigData;
        /// <summary>
        /// 配置数据路径
        /// </summary>
        public static string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "config.json");
        #endregion
        #region 加载配置
        public App()
        {
            Startup += App_Startup;
            Console.WriteLine("程序开始");
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            Console.WriteLine("程序开始");
            LoadConfig(); // 加载配置文件
            LoadNotice(); // 加载公告
            //当应用程序引发但未处理异常时出现，UI线程的异常,无法捕获多线程异常
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            //当某个异常未被捕获时出现,Thread多线程和UI线程都可以捕获
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        /// <summary>
        /// 加载Config/config.json文件
        /// </summary>
        public static void LoadConfig()
        {
            Console.WriteLine("加载文件");
            try
            {
                string jsonContent; // 通过文件流读取文件，有效防止占用导致程序错误
                using (FileStream fsRead = new FileStream(configPath, FileMode.Open))
                {
                    int fsLen = (int)fsRead.Length;
                    byte[] heByte = new byte[fsLen];
                    int r = fsRead.Read(heByte, 0, heByte.Length);
                    jsonContent = Encoding.UTF8.GetString(heByte);
                }
                ConfigData = JsonConvert.DeserializeObject<appconfig>(jsonContent);
                Home.Reload();
                Setting.Reload();
            }
            catch(Exception ex) { Error("未找到程序配置，请检查下载文件是否完整\n", ErrorType.RunTimeError, ex, true, false, false); } // 未找到文件时报错
        }
        /// <summary>
        /// 公告文本
        /// </summary>
        public static string Notice_Text = string.Empty;
        /// <summary>
        /// 加载公告
        /// </summary>
        public static async void LoadNotice()
        {
            Console.WriteLine("加载公告");
            try
            {
                await Task.Run(() =>
                {
                    WebClient webClient = new WebClient();
                    webClient.Encoding = Encoding.UTF8;
                    string url = NetTool.Pings(NoticeUrl);
                    switch(url.Split('/')[2]) // 显示的公告地址
                    {
                        case "gitee.com":
                            FNoticeUrl = "Gitee Raw";
                            break;
                        case "raw.gitmirror.com":
                            FNoticeUrl = "Gitmirror Raw";
                            break;
                        case "mirror.ghproxy.com":
                            FNoticeUrl = "GitProxy";
                            break;
                        case "raw.githubusercontent.com":
                            FNoticeUrl = "Github Raw";
                            break;
                        default: break;
                    }
                    try { Notice_Text = webClient.DownloadString(url); Console.WriteLine("公告接收成功！"); }
                    catch (Exception ex) { Notice_Text = "公告接收失败, 请检查网络\n或联系程序作者 MC118CN\n加载错误:" + ex.Message; Console.WriteLine("公告接收失败！"); }
                    webClient.Dispose();
                    Home.LoadNotice();
                });
            }
            catch (Exception ex) { Notice_Text = "公告接收失败\n请检查网络\n或联系程序作者 MC118CN\n加载错误:" + ex.Message; }
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
    public class TimerWindowViewModel : TimeTable.ViewModelBase // 实现实时更改时间表内容
    {
        private ObservableCollection<TimeTable.TableEntry> tables;
        public ObservableCollection<TimeTable.TableEntry> TableEntries
        {
            get { return tables; }
            set { tables = value; RaisePropertyChangedEvent("TableEntries"); }
        }
    }
    /// <summary>
    /// 静态类, 管理显示时间表的 ViewModel
    /// </summary>
    public static class CurrentTableEntry
    {
        /// <summary>
        /// [公用]当前时间表
        /// </summary>
        public static TimerWindowViewModel model = new TimerWindowViewModel();
    }
    /// <summary>
    /// HomePage 以及其他页面调用的 Binding ViewModel
    /// </summary>
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
    #endregion
}