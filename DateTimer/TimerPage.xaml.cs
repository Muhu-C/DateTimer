using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using MsgBox = HandyControl.Controls.MessageBox;
using HandyControl.Themes;
using DT_Lib;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace DateTimer
{
    /// <summary>
    /// TimerPage.xaml 的交互逻辑
    /// </summary>
    public partial class TimerPage : Page
    {
        #region TimerPage变量定义
        /// <summary>
        /// 时间表文件
        /// </summary>
        public TimeTable.TimeTableFile a;
        /// <summary>
        /// 选中 "a" 的当前时间表
        /// </summary>
        public List<TimeTable.Timetables> timetables = new List<TimeTable.Timetables> ();
        /// <summary>
        /// 选中 "a" 的当前时间表的实际内容
        /// </summary>
        public List<TimeTable.Table> tables = new List<TimeTable.Table>(); // 选中的时间表
        /// <summary>
        /// 时间表下标列表
        /// </summary>
        public List<int> indexes = new List<int>();
        /// <summary>
        /// 选中的时间表下标
        /// </summary>
        public static int selected_ind = -1;
        /// <summary>
        /// 存入的修改
        /// </summary>
        public static List<string> changes = new List<string>();
        #endregion
        #region TimerPage加载
        public TimerPage() { InitializeComponent(); DataContext = HomePage.viewModel;/* 使用 HomePage 的 BindingContent */ }
        private void Page_Loaded(object sender, RoutedEventArgs e) { Theme.SetSkin(this, Theme.GetSkin(MainWindow.Cur)); Reload(); }
        public void Reload() // 重载
        {
            #region 初始化
            changes = new List<string>();
            TPStart.IsEnabled = false;
            TPEnd.IsEnabled = false;
            ElementTb.IsEnabled = false;
            NoticeTb.IsEnabled = false;
            HomePage.viewModel.TableEntries = new ObservableCollection<TimeTable.TableEntry>();
            TPStart.SelectedTime = DateTime.Now;
            TPEnd.SelectedTime = DateTime.Now;
            ElementTb.Text = string.Empty;
            NoticeTb.Text = string.Empty;
            PosTb.Text = App.ConfigData.Timetable_File;
            #endregion
            try
            {
                a = TimeTable.GetTimetables(App.ConfigData.Timetable_File); // 获取时间表
                if (a != null) // 设置时间表
                {
                    timetables = a.timetables;
                    int i = 0;
                    DropDownPanel.Children.Clear();
                    if (timetables != null)
                    {
                        foreach (TimeTable.Timetables table in timetables) // 在下拉列表里添加日期
                        {
                            MenuItem item = new MenuItem();
                            if (table.date != "GENERAL")
                                item.Header = table.date;
                            else item.Header = TimeTable.GetWeekday(table.weekday);
                            indexes.Add(i);
                            item.Tag = i;
                            item.Click += ItemClick;
                            DropDownPanel.Children.Add(item);
                            i++;
                        }
                    }
                }
            }
            catch (Exception ex) { App.Error("无", App.ErrorType.UnknownError, ex, false); }
        }
        #endregion
        #region 选择时间段
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int a = TimeList.SelectedIndex;
            if(a >= 0)
            {
                TPStart.SelectedTime = new DateTime(TimeConverter.Int2Time(TimeConverter.Str2TimeInt(tables[a].start)).Ticks);
                TPEnd.SelectedTime = new DateTime(TimeConverter.Int2Time(TimeConverter.Str2TimeInt(tables[a].end)).Ticks);
                ElementTb.Text = tables[a].name;
                if (tables[a].notice != "NULL")
                    NoticeTb.Text = tables[a].notice;
                else NoticeTb.Text = String.Empty;
                TPStart.IsEnabled = true;
                TPEnd.IsEnabled = true;
                ElementTb.IsEnabled = true;
                NoticeTb.IsEnabled = true;
            }
            else
            {
                TPStart.IsEnabled = false;
                TPEnd.IsEnabled = false;
                ElementTb.IsEnabled = false;
                NoticeTb.IsEnabled = false;
            }
        }
        #endregion
        #region 更改当前日期数据
        /// <summary>
        /// 开始时间
        /// </summary>
        private void TPStart_SelectedTimeChanged(object sender, HandyControl.Data.FunctionEventArgs<DateTime?> e)
        {
            string starttime = TPStart.SelectedTime.Value.ToString("HH mm");
            try { if (selected_ind >= 0 && TPStart.IsEnabled) changes.Add("start%" + selected_ind + '%' + TimeList.SelectedIndex + '%' + starttime); }
            catch(Exception ex) { App.Error("即将关闭程序", App.ErrorType.UnknownError, ex, true); }
        }
        /// <summary>
        /// 结束时间
        /// </summary>
        private void TPEnd_SelectedTimeChanged(object sender, HandyControl.Data.FunctionEventArgs<DateTime?> e)
        {
            string endtime = TPEnd.SelectedTime.Value.ToString("HH mm");
            try { if (selected_ind >= 0 && TPEnd.IsEnabled) changes.Add("end%" + selected_ind+'%' +TimeList.SelectedIndex + '%' + endtime); }
            catch (Exception ex) { App.Error("即将关闭程序", App.ErrorType.UnknownError, ex, true); }
        }
        /// <summary>
        /// 事件名称
        /// </summary>
        private void ElementTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = ElementTb.Text;
            try { if (selected_ind >= 0 && ElementTb.IsEnabled) changes.Add("name%" + selected_ind + '%' + TimeList.SelectedIndex + '%' + text); }
            catch (Exception ex) { App.Error("即将关闭程序", App.ErrorType.UnknownError, ex, true); }
        }
        /// <summary>
        /// 事件提示
        /// </summary>
        private void NoticeTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = NoticeTb.Text;
            if (selected_ind >= 0 && NoticeTb.IsEnabled)
            {
                try
                {
                    if (text != string.Empty) changes.Add("notice%" + selected_ind + '%' + TimeList.SelectedIndex + '%' + text);
                    else changes.Add("notice%" + selected_ind + '%' + TimeList.SelectedIndex + '%' + "NULL");
                }
                catch (Exception ex) { App.Error("即将关闭程序", App.ErrorType.UnknownError, ex, true); }
            }
        }
        /// <summary>
        /// (未完成)双击新建一个指定日期的时间表 ##更改数据##
        /// </summary>
        private void PickDate_Click(object sender, RoutedEventArgs e)
        {
            changes.Add("new%" + "1 2" + '%' + "2020" + '%' + '0');
        }
        #endregion
        #region 选择日期 COMPLETED
        private void ItemClick(object sender, RoutedEventArgs e)
        {
            MenuItem item = e.Source as MenuItem; // 读取按钮数据
            selected_ind = Convert.ToInt32(item.Tag);
            ObservableCollection<TimeTable.TableEntry> converted = new ObservableCollection<TimeTable.TableEntry>(); // 新建显示的列表
            tables = timetables[selected_ind].tables; // 更新选中的时间表

            string date = timetables[selected_ind].date; // 显示 SelectedTb 的日期或时间
            string wday = TimeTable.GetWeekday(timetables[selected_ind].weekday);
            if (date != "GENERAL") SelectedTb.Text = date;
            else SelectedTb.Text = wday;

            foreach (TimeTable.Table t in tables) { converted.Add(TimeTable.Table2Entry(t)); } // 转换时间表
            HomePage.viewModel.TableEntries = converted; // 显示时间表
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Cur.ContentFrame.Navigate(App.Setting);
            MainWindow.Cur.SettingButton.IsSelected = true;
            MainWindow.Cur.TableButton.IsSelected = false;
        }

        #region 保存
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (string s in OtherTools.Duplicate_Removal(changes)) // 保存修改（一期工程）
                {
                    string[] L = s.Split('%'); // L[0]为属性, L[3]为属性值, L[1]和L[2]为位置
                    int indD = Convert.ToInt32(L[1]);
                    int indT = Convert.ToInt32(L[2]);
                    string name;
                    switch (L[0])
                    {
                        case "name":
                            name = L[3];
                            a.timetables[indD].tables[indT].name = name;
                            Console.Write("事件名 ");
                            break;
                        case "notice":
                            name = L[3];
                            a.timetables[indD].tables[indT].notice = name;
                            Console.Write("提示 ");
                            break;
                        case "start":
                            name = L[3];
                            a.timetables[indD].tables[indT].start = name;
                            Console.Write("开始时间 ");
                            break;
                        case "end":
                            name = L[3];
                            a.timetables[indD].tables[indT].end = name;
                            Console.Write("结束时间 ");
                            break;
                        case "new":
                            Console.Write("新建 ");
                            break;
                        default: break;
                    }
                    Console.Write(L[1] + " " + L[2] + " " + L[3] + "\n");
                }
                FileProcess.WriteFile(TimeTable.Json_Optimization(JsonConvert.SerializeObject(a)), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, App.ConfigData.Timetable_File)); // 写入新的 json
                // Console.Write(TimeTable.Json_Optimization(JsonConvert.SerializeObject(a) + "\n"));
                changes.Clear();
            }
            catch (Exception ex) { App.Error("即将关闭程序", App.ErrorType.UnknownError, ex, true, FeedBack: true); }
        }
        #endregion
    }
}
