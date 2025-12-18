using System.Windows.Controls;
using System.Windows.Input;
using WpfApp10.Models;
using WpfApp10.ViewModels;

namespace WpfApp10.Views
{
    public partial class CalendarView : UserControl
    {
        public CalendarView()
        {
            InitializeComponent();
        }

        private CalendarViewModel VM => DataContext as CalendarViewModel;

        private void PrevMonth_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            VM?.PrevMonth();
        }

        private void NextMonth_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            VM?.NextMonth();
        }

        private void Day_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border &&
                border.DataContext is CalendarDayItem day)
            {
                VM?.DayClicked(day);
            }
        }
    }
}
