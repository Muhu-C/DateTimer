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

        public List<Utils.TimeTable.Timetables> timetables = new List<Utils.TimeTable.Timetables> ();

        public List<Utils.TimeTable.Table> tables = new List<Utils.TimeTable.Table>(); // 选中的时间表



        /// <summary> 选中的时间表下标 </summary>
        public static int selected_ind = -1;

        /// <summary> 存入的修改 </summary>
        public static List<ViewUtils.ChangeEvent> changes = new List<ViewUtils.ChangeEvent>();

        public bool isPickDateOpen = false;

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

        /// <summary> 从文件开始重载 </summary>
        public void Reload_First()
        {
            // 初始化
            #region 初始化
            TPStart.IsEnabled = false;
            TPEnd.IsEnabled = false;
            ElementTb.IsEnabled = false;
            NoticeTb.IsEnabled = false;
            NewTime.IsEnabled = false;
            DelTable.IsEnabled = false;
            DelTime.IsEnabled = false;

            HomePage.viewModel.TableEntries = new ObservableCollection<Utils.TimeTable.TableEntry>();
            TPStart.SelectedTime = DateTime.Now;
            TPEnd.SelectedTime = DateTime.Now;
            ElementTb.Text = string.Empty;
            NoticeTb.Text = string.Empty;
            PosTb.Text = TimeTableFilePath;
            #endregion
            changes = new List<ViewUtils.ChangeEvent>();
            
            try
            {
                // 判断时间表是否合法
                if (!File.Exists(TimeTableFilePath))
                {
                    EditGrid.Hide();
                    return;
                }
                else EditGrid.Show();

                Utils.TimeTable.TimeTableFile a;

                // 获取时间表
                try { a = Utils.TimeTable.GetTimetables(TimeTableFilePath); }
                catch { a = new Utils.TimeTable.TimeTableFile(); }

                // 设置时间表
                if (a != null)
                {
                    timetables = a.timetables;
                    Reload_Second();
                }
            }
            catch (Exception ex) { App.Error("无", App.ErrorType.UnknownError, ex, false); }
        }

        /// <summary> 从类开始重载 </summary>
        private void Reload_Second()
        {
            #region 初始化
            TPStart.IsEnabled = false;
            TPEnd.IsEnabled = false;
            ElementTb.IsEnabled = false;
            NoticeTb.IsEnabled = false;
            NewTime.IsEnabled = false;
            DelTable.IsEnabled = false;
            DelTime.IsEnabled = false;

            HomePage.viewModel.TableEntries = new ObservableCollection<Utils.TimeTable.TableEntry>();
            TPStart.SelectedTime = DateTime.Now;
            TPEnd.SelectedTime = DateTime.Now;
            ElementTb.Text = string.Empty;
            NoticeTb.Text = string.Empty;
            PosTb.Text = TimeTableFilePath;
            #endregion

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
                    else 
                        item.Content = Utils.TimeTable.GetWeekday(table.weekday);
                    item.Tag = i;
                    TimeSel.Items.Add(item);
                    i++;
                }
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
                {
                    int SelInd = TimeList.SelectedIndex;
                    timetables[selected_ind].tables[TimeList.SelectedIndex].start = starttime;
                    changes.Add(new ViewUtils.ChangeEvent { ChangeClass = "start", ChangeContent = starttime, ChangeDate = selected_ind, ChangeTime = TimeList.SelectedIndex });
                    Reload_Second();
                    TimeSel.SelectedIndex = selected_ind;
                    TimeList.SelectedIndex = SelInd;
                }
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
                {
                    int SelInd = TimeList.SelectedIndex;
                    timetables[selected_ind].tables[TimeList.SelectedIndex].end = endtime;
                    changes.Add(new ViewUtils.ChangeEvent { ChangeClass = "end", ChangeContent = endtime, ChangeDate = selected_ind, ChangeTime = TimeList.SelectedIndex });
                    Reload_Second();
                    TimeSel.SelectedIndex = selected_ind;
                    TimeList.SelectedIndex = SelInd;
                }
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
                {
                    timetables[selected_ind].tables[TimeList.SelectedIndex].name = text;
                    changes.Add(new ViewUtils.ChangeEvent { ChangeClass = "name", ChangeContent = text, ChangeDate = selected_ind, ChangeTime = TimeList.SelectedIndex });
                }
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
                    if (text != string.Empty)
                    {
                        timetables[selected_ind].tables[TimeList.SelectedIndex].notice = text;
                        changes.Add(new ViewUtils.ChangeEvent { ChangeClass = "notice", ChangeContent = text, ChangeDate = selected_ind, ChangeTime = TimeList.SelectedIndex });
                    }
                    else
                    {
                        timetables[selected_ind].tables[TimeList.SelectedIndex].notice = "NULL";
                        changes.Add(new ViewUtils.ChangeEvent { ChangeClass = "notice", ChangeContent = "NULL", ChangeDate = selected_ind, ChangeTime = TimeList.SelectedIndex });
                    }
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
            timetables[selected_ind].tables.Add(new Utils.TimeTable.Table { name = "无", notice = "NULL", start = DateTime.Now.ToString("HH mm"), end = DateTime.Now.ToString("HH mm") });
            Reload_Second();
            TimeSel.SelectedIndex = selected_ind;
        }


        private void DelTable_Click(object sender, RoutedEventArgs e)
        {
            if (MsgBox.Show("是否删除当前时间表?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                timetables.Remove(timetables[selected_ind]);
                Reload_Second();
            }
        }

        private void DelTime_Click(object sender, object e)
        {
            if (MsgBox.Show("是否删除当前时间段?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                timetables[selected_ind].tables.Remove(timetables[selected_ind].tables[TimeList.SelectedIndex]);
                Reload_Second();
                TimeSel.SelectedIndex = selected_ind;
            }
        }

        #endregion

        #region 选择 - 时间段
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int a = TimeList.SelectedIndex;
            if (a >= 0)
            {
                TPStart.SelectedTime = new DateTime(Utils.TimeConverter.Int2Time(Utils.TimeConverter.Str2TimeInt(tables[a].start)).Ticks);
                TPEnd.SelectedTime = new DateTime(Utils.TimeConverter.Int2Time(Utils.TimeConverter.Str2TimeInt(tables[a].end)).Ticks);
                ElementTb.Text = tables[a].name;
                if (tables[a].notice != "NULL") NoticeTb.Text = tables[a].notice;
                else NoticeTb.Text = String.Empty;

                DelTime.IsEnabled = true;
                TPStart.IsEnabled = true;
                TPEnd.IsEnabled = true;
                ElementTb.IsEnabled = true;
                NoticeTb.IsEnabled = true;
            }
            else
            {
                DelTime.IsEnabled = false;
                TPStart.IsEnabled = false;
                TPEnd.IsEnabled = false;
                ElementTb.IsEnabled = false;
                NoticeTb.IsEnabled = false;
            }
        }
        #endregion

        #region 选择 - 日期

        private void ItemClick(object sender, RoutedEventArgs e)
        {
            // 更新选中的时间表
            ComboBoxItem item = TimeSel.SelectedItem as ComboBoxItem;
            if (item == null) return;

            NewTime.IsEnabled = true;
            DelTable.IsEnabled = true;

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
                List<ViewUtils.ChangeEvent> changes_sorted = Utils.OtherTools.Duplicate_Removal(changes);
                string str = "将会应用以下更改: \n";

                foreach (ViewUtils.ChangeEvent change in changes_sorted)
                {
                    str += "    ";
                    if (timetables[change.ChangeDate].date != "GENERAL") str += timetables[change.ChangeDate].date;
                    else str += Utils.TimeTable.GetWeekday(timetables[change.ChangeDate].weekday);

                    str += $" {Utils.TimeConverter.JsonTime2DisplayTime(timetables[change.ChangeDate].tables[change.ChangeTime].start)} ~ {Utils.TimeConverter.JsonTime2DisplayTime(timetables[change.ChangeDate].tables[change.ChangeTime].end)}";

                    switch (change.ChangeClass)
                    {
                        case "name":
                            str += " 事件名 -> ";
                            break;
                        case "notice":
                            str += " 事件提示 -> ";
                            break;
                        case "start":
                            str += " 开始时间 -> ";
                            break;
                        case "end":
                            str += " 结束时间 -> ";
                            break;
                    }

                    str += $"{change.ChangeContent} \n";
                }
                if (MsgBox.Show(str, "提示", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    Utils.TimeTable.TimeTableFile file = new Utils.TimeTable.TimeTableFile { timetables=timetables };
                    string fileStr = JsonConvert.SerializeObject(file);
                    fileStr = Utils.TimeTable.Json_Optimization(fileStr);
                    Utils.FileProcess.WriteFile(fileStr, TimeTableFilePath);
                    changes.Clear();
                }
            }
            catch (Exception ex) { App.Error("即将关闭程序", App.ErrorType.UnknownError, ex, true, FeedBack: true); }
        }

        #endregion
    }
}
