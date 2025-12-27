using System;
using System.Collections.ObjectModel;
using System.Globalization;
using WpfApp10.Helpers;
using WpfApp10.Models;

namespace WpfApp10.ViewModels
{
    public class CalendarViewModel : BaseViewModel
    {
        public ObservableCollection<CalendarDayItem> Days { get; }
            = new ObservableCollection<CalendarDayItem>();

        private DateTime _currentMonth = DateTime.Today;

        private CalendarDayItem _selectedDay;
        public CalendarDayItem SelectedDay
        {
            get => _selectedDay;
            set
            {
                Set(ref _selectedDay, value);
                IsPanelVisible = value != null && value.HasAny;
                OnPropertyChanged(nameof(SelectedDayTitle));
            }
        }

        private bool _isPanelVisible;
        public bool IsPanelVisible
        {
            get => _isPanelVisible;
            set => Set(ref _isPanelVisible, value);
        }

        private static readonly CultureInfo Ru = new CultureInfo("ru-RU");

        public string MonthTitle =>
            _currentMonth.ToString("MMMM yyyy", Ru);

        public string SelectedDayTitle =>
            SelectedDay?.Date.ToString("dd MMMM yyyy", Ru) ?? "";

        public RelayCommand NextMonthCommand { get; }
        public RelayCommand PrevMonthCommand { get; }
        public RelayCommand ClosePanelCommand { get; }

        public CalendarViewModel()
        {
            NextMonthCommand = new RelayCommand(_ => ChangeMonth(1));
            PrevMonthCommand = new RelayCommand(_ => ChangeMonth(-1));
            ClosePanelCommand = new RelayCommand(_ => ClosePanel());

            GenerateMonth();
        }

        private void ChangeMonth(int delta)
        {
            _currentMonth = _currentMonth.AddMonths(delta);
            GenerateMonth();
            OnPropertyChanged(nameof(MonthTitle));
        }

        private void ClosePanel()
        {
            SelectedDay = null;
            IsPanelVisible = false;
        }

        private void GenerateMonth()
        {
            Days.Clear();

            var tasks = Repositories.GetTasks();
            var projects = Repositories.GetProjects();
            var documents = Repositories.GetDocuments();

            var firstDay = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(_currentMonth.Year, _currentMonth.Month);

            for (int i = 0; i < daysInMonth; i++)
            {
                var date = firstDay.AddDays(i);
                var day = new CalendarDayItem { Date = date };

                foreach (var task in tasks)
                {
                    var d = DateHelper.Parse(task.Deadline);
                    if (d.HasValue && d.Value.Date == date.Date)
                        day.Tasks.Add(task);
                }

                foreach (var project in projects)
                {
                    var d = DateHelper.Parse(project.Deadline);
                    if (d.HasValue && d.Value.Date == date.Date)
                        day.Projects.Add(project);
                }

                foreach (var doc in documents)
                {
                    var d = DateHelper.Parse(doc.CreatedDate);
                    if (d.HasValue && d.Value.Date == date.Date)
                        day.Documents.Add(doc);
                }

                Days.Add(day);
            }
        }
    }
}
