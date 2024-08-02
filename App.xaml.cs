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
using System.Net;
using System.Text;
using System.Windows.Threading;

namespace DateTimer
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static string FeedBackUrl = "https://muhu-c.github.io"; // 错误反馈链接
        public static string NoticeUrl = "https://raw.gitmirror.com/Muhu-C/NoticePage/main/DATETIMER.NOTICE";
        public static string About = "DateTimer 1.0.0 By MC118CN\n使用 C#(.NET Framework) WPF HandyControls 编写\n本软件使用 MIT LICENSE, 转载请标明出处!";
        #region 错误处理
        public enum ErrorType
        {
            ProgramError = 0,
            UnknownError = 1,
            RunTimeError = 2,
            ProgresError = 3
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

            if(FeedBack) System.Diagnostics.Process.Start(FeedBackUrl);
            if(WindowType)MsgBox.Error("   错误原因: " + ErrorMessage + "\n   请告知程序作者", ErrorText);
            else System.Windows.Forms.MessageBox.Show("   错误原因: " + ErrorMessage + "\n   请告知程序作者", ErrorText, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            if (ShutDown) Environment.Exit(0);
        }
        #endregion

        public static HomePage Home = new HomePage();
        public static SettingPage Setting = new SettingPage();
        public static TimerWindow Timer = new TimerWindow();
        public static appconfig ConfigData;
        public static string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "config.json");
        public static string tablePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);

        public App()
        {
            LoadConfig();
        }
        /// <summary>
        /// 加载Config/config.json文件
        /// </summary>
        public static void LoadConfig()
        {
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
                LoadNotice();
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
                    try
                    {
                        Notice_Text = webClient.DownloadString(NoticeUrl);
                    }
                    catch (Exception ex) 
                    {
                        Notice_Text = "公告接收失败\n请检查网络\n或联系程序作者 MC118CN\n加载错误:" + ex.Message;
                        Error(ex.Message + "公告接收失败\n请检查网络", ErrorType.ProgresError, false, true, true);
                    }
                    Home.LoadNotice();
                });
            }
            catch (Exception ex) { Notice_Text = "公告接收失败\n请检查网络\n或联系程序作者 MC118CN\n加载错误:" + ex.Message; Error(ex.Message+"公告接收失败\n请检查网络",App.ErrorType.ProgresError, false, true, true); }
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