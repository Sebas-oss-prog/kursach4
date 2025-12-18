using System;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace WpfApp10.Models
{
    public class CalendarDayItem
    {
        public DateTime Date { get; set; }

        public int Day => Date.Day;

        public ObservableCollection<string> Tasks { get; }
        public ObservableCollection<string> Projects { get; }
        public ObservableCollection<string> Documents { get; }

        public bool HasTasks => Tasks.Count > 0;
        public bool HasProjects => Projects.Count > 0;
        public bool HasDocuments => Documents.Count > 0;

        public bool HasAny => HasTasks || HasProjects || HasDocuments;

        // 🎨 Цвет точки
        public Brush DotColor
        {
            get
            {
                if (HasProjects) return Brushes.MediumPurple;
                if (HasTasks) return Brushes.DodgerBlue;
                if (HasDocuments) return Brushes.SeaGreen;
                return Brushes.Transparent;
            }
        }

        public CalendarDayItem()
        {
            Tasks = new ObservableCollection<string>();
            Projects = new ObservableCollection<string>();
            Documents = new ObservableCollection<string>();
        }
    }
}
