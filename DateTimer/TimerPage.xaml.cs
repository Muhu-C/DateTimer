using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.IO;
using MsgBox = HandyControl.Controls.MessageBox;
using HandyControl.Themes;
using DT_Lib;
using System.Collections.ObjectModel;

namespace DateTimer
{
    /// <summary>
    /// TimerPage.xaml 的交互逻辑
    /// </summary>
    public partial class TimerPage : Page
    {
        public List<TimeTable.Timetables> timetables = new List<TimeTable.Timetables> ();
        public List<TimeTable.Table> tables = new List<TimeTable.Table>(); // 选中的时间表
        public List<int> indexes = new List<int>(); // 时间表代表的下标
        public static int selected_ind = -1; // 选中的时间表代表下标
        public static List<string> changes = new List<string>(); // 存入修改
        #region TimerPage加载
        public TimerPage() { InitializeComponent(); DataContext = HomePage.viewModel; /* 使用 HomePage 的 BindingContent */ }
        private void Page_Loaded(object sender, RoutedEventArgs e) { Theme.SetSkin(this, Theme.GetSkin(MainWindow.Cur)); Reload(); }
        public void Reload() // 重载
        {
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
            try
            {
                TimeTable.TimeTableFile a = TimeTable.GetTimetables(App.ConfigData.Timetable_File); // 获取时间表
                if (a != null) // 设置时间表
                {
                    timetables = a.timetables;
                    int i = 0;
                    DropDownPanel.Children.Clear();
                    foreach(TimeTable.Timetables table in timetables) // 在下拉列表里添加日期
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
            catch (Exception ex) { }
        }
        #endregion
        #region 选择时间段
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int a = TimeList.SelectedIndex;
            if(a != -1)
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
        #region 更改数据
        private void TPStart_SelectedTimeChanged(object sender, HandyControl.Data.FunctionEventArgs<DateTime?> e)
        {
            string starttime = TPStart.SelectedTime.Value.ToString("HH mm");
            if(selected_ind > 0) changes.Add("start%" + selected_ind + '%' + TimeList.SelectedIndex + '%' + starttime);
        }
        private void TPEnd_SelectedTimeChanged(object sender, HandyControl.Data.FunctionEventArgs<DateTime?> e)
        {
            string endtime = TPEnd.SelectedTime.Value.ToString("HH mm");
            if (selected_ind > 0) changes.Add("end%" + selected_ind+'%' +TimeList.SelectedIndex + '%' + endtime);
        }
        private void ElementTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = ElementTb.Text;
            if (selected_ind > 0) changes.Add("name%" + selected_ind + '%' + TimeList.SelectedIndex + '%' + text);
        }
        private void NoticeTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = NoticeTb.Text;
            if (selected_ind > 0)
            {
                if (text != string.Empty) changes.Add("notice%" + selected_ind + '%' + TimeList.SelectedIndex + '%' + text);
                else changes.Add("notice%" + selected_ind + '%' + TimeList.SelectedIndex + '%' + "NULL");
            }
        }
        /// <summary>
        /// 双击新建一个指定日期的时间表 ##更改数据##
        /// </summary>
        private void PickDate_Click(object sender, RoutedEventArgs e)
        {
            changes.Add("new%" + "1 3 5" + '%' + "2020/02/02" + '%');
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

            foreach (TimeTable.Table t in tables) converted.Add(TimeTable.Table2Entry(t)); // 转换时间表
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
            MsgBox.Show(String.Join("\n", OtherTools.Duplicate_Removal(changes)));
            changes.Clear();
        }
        #endregion
    }
}
