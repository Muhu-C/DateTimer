using System;
using System.IO;
using System.Text;

namespace DateTimer
{
    public class LogTool
    {
        public enum LogType
        {
            Info = 0,
            Warn = 1,
            Error = 2,
            Fatal = 3
        }

        public static void InitLog()
        {
            if (!App.isLogOpened) return;
            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Logs")))
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Logs"));
            using (StreamWriter sw = new StreamWriter(App.LogPath, true, Encoding.UTF8))
            {
                sw.Write("**This is Log Of DateTimer. Encoding: UTF-8.**\n\n");
            }
        }

        public static void WriteLog(string logStr, LogType type)
        {
            if (!App.isLogOpened) return;
            string LogStr = $"[{DateTime.Now:HH:mm:ss} {type}] {logStr}";
            Console.WriteLine(LogStr);
            using (StreamWriter sw = new StreamWriter(App.LogPath, true, Encoding.UTF8))
            {
                sw.Write(LogStr + '\n');
            }
        }
    }
}
