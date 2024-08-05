using DateTimer;
using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using MsgBox = HandyControl.Controls.MessageBox;
using System.Threading;
using HandyControl.Controls;
using DT_Lib;
using System.Net;
using System.Text;
using System.Windows.Threading;
using static System.Net.WebRequestMethods;

namespace DateTimer
{
    /// <summary>
    /// 程序根交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static string FeedBackUrl = "https://github.com/Muhu-C/DateTimer/issues"; // 错误反馈链接
        public static List<string> NoticeUrl = new List<string> 
        {
            "https://gitee.com/zzhkjf/NoticePage/raw/main/DATETIMER.NOTICE", // Gitee Raw 【首选】
            "https://raw.gitmirror.com/Muhu-C/NoticePage/main/DATETIMER.NOTICE", // Gitmirror(Github) Raw
            "https://mirror.ghproxy.com/https://raw.githubusercontent.com/Muhu-C/NoticePage/main/DATETIMER.NOTICE", // Gitproxy(Github) Raw
            "https://raw.githubusercontent.com/Muhu-C/NoticePage/main/DATETIMER.NOTICE" // Github Raw
        }; // 公告链接
        public static string FNoticeUrl;
        public static string About = "DateTimer 1.0.1 By MC118CN\n使用 C#(.NET Framework) WPF HandyControls 编写\n本软件使用 MIT LICENSE, 转载请标明出处!"; // 关于程序
        #region 错误处理
        public enum ErrorType
        {
            ProgramError = 0,
            UnknownError = 1,
            RunTimeError = 2,
            ProgresError = 3,
            FatalError = 4
        };
        /// <summary>
        /// 显示错误信息
        /// </summary>
        /// <param name="ErrorMessage">正文错误信息</param>
        /// <param name="Type">错误类型</param>
        /// <param name="ShutDown">是否关闭程序</param>
        /// <param name="FeedBack">是否反馈作者</param>
        public static void Error(string ErrorMessage, ErrorType Type, bool ShutDown, bool WindowType = true, bool FeedBack = true)
        {
            string ErrorText = "灾难性错误 - Fatal Error";
            if (Type == ErrorType.ProgramError)
                ErrorText = "程序运行错误 - Program Error";
            else if (Type == ErrorType.UnknownError)
                ErrorText = "未知错误 - Unknown Error";
            else if (Type == ErrorType.RunTimeError)
                ErrorText = "运行时错误 - Runtime Error";
            else if (Type == ErrorType.ProgresError)
                ErrorText = "处理错误 - Progress Error";

            if (FeedBack) { System.Diagnostics.Process.Start(FeedBackUrl); ErrorMessage += "\n请告知程序作者"; }
            if (WindowType) MsgBox.Error("错误原因: " + ErrorMessage, ErrorText);
            else System.Windows.Forms.MessageBox.Show("错误原因: " + ErrorMessage, ErrorText, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            if (ShutDown) Environment.Exit(0);
        }
        #endregion

        public static HomePage Home = new HomePage();
        public static SettingPage Setting = new SettingPage();
        public static TimerPage TimerPg = new TimerPage();
        public static TimerWindow Timer = new TimerWindow();
        public static appconfig ConfigData;
        public static string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "config.json");
        public static string tablePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);

        public App()
        {
            Console.WriteLine("程序开始运行");
            LoadConfig();
            LoadNotice();
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
            catch(FileNotFoundException)
            {
                System.Windows.Forms.MessageBox.Show("未找到程序配置", "程序运行错误 - Program Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                System.Diagnostics.Process.Start(FeedBackUrl);
                Current.Shutdown();
            }
        }

        public static string Notice_Text = string.Empty;
        public static async void LoadNotice()
        {
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
            catch (Exception ex) { Notice_Text = "公告接收失败\n请检查网络\n或联系程序作者 MC118CN\n加载错误:" + ex.Message; 
                Error(ex.Message+"公告接收失败\n请检查网络",App.ErrorType.ProgresError, false, true, true); }
        }
    }
    public class appconfig // Config/config.json 解析内容
    {
        public int Theme { get; set; } // 0 黑 1 白
        public string Target_Time { get; set; }
        public string Target_Type {  get; set; }
        public string Timetable_File { get; set; }
    }
}