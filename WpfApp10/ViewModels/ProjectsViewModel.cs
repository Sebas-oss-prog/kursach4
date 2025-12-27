using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Windows;
using WpfApp10.Helpers;
using WpfApp10.Models;

namespace WpfApp10.ViewModels
{
    public class ProjectsViewModel : BaseViewModel
    {
        public ObservableCollection<ProjectModel> Projects { get; }
            = new ObservableCollection<ProjectModel>();

        public ObservableCollection<EmployeeModel> Employees { get; }
            = new ObservableCollection<EmployeeModel>();

        private ProjectModel _selectedProject;
        public ProjectModel SelectedProject
        {
            get => _selectedProject;
            set
            {
                Set(ref _selectedProject, value);
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
            _isEditMode ? "Редактирование проекта" : "Создание проекта";

        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand CloseCommand { get; }

        public ProjectsViewModel()
        {
            LoadEmployees();
            LoadProjects();

            AddCommand = new RelayCommand(_ => AddProject(), _ => MainViewModel.Instance.CanAdd);
            EditCommand = new RelayCommand(_ => EditProject(), _ => SelectedProject != null && MainViewModel.Instance.CanEdit);
            DeleteCommand = new RelayCommand(_ => DeleteProject(), _ => SelectedProject != null && MainViewModel.Instance.CanDelete);
            SaveCommand = new RelayCommand(_ => SaveProject());
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

        private void LoadProjects()
        {
            Projects.Clear();

            using (var con = Database.GetConnection())
            {
                con.Open();
                using (var cmd = new SQLiteCommand("SELECT * FROM Projects", con))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        Projects.Add(new ProjectModel
                        {
                            Id = r.GetInt32(r.GetOrdinal("Id")),

                            Title = r.IsDBNull(r.GetOrdinal("Title"))
                                ? ""
                                : r.GetString(r.GetOrdinal("Title")),

                            Description = r.IsDBNull(r.GetOrdinal("Description"))
                                ? ""
                                : r.GetString(r.GetOrdinal("Description")),

                            Owner = r.IsDBNull(r.GetOrdinal("Owner"))
                                ? ""
                                : r.GetString(r.GetOrdinal("Owner")),

                            Deadline = r.IsDBNull(r.GetOrdinal("Deadline"))
                                ? ""
                                : r.GetString(r.GetOrdinal("Deadline")),

                            Progress = r.IsDBNull(r.GetOrdinal("Progress"))
                                ? 0
                                : r.GetInt32(r.GetOrdinal("Progress"))
                        });
                    }
                }
            }
        }

        private void AddProject()
        {
            if (!MainViewModel.Instance.CanAdd)
            {
                MessageBox.Show("У вас нет прав на добавление проектов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isEditMode = false;
            SelectedProject = new ProjectModel();
            OnPropertyChanged(nameof(PanelTitle));
            IsEditorVisible = true;
        }

        private void EditProject()
        {
            if (!MainViewModel.Instance.CanEdit)
            {
                MessageBox.Show("У вас нет прав на редактирование проектов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isEditMode = true;
            OnPropertyChanged(nameof(PanelTitle));
            IsEditorVisible = true;
        }

        private void DeleteProject()
        {
            if (!MainViewModel.Instance.CanDelete)
            {
                MessageBox.Show("У вас нет прав на удаление проектов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedProject == null)
                return;

            var result = MessageBox.Show($"Удалить проект \"{SelectedProject.Title}\"?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Repositories.DeleteProject(SelectedProject.Id);
                Projects.Remove(SelectedProject);
                SelectedProject = null;
            }
        }

        private void SaveProject()
        {
            if (SelectedProject == null)
                return;

            // Валидация
            if (string.IsNullOrWhiteSpace(SelectedProject.Title))
            {
                MessageBox.Show("Введите название проекта!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка прав при создании нового проекта
            if (!_isEditMode && !MainViewModel.Instance.CanAdd)
            {
                MessageBox.Show("У вас нет прав на добавление проектов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка прав при редактировании проекта
            if (_isEditMode && !MainViewModel.Instance.CanEdit)
            {
                MessageBox.Show("У вас нет прав на редактирование проектов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var con = Database.GetConnection())
            {
                con.Open();
                SQLiteCommand cmd;

                if (SelectedProject.Id == 0)
                {
                    cmd = new SQLiteCommand(
                        "INSERT INTO Projects (Title, Description, Owner, Deadline, Progress) VALUES (@t,@d,@o,@dl,0)", con);
                }
                else
                {
                    cmd = new SQLiteCommand(
                        "UPDATE Projects SET Title=@t, Description=@d, Owner=@o, Deadline=@dl WHERE Id=@id", con);
                    cmd.Parameters.AddWithValue("@id", SelectedProject.Id);
                }

                cmd.Parameters.AddWithValue("@t", SelectedProject.Title);
                cmd.Parameters.AddWithValue("@d", SelectedProject.Description);
                cmd.Parameters.AddWithValue("@o", SelectedProject.Owner);
                cmd.Parameters.AddWithValue("@dl", SelectedProject.Deadline);

                cmd.ExecuteNonQuery();

                // Уведомление
                string action = SelectedProject.Id == 0 ? "Добавлен" : "Обновлён";
                Repositories.AddNotification($"{action} проект: {SelectedProject.Title}");
            }

            LoadProjects();
            CloseEditor();
        }

        private void CloseEditor()
        {
            IsEditorVisible = false;
            SelectedProject = null;
        }
    }
}