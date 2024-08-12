using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using MsgBox = HandyControl.Controls.MessageBox;
using HandyControl.Themes;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Linq;
using HandyControl.Tools.Extension;
using System.Threading.Tasks;
using System.Diagnostics.Eventing.Reader;

namespace DateTimer.View
{
    /// <summary>
    /// TimerPage.xaml 的交互逻辑
    /// </summary>
    public partial class TimerPage : Page
    {
        #region TimerPage变量定义

        /// <summary> 时间表文件 </summary>
        public Utils.TimeTable.TimeTableFile a;

        /// <summary> 选中 "a" 的当前时间表 </summary>
        public List<Utils.TimeTable.Timetables> timetables = new List<Utils.TimeTable.Timetables> ();

        /// <summary> 选中 "a" 的当前时间表的实际内容 </summary>
        public List<Utils.TimeTable.Table> tables = new List<Utils.TimeTable.Table>(); // 选中的时间表

        /// <summary> 时间表下标列表 </summary>
        public List<int> indexes = new List<int>();

        /// <summary> 选中的时间表下标 </summary>
        public static int selected_ind = -1;

        /// <summary> 存入的修改 </summary>
        public static List<ViewUtils.ChangeEvent> changes = new List<ViewUtils.ChangeEvent>();

        public bool isPickDateOpen = false;
        public bool isPickTimeOpen = false;

        public string TimeTableFilePath = string.Empty;

        #endregion

        #region TimerPage加载

        public TimerPage() 
        {
            InitializeComponent();
            DataContext = HomePage.viewModel; // 使用 HomePage 的 BindingContent
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) 
        {
            Theme.SetSkin(this, Theme.GetSkin(Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow));
            Reload_First();
        }

        public void Reload_First()
        {
            #region 初始化
            changes = new List<ViewUtils.ChangeEvent>();
            TPStart.IsEnabled = false;
            TPEnd.IsEnabled = false;
            ElementTb.IsEnabled = false;
            NoticeTb.IsEnabled = false;

            HomePage.viewModel.TableEntries = new ObservableCollection<Utils.TimeTable.TableEntry>();
            TPStart.SelectedTime = DateTime.Now;
            TPEnd.SelectedTime = DateTime.Now;
            ElementTb.Text = string.Empty;
            NoticeTb.Text = string.Empty;
            PosTb.Text = TimeTableFilePath;
            #endregion
            try
            {
                // 判断时间表是否合法
                if (!File.Exists(TimeTableFilePath))
                {
                    EditGrid.Hide();
                    return;
                }
                else EditGrid.Show();

                // 获取时间表
                try { a = Utils.TimeTable.GetTimetables(TimeTableFilePath); }
                catch
                {
                    a = new Utils.TimeTable.TimeTableFile();
                }

                // 设置时间表
                if (a != null)
                {
                    timetables = a.timetables;
                    Reload_Second();
                }
            }
            catch (Exception ex) { App.Error("无", App.ErrorType.UnknownError, ex, false); }
        }

        private void Reload_Second()
        {
            int i = 0;
            TimeSel.Items.Clear();
            if (timetables != null)
            {
                // 在下拉列表里添加日期
                foreach (Utils.TimeTable.Timetables table in timetables)
                {
                    ComboBoxItem item = new ComboBoxItem();
                    if (table.date != "GENERAL")
                        item.Content = table.date;
                    else item.Content = Utils.TimeTable.GetWeekday(table.weekday);
                    indexes.Add(i);
                    item.Tag = i;
                    TimeSel.Items.Add(item);
                    i++;
                }
            }
        }
        #endregion

        #region 选择时间段
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int a = TimeSel.SelectedIndex;
            if(a >= 0)
            {
                TPStart.SelectedTime = new DateTime(Utils.TimeConverter.Int2Time(Utils.TimeConverter.Str2TimeInt(tables[a].start)).Ticks);
                TPEnd.SelectedTime = new DateTime(Utils.TimeConverter.Int2Time(Utils.TimeConverter.Str2TimeInt(tables[a].end)).Ticks);
                ElementTb.Text = tables[a].name;
                if (tables[a].notice != "NULL") NoticeTb.Text = tables[a].notice;
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

        /// <summary> 开始时间 </summary>
        private void TPStart_SelectedTimeChanged(object sender, HandyControl.Data.FunctionEventArgs<DateTime?> e)
        {
            // 设置开始时间
            string starttime = TPStart.SelectedTime.Value.ToString("HH mm");
            try 
            {
                if (selected_ind >= 0 && TPStart.IsEnabled) 
                    changes.Add(new ViewUtils.ChangeEvent { ChangeClass = "start", ChangeContent = starttime, ChangeDate = selected_ind, ChangeTime = TimeList.SelectedIndex });
            }
            catch(Exception ex) 
            {
                App.Error("即将关闭程序", App.ErrorType.UnknownError, ex, true);
            }
        }

        /// <summary> 结束时间 </summary>
        private void TPEnd_SelectedTimeChanged(object sender, HandyControl.Data.FunctionEventArgs<DateTime?> e)
        {
            // 设置结束时间
            string endtime = TPEnd.SelectedTime.Value.ToString("HH mm");
            try 
            {
                if (selected_ind >= 0 && TPEnd.IsEnabled)
                    changes.Add(new ViewUtils.ChangeEvent { ChangeClass = "end", ChangeContent = endtime, ChangeDate = selected_ind, ChangeTime = TimeList.SelectedIndex });
            }
            catch (Exception ex) 
            {
                App.Error("即将关闭程序", App.ErrorType.UnknownError, ex, true);
            }
        }

        /// <summary> 事件名称 </summary>
        private void ElementTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 设置事件名
            string text = ElementTb.Text;
            try 
            {
                if (selected_ind >= 0 && ElementTb.IsEnabled)
                    changes.Add(new ViewUtils.ChangeEvent { ChangeClass = "name", ChangeContent = text, ChangeDate = selected_ind, ChangeTime = TimeList.SelectedIndex });
            }
            catch (Exception ex) 
            {
                App.Error("即将关闭程序", App.ErrorType.UnknownError, ex, true);
            }
        }

        /// <summary> 事件提示 </summary>
        private void NoticeTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 设置提示
            string text = NoticeTb.Text;
            if (selected_ind >= 0 && NoticeTb.IsEnabled)
            {
                try
                {
                    if (text != string.Empty) changes.Add(new ViewUtils.ChangeEvent { ChangeClass = "notice", ChangeContent = text, ChangeDate = selected_ind, ChangeTime = TimeList.SelectedIndex });
                    else changes.Add(new ViewUtils.ChangeEvent { ChangeClass = "notice", ChangeContent = text, ChangeDate = selected_ind, ChangeTime = TimeList.SelectedIndex });
                }
                catch (Exception ex) { App.Error("即将关闭程序", App.ErrorType.UnknownError, ex, true); }
            }
        }

        /// <summary> (未完成)双击新建一个指定日期的时间表 ##更改数据## </summary>
        private async void PickDate_Click(object sender, RoutedEventArgs e)
        {
            isPickDateOpen = true;
            NewTimeTableWindow newTime = new NewTimeTableWindow();
            newTime.Show();

            // 异步等待
            await Task.Run(async () => 
            {
                while (isPickDateOpen) await Task.Delay(200);
                return;
            });

            Reload_Second();
            newTime = null;
        }

        private void NewTime_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region 选择日期
        private void ItemClick(object sender, RoutedEventArgs e)
        {
            // 更新选中的时间表
            ComboBoxItem item = TimeSel.SelectedItem as ComboBoxItem;
            if (item == null) return;
            selected_ind = Convert.ToInt32(item.Tag);
            ObservableCollection<Utils.TimeTable.TableEntry> converted = new ObservableCollection<Utils.TimeTable.TableEntry>();
            tables = timetables[selected_ind].tables;

            // 显示日期或时间
            string date = timetables[selected_ind].date; 
            string wday = Utils.TimeTable.GetWeekday(timetables[selected_ind].weekday);
            if (date != "GENERAL") SelectedTb.Text = date;
            else SelectedTb.Text = wday;

            // 转换与显示时间表
            foreach (Utils.TimeTable.Table t in tables) { converted.Add(Utils.TimeTable.Table2Entry(t)); }
            HomePage.viewModel.TableEntries = converted; 
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 更改时间表 json
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "配置文件 (可新建)|*.json";

            // 判断路径合法
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (timetables.Count != 0)
                    if (MsgBox.Show("更新文件将放弃此前的修改，是否更新？", "提示", MessageBoxButton.YesNo) == MessageBoxResult.No)
                        return;

                TimeTableFilePath = openFileDialog.FileName;
                Reload_First();
            }
        }

        #region 保存
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (object s in changes) // 保存修改（二期工程）
                {
                    ViewUtils.ChangeEvent change = (ViewUtils.ChangeEvent)s;
                    Console.WriteLine($"更改 {change.ChangeDate} {change.ChangeTime} {change.ChangeClass} 为 {change.ChangeContent}");
                }
                changes.Clear();
            }
            catch (Exception ex) { App.Error("即将关闭程序", App.ErrorType.UnknownError, ex, true, FeedBack: true); }
        }
        #endregion
    }
}
