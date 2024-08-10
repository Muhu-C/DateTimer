using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using HandyControl.Themes;
using DT_Lib;
using System.ComponentModel;
using System.Collections.ObjectModel;
using MsgBox = HandyControl.Controls.MessageBox;
using System.Linq;

namespace DateTimer.View
{
    /// <summary>
    /// TimerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TimerWindow : HandyControl.Controls.Window
    {
        public List<TimeTable.Table> tables = new List<TimeTable.Table>();

        public TimerWindow() { InitializeComponent(); }

        public void Window_Loaded(object sender, RoutedEventArgs e) // 获取时间，获取当前所在时间段
        {
            if (App.ConfigData.Theme == 0)
                Theme.SetSkin(this, HandyControl.Data.SkinType.Dark);
            else Theme.SetSkin(this, HandyControl.Data.SkinType.Default);
        }

        /// <summary> 重新加载窗口 </summary>
        public void Reload()
        {
            try
            {
                // 获取时间表
                TimeTable.TimeTableFile a = TimeTable.GetTimetables(App.ConfigData.Timetable_File);
                if (a != null)
                {
                    // 显示时间表
                    DataContext = CurrentTableEntry.model;
                    ObservableCollection<TimeTable.TableEntry> converted = new ObservableCollection<TimeTable.TableEntry>();

                    // 获取当天所在的时间表
                    int ind = TimeTable.GetTodayList(a.timetables);
                    if (ind != -1)
                    {
                        tables = a.timetables[ind].tables; 
                        foreach (TimeTable.Table t in tables)
                            converted.Add(TimeTable.Table2Entry(t));
                        CurrentTableEntry.model.TableEntries = converted;
                    }

                    // 无时间安排
                    else
                    {
                        ObservableCollection<TimeTable.TableEntry> nullentry = new ObservableCollection<TimeTable.TableEntry> { new TimeTable.TableEntry { Time = "无时间安排", Name = "未找到当天时间表" } };
                        CurrentTableEntry.model.TableEntries = nullentry;
                    }
                }
            }
            catch (Exception ex)
            {
                DataContext = CurrentTableEntry.model;
                ObservableCollection<TimeTable.TableEntry> errorentry = new ObservableCollection<TimeTable.TableEntry> { new TimeTable.TableEntry { Name = "错误", Time = "加载时间表失败", Notice = ex.Message } };
                CurrentTableEntry.model.TableEntries = errorentry;
            }
            GetTime(); // 获取当前所在时间段，获取当前倒计时，并显示当前时间段
        }

        private void Window_Closing(object sender, CancelEventArgs e) 
        {
            e.Cancel = true; Hide();
            MainWindow mw = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;
            mw.Home.ShowTimeTable.Style = FindResource("ButtonSuccess") as Style;
            mw.Home.ShowTimeTable.Content = "显示时间表";
        }

        /// <summary> 异步重复执行获取时间 </summary>
        public async void GetTime()
        {
            await Task.Run(async () =>
            {
                while (IsVisible) // 程序运行中重复执行
                {
                    TimeSpan remainingTime = TimeConverter.Str2Date(App.ConfigData.Target_Time) - DateTime.Now; // 目标剩余时间
                    try // 获取时间表，获取当前时间所在时间段
                    {
                        List<int> inds = TimeTable.GetCurZone(tables);
                        int ind = -1;
                        if (inds.Count != 0) ind = inds[0];
                        try
                        {
                            string str = "目标";
                            if (App.ConfigData.Target_Type != "NULL") str = App.ConfigData.Target_Type;
                            Dispatcher.Invoke(() =>
                            {
                                if (remainingTime < TimeSpan.Zero) CountdownText.Text = "已到达" + str + "时间";
                                else CountdownText.Text = "距 " + str + " " + remainingTime.Days + "天 " + remainingTime.Hours + "时 " + remainingTime.Minutes + "分 " + remainingTime.Seconds + "秒 ";
                                if (ind != TimetableListView.SelectedIndex && ind != -1) TimetableListView.SelectedIndex = ind;
                            });
                        }
                        catch (Exception ex) { App.Error("无", App.ErrorType.ProgramError, ex, false, true, true); return; }
                    }
                    catch (Exception ex) { App.Error("无", App.ErrorType.ProgresError, ex, false, true, false); return; }
                    await Task.Delay(1000);
                }
            });
        }
    }
}
