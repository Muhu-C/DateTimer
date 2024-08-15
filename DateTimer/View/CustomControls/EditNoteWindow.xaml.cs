using HandyControl.Themes;
using Newtonsoft.Json;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static DateTimer.Utils;

namespace DateTimer.View.CustomControls
{
    /// <summary>
    /// EditNoteWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EditNoteWindow : HandyControl.Controls.Window
    {
        NotePage.Note NewNote;
        public EditNoteWindow(int ind)
        {
            InitializeComponent();
            NewNote = new NotePage.Note
            {
                date = NotePage.CurNote.notes[ind].date,
                note = NotePage.CurNote.notes[ind].note,
                span = NotePage.CurNote.notes[ind].span,
                weekday = NotePage.CurNote.notes[ind].weekday,
                title = NotePage.CurNote.notes[ind].title
            };
            if (App.ConfigData.Theme == 0) Theme.SetSkin(this, HandyControl.Data.SkinType.Dark);
            else Theme.SetSkin(this, HandyControl.Data.SkinType.Default);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TbName.Text = NewNote.title;
            TbDescription.Text = NewNote.note;
            Dp.SelectedDate = TimeConverter.Str2Date(NewNote.date);
            Tp.SelectedTime = new DateTime(TimeConverter.Int2Time(TimeConverter.Str2TimeInt(NewNote.span)).Ticks);
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Dp.SelectedDate == null) return;
            NewNote.date = ((DateTime)Dp.SelectedDate).ToString("yyyy MM dd");
        }

        private void TimePicker_SelectedTimeChanged(object sender, HandyControl.Data.FunctionEventArgs<DateTime?> e)
        {
            if (Tp.SelectedTime == null) return;
            NewNote.span = ((DateTime)Tp.SelectedTime).ToString("HH mm");
        }

        private void ClearDate_Click(object sender, RoutedEventArgs e)
        {
            Dp.SelectedDate = null;
        }

        private void TbName_TextChanged(object sender, TextChangedEventArgs e)
        {
            NewNote.title = TbName.Text;
        }

        private void ClearTime_Click(object sender, RoutedEventArgs e)
        {
            Tp.SelectedTime = null;
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
                HandyControl.Controls.Growl.Error("未设置标题! ");
            }
            else
            {
                NotePage.CurNote.notes.Add(NewNote);
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
