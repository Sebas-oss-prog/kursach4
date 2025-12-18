using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using WpfApp10.Models;

namespace WpfApp10.ViewModels
{
    public class CalendarViewModel : BaseViewModel
    {
        public ObservableCollection<CalendarDayItem> Days { get; private set; }

        private DateTime _currentMonth = DateTime.Today;

        public string MonthTitle
        {
            get { return _currentMonth.ToString("MMMM yyyy", new CultureInfo("ru-RU")); }
        }

        // ===== ТЕСТОВЫЕ ДАННЫЕ =====
        private readonly string[] _projects =
        {
            "Разработка CRM",
            "Редизайн сайта",
            "Мобильное приложение",
            "Интеграция API",
            "Автоматизация отдела"
        };

        private readonly string[] _tasks =
        {
            "Проектирование БД",
            "Разработка API",
            "Верстка интерфейса",
            "Написание тестов",
            "Подготовка документации"
        };

        private readonly string[] _documents =
        {
            "Техническое задание",
            "Отчет по проекту",
            "График работ",
            "Финансовый отчет",
            "Презентация проекта"
        };

        public CalendarViewModel()
        {
            Days = new ObservableCollection<CalendarDayItem>();
            GenerateMonth();
        }

        public void NextMonth()
        {
            _currentMonth = _currentMonth.AddMonths(1);
            GenerateMonth();
        }

        public void PrevMonth()
        {
            _currentMonth = _currentMonth.AddMonths(-1);
            GenerateMonth();
        }

        public void DayClicked(CalendarDayItem day)
        {
            if (day == null || !day.HasAny)
                return;

            string text =
                "📅 " + day.Date.ToString("dd MMMM yyyy") + "\n\n" +
                "Задачи:\n" + (day.Tasks.Any() ? string.Join("\n", day.Tasks) : "—") + "\n\n" +
                "Проекты:\n" + (day.Projects.Any() ? string.Join("\n", day.Projects) : "—") + "\n\n" +
                "Документы:\n" + (day.Documents.Any() ? string.Join("\n", day.Documents) : "—");

            MessageBox.Show(text, "События дня");
        }

        private void GenerateMonth()
        {
            Days.Clear();

            DateTime firstDay = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(_currentMonth.Year, _currentMonth.Month);

            for (int i = 0; i < daysInMonth; i++)
            {
                DateTime date = firstDay.AddDays(i);
                CalendarDayItem day = new CalendarDayItem();
                day.Date = date;

                if (date.Day % 2 == 0)
                    day.Tasks.Add(_tasks[date.Day % _tasks.Length]);

                if (date.Day % 3 == 0)
                    day.Tasks.Add(_tasks[(date.Day + 1) % _tasks.Length]);

                if (date.Day % 5 == 0)
                    day.Projects.Add(_projects[date.Day % _projects.Length]);

                if (date.Day % 7 == 0)
                    day.Documents.Add(_documents[date.Day % _documents.Length]);

                Days.Add(day);
            }

            OnPropertyChanged("MonthTitle");
        }
    }
}
