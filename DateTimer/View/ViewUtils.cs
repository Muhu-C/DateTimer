using System;
using System.Threading;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Navigation;
using System.Windows.Media;
using HandyControl.Tools.Extension;
using System.Windows.Markup;

namespace DateTimer.View
{
    public class ViewUtils
    {
        /// <summary> 更改事件 </summary>
        public class ChangeEvent
        {
            public string ChangeContent { get; set; }
            public string ChangeClass { get; set; }
            public int ChangeDate { get; set; }
            public int ChangeTime { get; set; }
        }

        /// <summary> 新建时间表事件 </summary>
        public class NewTableEvent
        {
            public bool Mode { get; set; }
            public string Date { get; set; }
            public string WDay { get; set; }
        }
    }
}
