using HandyControl.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MsgBox = HandyControl.Controls.MessageBox;

namespace DateTimer.View.CustomControls
{
    /// <summary>
    /// NewTimeTableWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NewTimeTableWindow : HandyControl.Controls.Window
    {
        private List<string> Days = new List<string>();

        private ViewUtils.NewTableEvent New = new ViewUtils.NewTableEvent();

        public NewTimeTableWindow()
        {
            if (App.ConfigData.Theme == 0) Theme.SetSkin(this, HandyControl.Data.SkinType.Dark);
            else Theme.SetSkin(this, HandyControl.Data.SkinType.Default);
            InitializeComponent();
            LogTool.WriteLog("新建时间表 -> 初始化", LogTool.LogType.Info);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            New.Mode = true;
            CheckBox c = (CheckBox)sender;
            Days.Add((string)c.Tag);
            Days.Sort();
            New.WDay = String.Join(" ", Days);
            InfoText.Text = "星期日 ->" + Utils.TimeTable.GetWeekday(New.WDay);

            if (Days.Count == 0)
            {
                New.WDay = "GENERAL";
                InfoText.Text = "星期日 -> 未选择";
            }
        }


        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            New.Mode = true;
            CheckBox c = (CheckBox)sender;
            Days.Remove((string)c.Tag);
            Days.Sort();
            New.WDay = String.Join(" ", Days);
            InfoText.Text = "星期日 ->" + Utils.TimeTable.GetWeekday(New.WDay);

            if (Days.Count == 0)
            {
                New.WDay = "GENERAL";
                InfoText.Text = "星期日 -> 未选择";
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // 设置模式为日期
            DayPanel.IsEnabled = false;

            DateTime selected = (DateTime)DateT.SelectedDate;
            New.Date = selected.ToString("yyyy MM dd");
            New.Mode = false;
            InfoText.Text = "日期 ->" + New.Date;
        }

        private void ClearDate(object sender, RoutedEventArgs e)
        {
            // 设置模式为星期日
            DateT.SelectedDate = DateTime.Now;
            New.Date = "GENERAL";
            New.Mode = true;
            DayPanel.IsEnabled = true;
            Days.Sort();
            InfoText.Text = "星期日 -> " + Utils.TimeTable.GetWeekday(New.WDay);
            if (Days.Count == 0)
            {
                New.WDay = "GENERAL";
                InfoText.Text = "星期日 -> 未选择";
            }
        }

        private void Commit_Click(object sender, RoutedEventArgs e)
        {
            if (New.Date == "GENERAL" && New.WDay == "GENERAL")
            {
                MsgBox.Error("未选择日期或时间！", "警告");
                return;
            }
            if (New.Mode == true)
            {
                if (MsgBox.Show($"星期日: {Utils.TimeTable.GetWeekday(New.WDay)}", "请核对信息是否正确", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow)
                        .TimerPg.timetables.Add(new Utils.TimeTable.Timetables { date = "GENERAL", weekday = New.WDay, tables = new List<Utils.TimeTable.Table>() });

                    (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow).TimerPg.AddOrDel += $"\n新建时间表 {Utils.TimeTable.GetWeekday(New.WDay)}    ";
                    Close();
                }
            }
            else
            {
                if (MsgBox.Show($"日期: {New.Date}", "请核对信息是否正确", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow)
                        .TimerPg.timetables.Add(new Utils.TimeTable.Timetables { date = New.Date, weekday = "GENERAL", tables = new List<Utils.TimeTable.Table>() });

                    (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow).TimerPg.AddOrDel += $"\n新建时间表 {New.Date}    ";
                    Close();
                }
            }
        }

        private void Cancel_Click(object sender, object e) { Close(); }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LogTool.WriteLog("新建时间表 -> 关闭", LogTool.LogType.Info);
            if (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow == null) { return; }
            e.Cancel = true;
            Days.Clear();
            Cb1.IsChecked = false;
            Cb2.IsChecked = false;
            Cb3.IsChecked = false;
            Cb4.IsChecked = false;
            Cb5.IsChecked = false;
            Cb6.IsChecked = false;
            Cb7.IsChecked = false;
            New.Mode = true;
            New.WDay = "GENERAL";
            New.Date = "GENERAL";
            Hide();
            (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow).TimerPg.isPickDateOpen = false;
        }

        private MediaPlayer player = new MediaPlayer();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            player.Open(new Uri("Data/Media/winshow.wav", UriKind.Relative));
            player.Play();
            New.WDay = "GENERAL";
            New.Date = "GENERAL";
        }
    }
}
