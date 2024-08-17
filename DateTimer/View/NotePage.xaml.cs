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
        public static List<UndoneNoteEntry> UndoneNotes;
        public NewNoteWindow newNoteWindow;
        public EditNoteWindow editNoteWindow;
        public static int todayNote = 0;


        public NotePage()
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.Entries = new ObservableCollection<NoteEntry>();
            newNoteWindow = new NewNoteWindow();
            editNoteWindow = new EditNoteWindow();
            UndoneNotes = new List<UndoneNoteEntry>();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.TextColor = HomePage.viewModel.TextColor;
            if (App.ConfigData.Theme == 0) 
                Theme.SetSkin(this, HandyControl.Data.SkinType.Dark);
            else 
                Theme.SetSkin(this, HandyControl.Data.SkinType.Default);

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
            // 新建待办
            newNoteWindow.Show();
        }

        private void EditNote_Click(object sender, RoutedEventArgs e)
        {
            // 编辑待办
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
            return JsonConvert.DeserializeObject<NoteFile>(FileProcess.ReadFile(Path));
        }

        private static void WriteNotes(NoteFile file, string Path)
        {
            LogTool.WriteLog("Note -> 写入待办", LogTool.LogType.Info);
            FileProcess.WriteFile(JsonConvert.SerializeObject(file), Path);
        }

        private void GetUndoneList()
        {
            UndoneNotes.Clear();

            // 获取日期为主的待办
            foreach (Note note in CurNote.notes)
            {
                if (note.date == "default") continue;
                TimeSpan timeSpan = (note.span == "default") ? TimeSpan.Zero : TimeConverter.Str2Time(note.span);
                DateTime DT = TimeConverter.Str2Date(note.date);
                if (DT >= DateTime.Today && timeSpan == TimeSpan.Zero || DT + timeSpan > DateTime.Now)
                {
                    if (DT == DateTime.Today && timeSpan == TimeSpan.Zero || DT == DateTime.Today && DT + timeSpan > DateTime.Now)
                    {

                    }
                    UndoneNotes.Add(new UndoneNoteEntry
                    {
                        Date = string.Join("/", note.date.Split(' ')),
                        Name = note.title,
                        Span = (timeSpan == TimeSpan.Zero) ? string.Empty : TimeConverter.Time2Str(timeSpan, ":")
                    });
                }
            }
            UndoneNotes = NoteTimeSort(UndoneNotes);

            // 获取星期日为主的待办
            foreach (Note note1 in CurNote.notes)
            {
                if (note1.weekday == "default") continue;
                TimeSpan timeSpan = (note1.span == "default") ? TimeSpan.Zero : TimeConverter.Str2Time(note1.span);
                if (Convert.ToInt32(note1.weekday) % 7 == Convert.ToInt32(DateTime.Today.DayOfWeek))
                {
                    UndoneNotes.Insert(0, new UndoneNoteEntry
                    {
                        Date = TimeTable.GetWeekday(note1.weekday),
                        Name = note1.title,
                        Span = (timeSpan == TimeSpan.Zero) ? string.Empty : TimeConverter.Time2Str(timeSpan, ":")
                    });
                }
            }

            UndoneNotesList.ItemsSource = UndoneNotes;
        }

        private NoteEntry Note2Entry(Note note)
        {
            LogTool.WriteLog("Note -> 待办基类转显示类", LogTool.LogType.Info);
            NoteEntry entry = new NoteEntry();

            if (note.date == "default") entry.time = (note.weekday == "default") ? "未设置" : TimeTable.GetWeekday(note.weekday);
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

            // 通过冒泡排序按时间排出顺序
            for (int i = 0; i < lenofnotes; i++)
            {
                for (int j = 0; j < lenofnotes - i - 1; j++)
                {
                    DateTime time1 = TimeConverter.Str2Date(sorted[j].Date, "/") + ((sorted[j].Span == string.Empty) ? TimeSpan.Zero : TimeConverter.Str2Time(sorted[j].Span, ':'));
                    DateTime time2 = TimeConverter.Str2Date(sorted[j + 1].Date, "/") + ((sorted[j + 1].Span == string.Empty) ? TimeSpan.Zero : TimeConverter.Str2Time(sorted[j + 1].Span, ':'));
                    if (time1 > time2)
                        (sorted[j], sorted[j + 1]) = (sorted[j + 1], sorted[j]);
                }
            }

            return sorted;
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
