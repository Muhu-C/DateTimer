using HandyControl.Themes;
using HandyControl.Tools.Extension;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MsgBox = HandyControl.Controls.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace DateTimer.View
{
    /// <summary>
    /// TimerPage.xaml 的交互逻辑
    /// </summary>
    public partial class TimerPage : Page
    {
        #region TimerPage变量定义
        private static DateTime LastChange = DateTime.MinValue;

        public List<Utils.TimeTable.Timetables> timetables = new List<Utils.TimeTable.Timetables>();

        public List<Utils.TimeTable.Table> tables = new List<Utils.TimeTable.Table>(); // 选中的时间表

        public string AddOrDel = string.Empty;

        /// <summary> 存入的修改 </summary>
        public static List<ViewUtils.ChangeEvent> changes = new List<ViewUtils.ChangeEvent>();

        public bool isPickDateOpen = false;

        public string TimeTableFilePath = string.Empty;

        private CustomControls.NewTimeTableWindow newTime;

        #endregion

        #region TimerPage加载

        public TimerPage()
        {
            LogTool.WriteLog("编辑时间表 -> 初始化", LogTool.LogType.Info);
            InitializeComponent();
            newTime = new CustomControls.NewTimeTableWindow();
            DataContext = HomePage.viewModel; // 使用 HomePage 的 BindingContent
            LoadFile(false);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LogTool.WriteLog("编辑时间表 -> 加载", LogTool.LogType.Info);
            Theme.SetSkin(this, Theme.GetSkin(Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow));
            InitUI();
        }

        /// <summary> 从文件开始重载 </summary>
        public void LoadFile(bool second = true)
        {
            LogTool.WriteLog("编辑时间表 -> 重载文件", LogTool.LogType.Info);
            // 初始化
            #region 初始化
            TPStart.IsEnabled = false;
            TPEnd.IsEnabled = false;
            ElementTb.IsEnabled = false;
            NoticeTb.IsEnabled = false;

            NewTime.IsEnabled = false;
            DelTable.IsEnabled = false;
            DelTime.IsEnabled = false;

            AddOrDel = string.Empty;

            TimeSel.Items.Clear();
            HomePage.viewModel.TableEntries = new ObservableCollection<Utils.TimeTable.TableEntry>();

            TPStart.SelectedTime = DateTime.Now;
            TPEnd.SelectedTime = DateTime.Now;
            ElementTb.Text = string.Empty;
            NoticeTb.Text = string.Empty;

            PosTb.Text = TimeTableFilePath;
            SelectedTb.Text = "未选择";
            #endregion
            changes = new List<ViewUtils.ChangeEvent>();

            // 判断时间表是否合法
            if (!File.Exists(TimeTableFilePath))
            {
                EditGrid.Hide();
                return;
            }
            else EditGrid.Show();

            // 获取时间表
            try
            {
                Utils.TimeTable.TimeTableFile a = Utils.TimeTable.GetTimetables(TimeTableFilePath);
                // 设置时间表
                if (a != null)
                {
                    timetables = a.timetables;
                    if (second) InitUI();
                }
            }
            catch
            {
                LogTool.WriteLog("编辑时间表 -> json 文件格式有误", LogTool.LogType.Warn);
                EditGrid.Hide();
                HandyControl.Controls.Growl.Error("json 文件格式有误! ");
            }
        }

        /// <summary> 从类开始重载 </summary>
        private void InitUI()
        {
            LogTool.WriteLog("编辑时间表 -> 重载UI", LogTool.LogType.Info);
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
            SelectedTb.Text = "未选择";
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
                if (TimeSel.SelectedIndex >= 0 && TPStart.IsEnabled)
                {
                    int SelInd = TimeList.SelectedIndex;
                    int SelInd2 = TimeSel.SelectedIndex;
                    timetables[TimeSel.SelectedIndex].tables[TimeList.SelectedIndex].start = starttime;
                    changes.Add(new ViewUtils.ChangeEvent { ChangeClass = "start", ChangeContent = starttime, ChangeDate = TimeSel.SelectedIndex, ChangeTime = TimeList.SelectedIndex });
                    InitUI();
                    TimeSel.SelectedIndex = SelInd2;
                    TimeList.SelectedIndex = SelInd;
                }
            }
            catch (Exception ex)
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
                if (TimeSel.SelectedIndex >= 0 && TPEnd.IsEnabled)
                {
                    int SelInd = TimeList.SelectedIndex;
                    int SelInd2 = TimeSel.SelectedIndex;
                    timetables[TimeSel.SelectedIndex].tables[TimeList.SelectedIndex].end = endtime;
                    changes.Add(new ViewUtils.ChangeEvent { ChangeClass = "end", ChangeContent = endtime, ChangeDate = TimeSel.SelectedIndex, ChangeTime = TimeList.SelectedIndex });
                    InitUI();
                    TimeSel.SelectedIndex = SelInd2;
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
                if (TimeSel.SelectedIndex >= 0 && ElementTb.IsEnabled)
                {
                    timetables[TimeSel.SelectedIndex].tables[TimeList.SelectedIndex].name = text;
                    changes.Add(new ViewUtils.ChangeEvent { ChangeClass = "name", ChangeContent = text, ChangeDate = TimeSel.SelectedIndex, ChangeTime = TimeList.SelectedIndex });
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
            if (TimeSel.SelectedIndex >= 0 && NoticeTb.IsEnabled)
            {
                try
                {
                    if (text != string.Empty)
                    {
                        timetables[TimeSel.SelectedIndex].tables[TimeList.SelectedIndex].notice = text;
                        changes.Add(new ViewUtils.ChangeEvent { ChangeClass = "notice", ChangeContent = text, ChangeDate = TimeSel.SelectedIndex, ChangeTime = TimeList.SelectedIndex });
                    }
                    else
                    {
                        timetables[TimeSel.SelectedIndex].tables[TimeList.SelectedIndex].notice = "NULL";
                        changes.Add(new ViewUtils.ChangeEvent { ChangeClass = "notice", ChangeContent = "NULL", ChangeDate = TimeSel.SelectedIndex, ChangeTime = TimeList.SelectedIndex });
                    }
                }
                catch (Exception ex) { App.Error("即将关闭程序", App.ErrorType.UnknownError, ex, true); }
            }
        }

        /// <summary> 新建时间表 </summary>
        private async void PickDate_Click(object sender, RoutedEventArgs e)
        {
            if (!EditGrid.IsVisible) return;
            if (isPickDateOpen) { MsgBox.Warning("新建时间表窗口已打开! ", "提示"); return; }
            isPickDateOpen = true;
            newTime.Show();

            // 异步等待
            await Task.Run(async () =>
            {
                while (isPickDateOpen) await Task.Delay(200);
                return;
            });

            InitUI();
            TimeSel.SelectedIndex = -1;
        }

        private void NewTime_Click(object sender, RoutedEventArgs e)
        {
            int selind = TimeSel.SelectedIndex;
            if (TimeSel.SelectedIndex < 0) return;
            AddOrDel += $"\n新建时间段 ";
            if (timetables[TimeSel.SelectedIndex].date != "GENERAL") AddOrDel += timetables[TimeSel.SelectedIndex].date;
            else AddOrDel += Utils.TimeTable.GetWeekday(timetables[TimeSel.SelectedIndex].weekday);
            AddOrDel += "    ";

            string lastend, newendstr;
            if (timetables[TimeSel.SelectedIndex].tables.Count == 0)
            {
                lastend = "07 00";
                newendstr = "08 30";
            }
            else
            {
                lastend = timetables[TimeSel.SelectedIndex].tables[timetables[TimeSel.SelectedIndex].tables.Count - 1].end;
                TimeSpan newend = Utils.TimeConverter.Str2Time(lastend);
                if (App.ConfigData.Front_Min <= 8) newend += TimeSpan.FromMinutes(30);
                else if (App.ConfigData.Front_Min <= 20) newend += TimeSpan.FromHours(1);
                else newend += TimeSpan.FromMinutes(90);
                newendstr = $"{newend.Hours:00} {newend.Minutes:00}";
            }

            timetables[TimeSel.SelectedIndex].tables.Add(new Utils.TimeTable.Table { name = "无", notice = "NULL", start = lastend, end = newendstr });
            InitUI();
            TimeSel.SelectedIndex = selind;
        }


        private void DelTable_Click(object sender, RoutedEventArgs e)
        {
            if (TimeSel.SelectedIndex < 0) return;
            if (MsgBox.Show("是否删除当前时间表?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                AddOrDel += "\n删除时间表 ";
                if (timetables[TimeSel.SelectedIndex].date != "GENERAL") AddOrDel += timetables[TimeSel.SelectedIndex].date;
                else AddOrDel += Utils.TimeTable.GetWeekday(timetables[TimeSel.SelectedIndex].weekday);
                AddOrDel += "    ";

                timetables.Remove(timetables[TimeSel.SelectedIndex]);
                InitUI();
            }
        }

        private void DelTime_Click(object sender, EventArgs e)
        {
            int selind = TimeSel.SelectedIndex;
            if (TimeSel.SelectedIndex < 0) return;
            if (tables.Count == 0) return;
            if (TimeList.SelectedIndex < 0) TimeList.SelectedIndex = tables.Count - 1;
            AddOrDel += "\n删除时间段 ";
            if (timetables[TimeSel.SelectedIndex].date != "GENERAL") AddOrDel += timetables[TimeSel.SelectedIndex].date;
            else AddOrDel += Utils.TimeTable.GetWeekday(timetables[TimeSel.SelectedIndex].weekday);
            AddOrDel += $" {Utils.TimeConverter.JTime2DTime(timetables[TimeSel.SelectedIndex].tables[TimeList.SelectedIndex].start)} ~ {Utils.TimeConverter.JTime2DTime(timetables[TimeSel.SelectedIndex].tables[TimeList.SelectedIndex].end)}";
            AddOrDel += "    ";

            timetables[TimeSel.SelectedIndex].tables.Remove(timetables[TimeSel.SelectedIndex].tables[TimeList.SelectedIndex]);
            InitUI();
            TimeSel.SelectedIndex = selind;
        }

        #endregion

        #region 选择 - 时间段
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int a = TimeList.SelectedIndex;
            if (a >= 0)
            {
                TPStart.SelectedTime = new DateTime(Utils.TimeConverter.Str2Time(tables[a].start).Ticks);
                TPEnd.SelectedTime = new DateTime(Utils.TimeConverter.Str2Time(tables[a].end).Ticks);
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

        #region 选择 - 日期

        private void ItemClick(object sender, RoutedEventArgs e)
        {
            if (TimeSel.SelectedIndex < 0) return;

            NewTime.IsEnabled = true;
            DelTable.IsEnabled = true;
            DelTime.IsEnabled = true;

            ObservableCollection<Utils.TimeTable.TableEntry> converted = new ObservableCollection<Utils.TimeTable.TableEntry>();
            tables = timetables[TimeSel.SelectedIndex].tables;

            // 显示日期或时间
            string date = timetables[TimeSel.SelectedIndex].date;
            string wday = Utils.TimeTable.GetWeekday(timetables[TimeSel.SelectedIndex].weekday);
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
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "配置文件 (可新建)|*.json";

                // 判断路径合法
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (timetables != null)
                        if (timetables.Count != 0)
                            if (MsgBox.Show("更新文件将放弃此前的修改，是否更新？", "提示", MessageBoxButton.YesNo) == MessageBoxResult.No)
                                return;
                    TimeTableFilePath = openFileDialog.FileName;
                    LogTool.WriteLog("编辑时间表 -> 更改配置", LogTool.LogType.Info);
                    LoadFile();
                }
                else openFileDialog.Dispose();
            }
        }

        #region 保存
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // 未打开文件时直接忽略
            if (!EditGrid.IsVisible) return;

            // 3 秒内请勿修改多次
            TimeSpan timeSpan = DateTime.Now - LastChange;
            if (timeSpan.TotalSeconds < 3)
            {
                MsgBox.Error("请勿频繁操作! ");
                return;
            }

            // 开始保存
            LastChange = DateTime.Now;
            // 显示更改
            changes = Utils.OtherTools.Duplicate_Removal(changes);
            string str = "将会应用以下更改: \n";
            foreach (ViewUtils.ChangeEvent change in changes)
            {
                if (timetables[change.ChangeDate].date != "GENERAL") str += timetables[change.ChangeDate].date;
                else str += Utils.TimeTable.GetWeekday(timetables[change.ChangeDate].weekday);
                str += $" {Utils.TimeConverter.JTime2DTime(timetables[change.ChangeDate].tables[change.ChangeTime].start)} ~ {Utils.TimeConverter.JTime2DTime(timetables[change.ChangeDate].tables[change.ChangeTime].end)}";
                switch (change.ChangeClass)
                {
                    case "name": str += " 事件名 -> "; break;
                    case "notice": str += " 事件提示 -> "; break;
                    case "start": str += " 开始时间 -> "; break;
                    case "end": str += " 结束时间 -> "; break;
                }
                str += $"{change.ChangeContent}    \n";
            }
            str += AddOrDel;

            // 保存文件
            if (MsgBox.Show(str, "提示", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.OK)
            {
                LogTool.WriteLog("编辑时间表 -> 保存时间表", LogTool.LogType.Info);
                Utils.TimeTable.TimeTableFile file = new Utils.TimeTable.TimeTableFile { timetables = timetables };
                string fileStr = JsonConvert.SerializeObject(file);
                fileStr = Utils.TimeTable.Json_Optimization(fileStr);
                Utils.FileProcess.WriteFile(fileStr, TimeTableFilePath);
                changes.Clear();
                str = string.Empty;
                AddOrDel = string.Empty;
                (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow).Timer.LoadJson();
                HandyControl.Controls.Growl.Success("时间表保存成功! ");
            }
        }
        #endregion
    }
}
