using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        private void TbName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            NewNote.title = TbName.Text;
        }

        private void ClearTime_Click(object sender, RoutedEventArgs e)
        {
            Tp.SelectedTime = null;
        }

        private void TbDescription_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
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
