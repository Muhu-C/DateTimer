using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Net.NetworkInformation;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace DT_Lib
{
    /// <summary> 时间转换 </summary>
    public class TimeConverter // 时间转换
    {
        #region 字符串与年月日列表互转
        /// <summary> 把时间转为年月日8位字符串 </summary>
        /// <param name="year">年(返回4位)</param>
        /// <param name="month">月(返回2位)</param>
        /// <param name="day">日(返回2位)</param>
        /// <returns></returns>
        public static string DateInt2Str(int year, int month, int day)
        {
            string yearQ = year.ToString("0000"), monthQ = month.ToString("00"), dayQ = day.ToString("00"); // 转字符串
            string a = ""; // 返回值
            a = yearQ + ' ' + monthQ + ' ' + dayQ; //按照格式拼接字符串
            return a;
        }
        /// <summary> 把年月日转为列表 </summary>
        /// <param name="date">格式需为"yyyy MM dd"</param>
        /// <returns> Int类型列表 [0]:年 [1]:月 [2]:日 </returns>
        public static List<int> Str2DateInt(string date)
        {
            string[] time = date.Split(' '); // 0 年 1 月 2 日
            List<int> list = new List<int>();// 0 年 1 月 2 日
            if (date.Length == 10)
            {
                int year = int.Parse(time[0]);
                list.Add(year);

                int month = int.Parse(time[1]);
                list.Add(month);

                int day = int.Parse(time[2]);
                list.Add(day);
            }
            return list;
        }
        #endregion

        #region 字符串与DateTime互转
        /// <summary> 字符串转DateTime类型 </summary>
        /// <param name="dtstr">格式: yyyy MM dd</param>
        /// <returns> DateTime 类 </returns>
        public static DateTime Str2Date(string dtstr)
        {
            DateTime dt = DateTime.Now;
            try 
            {
                dt = DateTime.ParseExact(dtstr,"yyyy MM dd", null);
                return dt;
            }
            catch (Exception ex) { throw ex; }
        }
        /// <summary> DateTime 类型转 string </summary>
        /// <param name="dateTime"></param>
        /// <returns> 格式: yyyy MM dd </returns>
        public static string Date2Str(DateTime dateTime)
        {
            try { return dateTime.Year.ToString("0000") + " " + dateTime.Month.ToString("00") + " " + dateTime.Day.ToString("00"); }
            catch (Exception ex) { throw ex; }
        }

        #endregion

        #region 列表和时分字符串互转

        /// <summary> 把时间4位字符串转为列表 </summary>
        /// <param name="str">格式须为 "HH MM"</param>
        /// <returns> Int类型列表 [0]:时 [1]:分 </returns>
        public static List<int> Str2TimeInt(string time, char split = ' ')
        {
            string[] times = time.Split(split); // times[0]为时 times[1]为分
            List<int> TimeList = new List<int>();
            foreach (string s in times)
            {
                int inttime = int.Parse(s);
                TimeList.Add(inttime);
            }
            return TimeList;
        }

        /// <summary> 把列表转为时间4位字符串 </summary>
        /// <param name="list">格式需为 [0]:时 [1]:分</param>
        /// <returns>字符串 "HH MM"</returns>
        public static string TimeInt2Str(List<int> list, char Insert = ' ')
        {
            return list[0].ToString("00") + Insert + list[1].ToString("00");
        }

        #endregion

        #region 时分列表和TimeSpan互转

        /// <summary>
        /// 把列表转为TimeSpan时间
        /// </summary>
        /// <param name="list">格式需为 [0]:时 [1]:分</param>
        /// <returns></returns>
        public static TimeSpan Int2Time(List<int> list)
        {
            return DateTime.Parse(TimeInt2Str(list, ':')).TimeOfDay;
        }

        /// <summary>
        /// 把列表转为TimeSpan时间
        /// </summary>
        /// <param name="time">TimeSpan 时间</param>
        /// <returns>Int列表 [0]:时 [1]:分</returns>
        public static List<int> Time2Int(TimeSpan time)
        {
            return new List<int> { time.Hours, time.Minutes };
        }

        #endregion

        public static string JsonTime2DisplayTime(string time)
        {
            string[] a = time.Split(' ');
            return int.Parse(a[0]).ToString() + ":" + a[1];
        }

        public static string NumToTime(string num)
        {
            string numStr = "1234567";
            string chineseStr = "一二三四五六日";
            string result = "";
            int numIndex = numStr.IndexOf(num);
            if (numIndex > -1)
                result = chineseStr.Substring(numIndex, 1);
            return result;
        }
    }

    /// <summary> 文件流处理 </summary>
    public class FileProcess // 文件流处理
    {
        /// <summary> 用流写入文件 </summary>
        /// <param name="Text">字符串</param>
        /// <param name="Path">存放位置</param>
        public static void WriteFile(string Text,string Path)
        {
            using (StreamWriter sw = new StreamWriter(Path, false, Encoding.UTF8))
            {
                sw.Write(Text);
            }
        }
        
        /// <summary> 用流读取文件 </summary>
        /// <param name="Path">文件路径</param>
        /// <returns></returns>
        public static string ReadFile(string Path)
        {
            using (StreamReader sr = new StreamReader(Path))
            {
                string content;
                content = sr.ReadToEnd();
                return content;
            }
        }
    }

    /// <summary> 时间表处理 </summary>
    public class TimeTable
    {
        #region timetable-JSON
        /// <summary> ViewModel基础类 </summary>
        public abstract class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            protected void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
            {
                if (propertyName == "")
                {
                    propertyName = GetCallerMemberName();
                }
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
            string GetCallerMemberName()
            {
                StackTrace trace = new StackTrace();
                StackFrame frame = trace.GetFrame(2);//1代表上级，2代表上上级，以此类推  
                var propertyName = frame.GetMethod().Name;
                if (propertyName.StartsWith("get_") || propertyName.StartsWith("set_") || propertyName.StartsWith("put_"))
                {
                    propertyName = propertyName.Substring("get_".Length);
                }
                return propertyName;
            }
        }
        /// <summary> json 反序列化的类 </summary>
        public class TimeTableFile // json第一层
        {
            public List<Timetables> timetables { get; set; } // 第二层
        }
        /// <summary> 时间表列表类 </summary>
        public class Timetables // json第二层
        {
            public string date { get; set; }
            public string weekday { get; set; }
            public List<Table> tables { get; set; } // 第三层
        }
        /// <summary> 时间表类 </summary>
        public class Table // json第三层
        {
            public string name { get; set; }
            public string start { get; set; }
            public string end { get; set; }
            public string notice { get; set; }
        }
        /// <summary> 时间表显示类 </summary>
        public class TableEntry
        {
            public string Name { get; set; }
            public string Time { get; set; }
            public string Notice {  get; set; }
        }
        #endregion

        #region 处理

        /// <summary> 反序列化时间表 json 文件 </summary>
        /// <param name="Path">json 位置</param>
        /// <returns>时间表类</returns>
        public static TimeTableFile GetTimetables(string Path)
        {
            TimeTableFile tables;
            string JsonStr = FileProcess.ReadFile(Path);
            try { tables = JsonConvert.DeserializeObject<TimeTableFile>(JsonStr); }
            catch(Exception ex) { throw ex; } // 错误处理
            return tables;
        }

        /// <summary> 序列化并写入时间表 json </summary>
        /// <param name="table">时间表类</param>
        /// <param name="Path">时间表文件位置</param>
        public static void WriteTimetables(TimeTableFile table,string Path)
        {
            string timetablejson = JsonConvert.SerializeObject(table);
            FileProcess.WriteFile(timetablejson, Path);
        }

        /// <summary> 将时间表Table类转为显示时间表TableEntry类 </summary>
        /// <param name="table">时间表Table类</param>
        /// <returns>时间表TableEntry类</returns>
        public static TableEntry Table2Entry(Table table)
        {
            TableEntry entry = new TableEntry();
            entry.Name = table.name;
            if (table.notice != "NULL") entry.Notice = table.notice;
            string time1 = TimeConverter.JsonTime2DisplayTime(table.start);
            string time2 = TimeConverter.JsonTime2DisplayTime(table.end);
            entry.Time = time1 + "~" + time2;
            return entry;
        }

        /// <summary> 获取当前所在时间段 </summary>
        /// <param name="table"></param>
        /// <returns>当前时间在时间段的Index</returns>
        public static List<int> GetCurZone(List<Table> tables)
        {
            try
            {
                List<int> index = new List<int>();
                int i = 0;
                foreach (Table table in tables)
                {
                    TimeSpan start = TimeConverter.Int2Time(TimeConverter.Str2TimeInt(table.start));
                    TimeSpan end = TimeConverter.Int2Time(TimeConverter.Str2TimeInt(table.end));
                    if (start < end)
                    {
                        TimeSpan now = DateTime.Now.TimeOfDay;
                        if (now > start && now < end)
                        {
                            index.Add(i);
                        }
                    }
                    i++;
                }
                return index;
            }
            catch (Exception ex) { throw ex; }
        }

        /// <summary> 获取当天对应时间表 </summary>
        /// <param name="timetables">时间表类</param>
        /// <returns>索引</returns>
        public static int GetTodayList(List<Timetables> timetables)
        {
            if (timetables == null) return -1;
            if (timetables.Count == 0) return -1;
            int l = -1,i = 0; // index
            int weekday = Convert.ToInt16(DateTime.Now.DayOfWeek); // 0 为周日
            int curday = Convert.ToInt16(DateTime.Now.Day);
            int curmon = Convert.ToInt16(DateTime.Now.Month);
            int curyear = Convert.ToInt16(DateTime.Now.Year);
            
            foreach (Timetables t in timetables)
            {
                if (t.date == "GENERAL")
                {
                    string wds = t.weekday;
                    if (wds.Contains(weekday.ToString())) l = i;
                }
                else
                {
                    List<int> list = TimeConverter.Str2DateInt(t.date);
                    try { if (list[0] == curyear && list[1] == curmon && list[2] == curday) l = i; }
                    catch { l = -1; }
                }
                i++;
            }
            return l;
        }

        /// <summary> 将时间表 json 的星期日转为汉字文本 </summary>
        /// <param name="jsonweekday">时间表 json 的星期日</param>
        /// <returns>汉字文本</returns>
        public static string GetWeekday(string jsonweekday)
        {
            List<string> outstr = new List<string>();
            string[] a = jsonweekday.Split(' ');
            foreach (string str in a)
                outstr.Add("周" + TimeConverter.NumToTime(str));
            return String.Join(", ",outstr);
        }

        /// <summary> 将 json 字符串格式化 </summary>
        /// <param name="oldjson">单行 json 字符串</param>
        /// <returns>格式化后的 json 字符串</returns>
        public static string Json_Optimization(string oldjson)
        {
            int l = 0, k = 0;
            bool isInString = false;
            string newjson = string.Empty;
            foreach (char c in oldjson)
            {
                if (c == '\"') { if (!isInString) isInString = true; else isInString = false; }
                newjson += c;
                if (c == '{' && !isInString)
                {
                    l++;
                    newjson += '\n';
                    for (int i = 1; i <= l; i++) newjson += "    ";
                }
                else if (oldjson.Length > k + 1 && oldjson[k + 1] == '}' && !isInString)
                {
                    l--;
                    newjson += '\n';
                    for (int i = 1; i <= l; i++) newjson += "    ";
                }
                else if (c == ',' && !isInString)
                {
                    newjson += '\n';
                    for (int i = 1; i <= l; i++) newjson += "    ";
                }
                k++;
            }
            return newjson;
        }
        #endregion
    }

    /// <summary> 网络工具 </summary>
    public class NetTool
    {
        /// <summary> ping 各个网站并返回延迟最小的 </summary>
        /// <param name="strings"> 网站链接列表 </param>
        /// <returns> 延迟最小网站的链接 </returns>
        public static string Pings(List<string> strings)
        {
            string minlink = string.Empty;
            long min = 2147483647;
            foreach (string s in strings)
            {
                string s1 = s.Split('/')[2]; // 分割域名
                Ping ping = new Ping();
                try
                {
                    PingReply reply = ping.Send(s1, 4200);
                    if (reply.Status == IPStatus.Success)
                    {
                        if (min > reply.RoundtripTime && reply.RoundtripTime != 0)
                        {
                            min = reply.RoundtripTime;
                            minlink = s;
                        }
                        if(reply.RoundtripTime <= 40 && reply.RoundtripTime > 0) return s; // 如果延迟极小则直接使用
                    }
                }
                catch { Console.WriteLine("错误地址: "+s1); continue; } // 无法连接网站时跳过
            }
            return minlink;
        }
    }

    /// <summary> 其他工具 </summary>
    public class OtherTools
    {
        /// <summary> 连接多个字符串 </summary>
        /// <param name="connecter"> 连接字符串的方式 </param>
        /// <param name="args"> 多个字符串 </param>
        /// <returns></returns>
        public static string ConnectErrStr(string connecter, params string[] args)
        {
            string ret = string.Empty;
            foreach (string s in args)
                ret += (s + connecter);
            return ret;
        }

        /// <summary> 将修改列表去重 </summary>
        /// <param name="strings">原修改列表</param>
        /// <returns>去重后的修改列表</returns>
        public static List<string> DuplicateRemoval(List<string> strings)
        {
            List<string> list = new List<string>();
            int cnt = strings.Count;
            if (strings != null &&cnt > 0)
            {
                strings = ClassSort(strings);
                for (int i = 0;i < cnt;i++)
                {
                    if (i != cnt - 1)
                    {
                        string[] new1 = strings[i].Split('%');
                        string strA = new1[0] + '%' + new1[1] + '%' + new1[2];
                        string strB = new1[3];
                        string[] new2 = strings[i + 1].Split('%');
                        string strA2 = new2[0] + '%' + new2[1] + '%' + new2[2];
                        string strB2 = new2[3];
                        if (strA != strA2) list.Add(strA + "%" + strB);
                    }
                    else list.Add(strings[i]);
                }
            }
            return list;
        }

        /// <summary>
        /// 对列表进行分类排序
        /// </summary>
        /// <param name="strings">修改记录</param>
        /// <returns>排序后的修改记录</returns>
        public static List<string> ClassSort(List<string> strings)
        {
            List<List<string>> list2f = new List<List<string>>();
            List<string> list1f = new List<string>();
            foreach (string str in strings)
            {
                string[] new1 = str.Split('%');
                string strA = new1[0] + '%' + new1[1] + '%' + new1[2];
                string strB = new1[3];
                bool a = false;
                for(int i = 0;i < list2f.Count;i++)
                    if (list2f[i][0].StartsWith(strA)) { list2f[i].Add(strA + "%" + strB); a = true; }
                if(a == false) list2f.Add(new List<string> { strA + "%" + strB });
            }
            for(int i = 0;i < list2f.Count;i++)
                for(int j = 0;j < list2f[i].Count;j++)
                    list1f.Add(list2f[i][j]);
            return list1f;
        }

        /// <summary>
        /// 获取 Windows 版本
        /// </summary>
        /// <returns>Windows 版本字符串</returns>
        public static string GetWinVer()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion")) // 获取注册表目录
            {
                string productName = key.GetValue("ProductName") as string; // 系统名称（Win11不适用）
                try
                {
                    int majorVersion = (int)key.GetValue("CurrentMajorVersionNumber"); // 系统版本
                    var buildNumber = int.Parse(key.GetValue("CurrentBuildNumber").ToString()); // 构建(大于22000为Win11)

                    if (!string.IsNullOrEmpty(productName) && productName.ToLower().Contains("windows"))
                    {
                        if (majorVersion > 10 || majorVersion == 10 && buildNumber >= 22000)
                        {
                            if (majorVersion > 10) return "Windows " + majorVersion + " Build " + buildNumber;
                            else return "Windows 11 Build " + buildNumber;
                        }
                        else if (majorVersion == 10 && buildNumber < 22000) return "Windows 10 Build " + buildNumber;
                        else return productName;
                    }
                    else return "错误";
                }
                catch
                {
                    return productName;
                }
            }
        }

        /// <summary>
        /// 获取运行时版本
        /// </summary>
        /// <returns>.NET 版本</returns>
        public static string GetEnvVer()
        {
            try { return RuntimeInformation.FrameworkDescription; }
            catch (Exception e) { throw e; }
        }

        /// <summary>
        /// 获取系统位数
        /// </summary>
        /// <returns>64 或 32</returns>
        public static int GetBit()
        {
            int n = 0;
            if (Environment.Is64BitOperatingSystem) n = 64;
            else n = 32;
            return n;
        }
    }
}
