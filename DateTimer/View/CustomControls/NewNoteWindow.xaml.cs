using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HandyControl;
using HandyControl.Themes;
using Newtonsoft.Json;
using static DateTimer.Utils;

namespace DateTimer.View.CustomControls
{
    /// <summary>
    /// NewNoteWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NewNoteWindow : HandyControl.Controls.Window
    {
        NotePage.Note NewNote;
        public NewNoteWindow()
        {
            InitializeComponent();
            NewNote = new NotePage.Note
            {
                date="default",
                note="default",
                span="default",
                weekday="default",
                title=string.Empty
            };
            if (App.ConfigData.Theme == 0) Theme.SetSkin(this, HandyControl.Data.SkinType.Dark);
            else Theme.SetSkin(this, HandyControl.Data.SkinType.Default);
        }

        private void DatePicker_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Dp.SelectedDate == null)
            {
                NewNote.date = "default";
                return;
            }
            NewNote.date = ((DateTime)Dp.SelectedDate).ToString("yyyy MM dd");
        }

        private void TimePicker_SelectedTimeChanged(object sender, HandyControl.Data.FunctionEventArgs<DateTime?> e)
        {
            if (Tp.SelectedTime == null)
            {
                NewNote.span = "default";
                return;
            }
            NewNote.span = ((DateTime)Tp.SelectedTime).ToString("HH mm");
        }

        private void ClearDate_Click(object sender, RoutedEventArgs e)
        {
            Dp.SelectedDate = null;
        }

        private void Cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NewNote == null) return;
            if (Cb.SelectedIndex <= 0) NewNote.weekday = "default";
            else NewNote.weekday = Cb.SelectedIndex.ToString();
        }

        private void TbName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (OtherTools.CountStrLen(TbName.Text) > 20)
            {
                TbName.Text = OtherTools.LimitStrLen(TbName.Text, 20);
                TbName.CaretIndex = TbName.Text.Length - 1;
            }
            NewNote.title = TbName.Text;
        }

        private void ClearTime_Click(object sender, RoutedEventArgs e)
        {
            Tp.SelectedTime = null;
            Tp.DisplayTime = DateTime.Now;
        }

        private void TbDescription_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (TbDescription.Text == string.Empty) NewNote.note = "default";
            else if (TbDescription.Text == "default")
            {
                TbDescription.Text = string.Empty;
                NewNote.note = "default";
            }
            else
            {
                if (OtherTools.CountStrLen(TbDescription.Text) > 40)
                {
                    TbDescription.Text = OtherTools.LimitStrLen(TbDescription.Text, 40);
                    TbDescription.CaretIndex = TbDescription.Text.Length - 1;
                }
                NewNote.note = TbDescription.Text;
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (NewNote.title == string.Empty)
            {
                HandyControl.Controls.Growl.Error("未设置事件名! ");
            }
            else
            {
                NotePage.CurNote.notes.Add(NewNote);
                string json = JsonConvert.SerializeObject(NotePage.CurNote);
                if (NewNote.date != "default" && NewNote.weekday != "default")
                    HandyControl.Controls.Growl.WarningGlobal("请注意: 待办事项优先判定日期! ");
                FileProcess.WriteFile(json, "Data\\Config\\notes.json");
                (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow).Note.LoadFile();
                Close();
                HandyControl.Controls.Growl.SuccessGlobal("待办设置成功! ");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NewNote.date = "default";
            NewNote.title = string.Empty;
            NewNote.note = "default";
            NewNote.span = "default";
            NewNote.weekday = "default";
            TbName.Text = string.Empty;
            TbDescription.Text = string.Empty;
            Dp.SelectedDate = null;
            Cb.SelectedIndex = 0;
            Tp.SelectedTime = null;
            e.Cancel = true;
            Hide();
        }
    }
}
