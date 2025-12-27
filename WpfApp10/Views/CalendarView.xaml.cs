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

            DataContext = new CalendarViewModel();
        }

        private void Day_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CalendarViewModel vm &&
                sender is Border b &&
                b.DataContext is CalendarDayItem day)
            {
                vm.SelectedDay = day;
            }
        }
    }
}
