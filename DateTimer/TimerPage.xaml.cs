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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MsgBox = HandyControl.Controls.MessageBox;
using HandyControl.Themes;

namespace DateTimer
{
    /// <summary>
    /// TimerPage.xaml 的交互逻辑
    /// </summary>
    public partial class TimerPage : Page
    {
        public TimerPage()
        {
            InitializeComponent();
            DataContext = HomePage.viewModel; // 使用 HomePage 的 BindingContent
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Theme.SetSkin(this, Theme.GetSkin(MainWindow.Cur));
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TPStart_SelectedTimeChanged(object sender, HandyControl.Data.FunctionEventArgs<DateTime?> e)
        {

        }

        private void TPEnd_SelectedTimeChanged(object sender, HandyControl.Data.FunctionEventArgs<DateTime?> e)
        {

        }

        private void PickDate_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
