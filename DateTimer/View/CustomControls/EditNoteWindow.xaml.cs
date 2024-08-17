using HandyControl.Themes;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static DateTimer.Utils;

namespace DateTimer.View.CustomControls
{
    /// <summary>
    /// EditNoteWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EditNoteWindow : HandyControl.Controls.Window
    {
        int changeindex;
        NotePage.Note NewNote;
        public EditNoteWindow()
        {
            InitializeComponent();
            if (App.ConfigData.Theme == 0) Theme.SetSkin(this, HandyControl.Data.SkinType.Dark);
            else Theme.SetSkin(this, HandyControl.Data.SkinType.Default);
        }

        public void Init(int ind)
        {
            changeindex = ind;
            NewNote = new NotePage.Note
            {
                date = NotePage.CurNote.notes[ind].date,
                note = NotePage.CurNote.notes[ind].note,
                span = NotePage.CurNote.notes[ind].span,
                weekday = NotePage.CurNote.notes[ind].weekday,
                title = NotePage.CurNote.notes[ind].title
            };
            Show();
            TbName.Text = NewNote.title;
            TbDescription.Text = NewNote.note;
            if (NewNote.date != "default") Dp.SelectedDate = TimeConverter.Str2Date(NewNote.date);
            if (NewNote.span != "default") Tp.SelectedTime = new DateTime(TimeConverter.Str2Time(NewNote.span).Ticks);
            if (NewNote.weekday != "default") Cb.SelectedIndex = Convert.ToInt32(NewNote.weekday);
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
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

        private void TbName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Utils.OtherTools.CountStrLen(TbName.Text) > 20)
            {
                TbName.Text = TbName.Text.Substring(0, 10);
            }
            NewNote.title = TbName.Text;
        }

        private void ClearTime_Click(object sender, RoutedEventArgs e)
        {
            Tp.SelectedTime = null;
            Tp.DisplayTime = DateTime.Now;
        }

        private void Cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NewNote == null) return;
            if (Cb.SelectedIndex <= 0) NewNote.weekday = "default";
            else NewNote.weekday = Cb.SelectedIndex.ToString();
        }

        private void TbDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TbDescription.Text == string.Empty) NewNote.note = "default";
            else NewNote.note = TbDescription.Text;
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
                NotePage.CurNote.notes.RemoveAt(changeindex);
                NotePage.CurNote.notes.Add(NewNote);
                if (NewNote.date != "default" && NewNote.weekday != "default")
                    HandyControl.Controls.Growl.WarningGlobal("请注意: 待办事项优先判定日期! ");
                string json = JsonConvert.SerializeObject(NotePage.CurNote);
                FileProcess.WriteFile(json, "Data\\Config\\notes.json");
                (Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow).Note.LoadFile();
                Close();
                HandyControl.Controls.Growl.SuccessGlobal("待办设置成功! ");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
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
