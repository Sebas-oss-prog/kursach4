using System;
using System.Collections.ObjectModel;
using WpfApp10.Models;

public class CalendarDayItem
{
    public DateTime Date { get; set; }

    public int Day => Date.Day;

    // 🔴 БЫЛО: string
    // 🔵 СТАЛО: TaskModel
    public ObservableCollection<TaskModel> Tasks { get; }
        = new ObservableCollection<TaskModel>();

    public ObservableCollection<ProjectModel> Projects { get; }
        = new ObservableCollection<ProjectModel>();

    public ObservableCollection<DocumentModel> Documents { get; }
        = new ObservableCollection<DocumentModel>();

    public bool HasAny =>
        Tasks.Count > 0 ||
        Projects.Count > 0 ||
        Documents.Count > 0;
}
