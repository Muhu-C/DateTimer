using DateTimer.View.CustomControls;
using HandyControl.Themes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static DateTimer.Utils;

namespace DateTimer.View
{
    /// <summary>
    /// NotePage.xaml 的交互逻辑
    /// </summary>
    public partial class NotePage : Page
    {
        public NotePageBindContent viewModel = new NotePageBindContent();
        public static NoteFile CurNote;
        public NewNoteWindow newNoteWindow;
        public EditNoteWindow editNoteWindow;

        public NotePage()
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.Entries = new ObservableCollection<NoteEntry>();
            newNoteWindow = new NewNoteWindow();
            editNoteWindow = new EditNoteWindow();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.TextColor = HomePage.viewModel.TextColor;
            if (App.ConfigData.Theme == 0) Theme.SetSkin(this, HandyControl.Data.SkinType.Dark);
            else Theme.SetSkin(this, HandyControl.Data.SkinType.Default);
            LoadFile();
        }

        public void LoadFile()
        {
            CurNote = GetNotes("Data\\Config\\notes.json");
            viewModel.Entries.Clear();
            foreach (Note note in CurNote.notes)
                viewModel.Entries.Add(Note2Entry(note));
            GetUndoneList();
        }

        private void NewNoteButton_Click(object sender, RoutedEventArgs e)
        {
            newNoteWindow.Show();
        }

        private void EditNote_Click(object sender, RoutedEventArgs e)
        {
            editNoteWindow.Init(NoteList.SelectedIndex);
        }

        private void DeleteNote_Click(object sender, RoutedEventArgs e)
        {
            CurNote.notes.RemoveAt(NoteList.SelectedIndex);
            WriteNotes(CurNote, "Data\\Config\\notes.json");
            LoadFile();
            HandyControl.Controls.Growl.SuccessGlobal("待办删除成功! ");
        }

        public class NotePageBindContent : TimeTable.ViewModelBase
        {
            private Brush textcolor;
            public Brush TextColor
            {
                get { return textcolor; }
                set
                {
                    if (textcolor != value)
                    {
                        textcolor = value;
                        RaisePropertyChangedEvent("TextColor");
                    }
                }
            }

            private ObservableCollection<NoteEntry> entries;
            public ObservableCollection<NoteEntry> Entries
            {
                get { return entries; }
                set
                {
                    if (entries != value)
                    {
                        entries = value;
                        RaisePropertyChangedEvent("Entries");
                    }
                }
            }
        }

        #region 函数定义
        public class NoteFile
        {
            public List<Note> notes { get; set; }
        }

        public class Note
        {
            public string date { get; set; }
            public string weekday { get; set; }
            public string span { get; set; }
            public string title { get; set; }
            public string note { get; set; }
        }

        private static NoteFile GetNotes(string Path)
        {
            LogTool.WriteLog("Note -> 获取待办", LogTool.LogType.Info);
            string JsonStr = FileProcess.ReadFile(Path);
            return JsonConvert.DeserializeObject<NoteFile>(JsonStr);
        }

        private static void WriteNotes(NoteFile file, string Path)
        {
            LogTool.WriteLog("Note -> 写入待办", LogTool.LogType.Info);
            string json = JsonConvert.SerializeObject(file);
            FileProcess.WriteFile(json, Path);
        }

        private void GetUndoneList()
        {
            List<UndoneNoteEntry> undones = new List<UndoneNoteEntry>();
            foreach (Note note in CurNote.notes)
            {
                if (note.date == "default") continue;
                TimeSpan timeSpan = (note.span == "default") ? TimeSpan.Zero : TimeConverter.Str2Time(note.span);
                DateTime DT = TimeConverter.Str2Date(note.date);
                if (DT >= DateTime.Today && timeSpan == TimeSpan.Zero || DT + timeSpan > DateTime.Now)
                    undones.Add(new UndoneNoteEntry
                    {
                        Date = string.Join("/", note.date.Split(' ')),
                        Name = note.title,
                        Span = (timeSpan == TimeSpan.Zero) ? "" : TimeConverter.Time2Str(timeSpan, ":")
                    });
            }
            UndoneNotesList.ItemsSource = undones;
        }

        private NoteEntry Note2Entry(Note note)
        {
            LogTool.WriteLog("Note -> 待办基类转显示类", LogTool.LogType.Info);
            NoteEntry entry = new NoteEntry();
            if (note.date == "default")
                entry.time = (note.weekday == "default") ? "未设置" : TimeTable.GetWeekday(note.weekday);
            else entry.time = string.Join("/", note.date.Split(' '));

            entry.span = (note.span == "default") ? "未设置" : note.span;
            entry.note = (note.note == "default") ? "无描述" : note.note;
            
            entry.title = note.title;
            return entry;
        }

        private List<UndoneNoteEntry> NoteTimeSort(List<UndoneNoteEntry> undones)
        {
            List<UndoneNoteEntry> sorted = undones;
            int lenofnotes = sorted.Count;
        }
        #endregion

        #region 右击选中
        private void ListViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 使右击列表时选中当前内容
            if (VisualUpwardSearch<ListViewItem>(e.OriginalSource as DependencyObject) is ListViewItem listViewItem)
            {
                NoteList.SelectedIndex = -1; // 取消选择
                listViewItem.IsSelected = true; // 重新选择
                e.Handled = true;
            }
        }

        static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T)) source = VisualTreeHelper.GetParent(source);
            return source;
        }
        #endregion
    }
    public class NoteEntry
    {
        public string time { get; set; }
        public string span { get; set; }
        public string title { get; set; }
        public string note { get; set; }
    }
    public class UndoneNoteEntry
    {
        public string Date { get; set; }
        public string Span { get; set; }
        public string Name { get; set; }
    }
}
