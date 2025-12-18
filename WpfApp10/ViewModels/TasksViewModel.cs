using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using WpfApp10.Helpers;
using WpfApp10.Models;

namespace WpfApp10.ViewModels
{
    public class TasksViewModel : BaseViewModel
    {
        public ObservableCollection<TaskModel> Tasks { get; } =
            new ObservableCollection<TaskModel>();

        public ObservableCollection<EmployeeModel> Employees { get; } =
            new ObservableCollection<EmployeeModel>();

        public ObservableCollection<EmployeeModel> SelectedEmployees { get; } =
            new ObservableCollection<EmployeeModel>();

        private TaskModel _selectedTask;
        public TaskModel SelectedTask
        {
            get => _selectedTask;
            set
            {
                Set(ref _selectedTask, value);
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _isEditorVisible;
        public bool IsEditorVisible
        {
            get => _isEditorVisible;
            set => Set(ref _isEditorVisible, value);
        }

        private bool _isEditMode;
        public string PanelTitle =>
            _isEditMode ? "Редактирование задачи" : "Создание задачи";

        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand CloseCommand { get; }

        public TasksViewModel()
        {
            LoadEmployees();
            LoadTasks();

            AddCommand = new RelayCommand(_ => AddTask());
            EditCommand = new RelayCommand(_ => EditTask(), _ => SelectedTask != null);
            DeleteCommand = new RelayCommand(_ => DeleteTask(), _ => SelectedTask != null);
            SaveCommand = new RelayCommand(_ => SaveTask());
            CloseCommand = new RelayCommand(_ => CloseEditor());
        }

        private void LoadEmployees()
        {
            Employees.Clear();

            using (var con = Database.GetConnection())
            {
                con.Open();
                var cmd = new SQLiteCommand("SELECT Id, FullName, Position FROM Employees", con);
                var r = cmd.ExecuteReader();

                while (r.Read())
                {
                    Employees.Add(new EmployeeModel
                    {
                        Id = r.GetInt32(0),
                        FullName = r.GetString(1),
                        Position = r.GetString(2)
                    });
                }
            }
        }

        private void LoadTasks()
        {
            Tasks.Clear();

            using (var con = Database.GetConnection())
            {
                con.Open();
                var cmd = new SQLiteCommand("SELECT * FROM Tasks", con);
                var r = cmd.ExecuteReader();

                while (r.Read())
                {
                    Tasks.Add(new TaskModel
                    {
                        Id = r.GetInt32(0),
                        ProjectId = r.GetInt32(1),
                        Title = r.GetString(2),
                        Description = r.GetString(3),
                        Status = r.GetString(4),
                        Priority = r.GetString(5),
                        Progress = r.GetInt32(6),
                        AssignedTo = r.GetString(7),
                        Deadline = r.GetString(8)
                    });
                }
            }
        }

        private void AddTask()
        {
            _isEditMode = false;
            SelectedTask = new TaskModel();
            SelectedEmployees.Clear();
            OnPropertyChanged(nameof(PanelTitle));
            IsEditorVisible = true;
        }

        private void EditTask()
        {
            _isEditMode = true;
            SelectedEmployees.Clear();

            foreach (var e in SelectedTask.Employees)
                SelectedEmployees.Add(e);

            OnPropertyChanged(nameof(PanelTitle));
            IsEditorVisible = true;
        }

        private void SaveTask()
        {
            // сохраняем сотрудников
            SelectedTask.Employees.Clear();
            foreach (var e in SelectedEmployees)
                SelectedTask.Employees.Add(e);

            // основной ответственный = первый
            SelectedTask.AssignedTo =
                SelectedEmployees.FirstOrDefault()?.FullName ?? "";

            using (var con = Database.GetConnection())
            {
                con.Open();
                SQLiteCommand cmd;

                if (SelectedTask.Id == 0)
                {
                    cmd = new SQLiteCommand(
                        @"INSERT INTO Tasks
                        (ProjectId, Title, Description, Status, Priority, Progress, AssignedTo, Deadline)
                        VALUES (0,@t,@d,@s,@p,0,@a,@dl)", con);
                }
                else
                {
                    cmd = new SQLiteCommand(
                        @"UPDATE Tasks SET
                        Title=@t, Description=@d, Status=@s,
                        Priority=@p,
                        AssignedTo=@a, Deadline=@dl
                        WHERE Id=@id", con);

                    cmd.Parameters.AddWithValue("@id", SelectedTask.Id);
                }

                cmd.Parameters.AddWithValue("@t", SelectedTask.Title);
                cmd.Parameters.AddWithValue("@d", SelectedTask.Description);
                cmd.Parameters.AddWithValue("@s", SelectedTask.Status);
                cmd.Parameters.AddWithValue("@p", SelectedTask.Priority);
                cmd.Parameters.AddWithValue("@a", SelectedTask.AssignedTo);
                cmd.Parameters.AddWithValue("@dl", SelectedTask.Deadline);

                cmd.ExecuteNonQuery();
            }

            LoadTasks();
            CloseEditor();
        }

        private void DeleteTask()
        {
            using (var con = Database.GetConnection())
            {
                con.Open();
                var cmd = new SQLiteCommand("DELETE FROM Tasks WHERE Id=@id", con);
                cmd.Parameters.AddWithValue("@id", SelectedTask.Id);
                cmd.ExecuteNonQuery();
            }

            Tasks.Remove(SelectedTask);
            SelectedTask = null;
        }

        private void CloseEditor()
        {
            IsEditorVisible = false;
            SelectedTask = null;
            SelectedEmployees.Clear();
        }
    }
}
