﻿using HandyControl.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MsgBox = HandyControl.Controls.MessageBox;

namespace DateTimer.View
{
    /// <summary>
    /// NewTimeTableWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NewTimeTableWindow : HandyControl.Controls.Window
    {
        public List<string> Days = new List<string>();

        public ViewUtils.NewTableEvent New = new ViewUtils.NewTableEvent();

        /// <summary> mode 为 false 时选日期，true 时选星期日 </summary>
        public bool mode = false;

        public NewTimeTableWindow()
        {
            if (App.ConfigData.Theme == 0) Theme.SetSkin(this, HandyControl.Data.SkinType.Dark);
            else Theme.SetSkin(this, HandyControl.Data.SkinType.Default);
            InitializeComponent();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            New.Mode = true;
            mode = true;
            CheckBox c = (CheckBox)sender;
            Days.Add((string)c.Tag);
            New.WDay = String.Join(" ", Days);
            InfoText.Text = "星期日 ->" + String.Join(" ",Days);

            if (Days.Count == 0)
            {
                New.WDay = "";
                InfoText.Text = "星期日 -> 未选择";
            }
        }


        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            New.Mode = true;
            mode = true;
            CheckBox c = (CheckBox)sender;
            Days.Remove((string)c.Tag);
            New.WDay = String.Join(" ", Days);
            InfoText.Text = "星期日 ->" + String.Join(" ", Days);

            if (Days.Count == 0)
            {
                New.WDay = "";
                InfoText.Text = "星期日 -> 未选择";
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // 设置模式为日期
            mode = false;
            DayPanel.IsEnabled = false;

            DateTime selected = (DateTime)DateT.SelectedDate;
            New.Date = selected.ToString("yyyy MM dd");
            New.Mode = mode;
            InfoText.Text = "日期 ->" + New.Date;
        }

        private void ClearDate(object sender, RoutedEventArgs e)
        {
            // 设置模式为星期日
            DateT.SelectedDate = DateTime.Now;
            New.Mode = true;
            mode = true;
            DayPanel.IsEnabled = true;
            InfoText.Text = "星期日 ->v" + String.Join(" ", Days);
        }

        private void Commit_Click(object sender, RoutedEventArgs e)
        {
            if (New.Mode == true) 
            {
                if(MsgBox.Show($"星期日: {New.WDay}", "请核对信息是否正确", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow)
                        .TimerPg.timetables.Add(new Utils.TimeTable.Timetables { date = "GENERAL", weekday = New.WDay, tables = new List<Utils.TimeTable.Table>() });
                    (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow).TimerPg.isPickDateOpen = false;
                    Close();
                }
            }
            else
            {
                if (MsgBox.Show($"日期: {New.Date}", "请核对信息是否正确", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow)
                        .TimerPg.timetables.Add(new Utils.TimeTable.Timetables { date = New.Date, weekday = New.WDay, tables = new List<Utils.TimeTable.Table>() });
                    (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow).TimerPg.isPickDateOpen = false;
                    Close();
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}