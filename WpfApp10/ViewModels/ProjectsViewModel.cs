using System.Collections.ObjectModel;
using System.Data.SQLite;
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

            AddCommand = new RelayCommand(_ => AddProject());
            EditCommand = new RelayCommand(_ => EditProject(), _ => SelectedProject != null);
            DeleteCommand = new RelayCommand(_ => DeleteProject(), _ => SelectedProject != null);
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
                var cmd = new SQLiteCommand("SELECT * FROM Projects", con);
                var r = cmd.ExecuteReader();

                while (r.Read())
                {
                    Projects.Add(new ProjectModel
                    {
                        Id = r.GetInt32(0),
                        Title = r.GetString(1),
                        Description = r.GetString(2),
                        Owner = r.GetString(3),
                        Deadline = r.GetString(4),
                        Progress = r.GetInt32(5)
                    });
                }
            }
        }

        private void AddProject()
        {
            _isEditMode = false;
            SelectedProject = new ProjectModel();
            OnPropertyChanged(nameof(PanelTitle));
            IsEditorVisible = true;
        }

        private void EditProject()
        {
            _isEditMode = true;
            OnPropertyChanged(nameof(PanelTitle));
            IsEditorVisible = true;
        }

        private void DeleteProject()
        {
            using (var con = Database.GetConnection())
            {
                con.Open();
                var cmd = new SQLiteCommand("DELETE FROM Projects WHERE Id=@id", con);
                cmd.Parameters.AddWithValue("@id", SelectedProject.Id);
                cmd.ExecuteNonQuery();
            }

            Projects.Remove(SelectedProject);
            SelectedProject = null;
        }

        private void SaveProject()
        {
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
