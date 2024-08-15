using System;
using DateTimer;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using HandyControl.Themes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace DateTimer.View
{
    /// <summary>
    /// TimerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TimerWindow : HandyControl.Controls.Window
    {
        public List<Utils.TimeTable.Table> tables = new List<Utils.TimeTable.Table>();
        public List<int> undone = new List<int>();

        public TimerWindow()
        {
            InitializeComponent();
            LogTool.WriteLog("时间表窗口 -> 初始化", LogTool.LogType.Info);
            CurrentTableEntry.model.TableEntries = new ObservableCollection<Utils.TimeTable.TableEntry>();
        }
        public void Window_Loaded(object sender, RoutedEventArgs e) // 获取时间，获取当前所在时间段
        {
            LogTool.WriteLog("时间表窗口 -> 加载", LogTool.LogType.Info);
            DataContext = CurrentTableEntry.model;
            SizeToContent = SizeToContent.WidthAndHeight;
            if (App.ConfigData.Theme == 0)
                Theme.SetSkin(this, HandyControl.Data.SkinType.Dark);
            else Theme.SetSkin(this, HandyControl.Data.SkinType.Default);
        }

        /// <summary> 加载时间表 </summary>
        public void LoadJson()
        {
            LogTool.WriteLog("时间表窗口 -> 加载时间表", LogTool.LogType.Info);
            // 获取时间表
            try
            {
                InitUI(Utils.TimeTable.GetTimetables(App.ConfigData.Timetable_File));
            }
            catch
            {
                App.NoticeWindow.Show();
                App.NoticeWindow.Ctt.NoticeText1 = "警告";
                App.NoticeWindow.Ctt.NoticeText2 = $"时间表配置错误！\n请检查 json 文件是否完整";
                App.NoticeWindow.MediaFile = "Data/Media/notice.wav";
                App.NoticeWindow.Init();
            }
        }

        /// <summary> 显示时间表 </summary>
        public void InitUI(Utils.TimeTable.TimeTableFile file)
        {
            LogTool.WriteLog("时间表窗口 -> 加载 UI", LogTool.LogType.Info);
            if (file == null)
            {
                CurrentTableEntry.model.TableEntries = new ObservableCollection<Utils.TimeTable.TableEntry> 
                { new Utils.TimeTable.TableEntry { Time = "提示", Name = "没有时间表", Notice = "请配置时间表" } };
                return;
            }
            int TodayListIndex = Utils.TimeTable.GetTodayList(file.timetables);
            if (TodayListIndex < 0)
            {
                CurrentTableEntry.model.TableEntries = new ObservableCollection<Utils.TimeTable.TableEntry> 
                { new Utils.TimeTable.TableEntry { Time = "0:00 ~ 23:59", Name = "今日无时间安排" } };
                return;
            }

            tables = file.timetables[TodayListIndex].tables; // 当天时间表
            undone = Utils.TimeTable.GetTodayUndone(tables); // 未完成项目
            CurrentTableEntry.model.TableEntries.Clear();
            foreach (Utils.TimeTable.Table table in tables) CurrentTableEntry.model.TableEntries.Add(Utils.TimeTable.Table2Entry(table)); // 添加内容
        }


        private void Window_Closing(object sender, CancelEventArgs e) 
        {
            LogTool.WriteLog("时间表窗口 -> 隐藏时间表", LogTool.LogType.Info);
            if (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow == null) { return; }
            e.Cancel = true; Hide();
            MainWindow mw = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;
            mw.Home.ShowTimeTable.Style = FindResource("ButtonSuccess") as Style;
            mw.Home.ShowTimeTable.Content = "显示时间表";
        }

        /// <summary> 异步重复执行获取时间 </summary>
        public async void GetTime()
        {
            LogTool.WriteLog("时间表窗口 -> 获取时间开始", LogTool.LogType.Info);
            int SpanSeconds = 0;
            await Task.Run(async () =>
            {
                while (true) // 程序运行中重复执行
                {
                    if (IsVisible)
                    {
                        TimeSpan remainingTime = Utils.TimeConverter.Str2Date(App.ConfigData.Target_Time) - DateTime.Now; // 目标剩余时间
                        
                        // 获取时间表，获取当前时间所在时间段
                        List<int> inds = Utils.TimeTable.GetCurZone(tables);
                        int ind = -1;
                        if (inds.Count != 0) ind = inds[0];
                        string str = "目标";
                        if (App.ConfigData.Target_Type != "NULL") str = App.ConfigData.Target_Type;
                        Dispatcher.Invoke(() =>
                        {
                            if (remainingTime < TimeSpan.Zero) CountdownText.Text = "已到达" + str + "时间";
                            else CountdownText.Text = $"距 {str} {remainingTime.Days}天 {remainingTime.Hours}时" +
                                $" {remainingTime.Minutes}分 {remainingTime.Seconds}秒";

                            if (ind != TimetableListView.SelectedIndex && ind != -1) TimetableListView.SelectedIndex = ind;
                        });
                        inds.Clear();
                    }

                    int nowind = Utils.TimeTable.IsStart(tables, TimeSpan.Zero);
                    if (nowind != -1 && undone[nowind] > 0)
                    {
                        undone[nowind] = 0;
                        Dispatcher.Invoke(() =>
                        {
                            App.NoticeWindow.Show();
                            App.NoticeWindow.Ctt.NoticeText1 = "提示";

                            if (tables[nowind].notice != "NULL") 
                                App.NoticeWindow.Ctt.NoticeText2 = 
                                $"{tables[nowind].name} 时间 到了\n提示: {tables[nowind].notice}\n" +
                                $"时间段: {Utils.TimeConverter.JTime2DTime(tables[nowind].start)} ~ {Utils.TimeConverter.JTime2DTime(tables[nowind].end)}";

                            else 
                                App.NoticeWindow.Ctt.NoticeText2 = 
                                $"{tables[nowind].name} 时间 到了\n提示: 无\n时间段: {Utils.TimeConverter.JTime2DTime(tables[nowind].start)} " +
                                $"~ {Utils.TimeConverter.JTime2DTime(tables[nowind].end)}";

                            App.NoticeWindow.MediaFile = "Data/Media/alarm.wav";
                            App.NoticeWindow.Init();
                        });
                    }

                    int fminind = Utils.TimeTable.IsStart(tables, TimeSpan.FromMinutes(App.ConfigData.Front_Min));
                    if (fminind != -1 && undone[fminind] == 2)
                    {
                        if (nowind != -1 && SpanSeconds < 6) SpanSeconds++;
                        else
                        {
                            SpanSeconds = 0;
                            undone[fminind] = 1;
                            Dispatcher.Invoke(() =>
                            {
                                App.NoticeWindow.Show();
                                App.NoticeWindow.Ctt.NoticeText1 = "提示";

                                if (tables[fminind].notice != "NULL")
                                    App.NoticeWindow.Ctt.NoticeText2 =
                                    $"{App.ConfigData.Front_Min} 分钟后将要到 {tables[fminind].name} 时间了\n提示: {tables[fminind].notice}\n" +
                                    $"时间段: {Utils.TimeConverter.JTime2DTime(tables[fminind].start)} ~ {Utils.TimeConverter.JTime2DTime(tables[fminind].end)}";

                                else
                                    App.NoticeWindow.Ctt.NoticeText2 =
                                    $"{App.ConfigData.Front_Min} 分钟后将要到 {tables[fminind].name} 时间了\n提示: 无\n" +
                                    $"时间段: {Utils.TimeConverter.JTime2DTime(tables[fminind].start)} ~ {Utils.TimeConverter.JTime2DTime(tables[fminind].end)}";

                                App.NoticeWindow.MediaFile = "Data/Media/notice.wav";
                                App.NoticeWindow.Init();
                            });
                        }
                    }
                    await Task.Delay(1000);
                }
            });
            LogTool.WriteLog("时间表窗口 -> 获取时间结束", LogTool.LogType.Info);
        }
    }
}
