using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WpfApp10.Helpers;
using WpfApp10.Models;

namespace WpfApp10.ViewModels
{
    public class TasksViewModel : BaseViewModel
    {
        // ================= DATA =================
        public ObservableCollection<TaskModel> Tasks { get; }
            = new ObservableCollection<TaskModel>();

        public ObservableCollection<EmployeeModel> Employees { get; }
            = new ObservableCollection<EmployeeModel>();

        // ================= SELECTED TASK =================
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

        // ================= UI STATE =================
        private bool _isEditorVisible;
        public bool IsEditorVisible
        {
            get => _isEditorVisible;
            set => Set(ref _isEditorVisible, value);
        }

        private bool _isEditMode;
        public string PanelTitle => _isEditMode ? "Редактирование задачи" : "Новая задача";

        // ================= COMMANDS =================
        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand CloseCommand { get; }

        // ================= CTOR =================
        public TasksViewModel()
        {
            AddCommand = new RelayCommand(_ => Add(), _ => MainViewModel.Instance.CanAdd);
            EditCommand = new RelayCommand(_ => Edit(), _ => SelectedTask != null && MainViewModel.Instance.CanEdit);
            DeleteCommand = new RelayCommand(_ => Delete(), _ => SelectedTask != null && MainViewModel.Instance.CanDelete);
            SaveCommand = new RelayCommand(_ => Save());
            CloseCommand = new RelayCommand(_ => Close());

            LoadData();
        }

        // ================= LOAD =================
        private void LoadData()
        {
            Tasks.Clear();
            Employees.Clear();

            foreach (var t in Repositories.GetTasks())
                Tasks.Add(t);

            foreach (var e in Repositories.GetEmployees())
                Employees.Add(e);
        }

        // ================= ADD =================
        private void Add()
        {
            if (!MainViewModel.Instance.CanAdd)
            {
                MessageBox.Show("У вас нет прав на добавление задач", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isEditMode = false;
            SelectedTask = new TaskModel();
            OnPropertyChanged(nameof(PanelTitle));
            IsEditorVisible = true;
        }

        // ================= EDIT =================
        private void Edit()
        {
            if (!MainViewModel.Instance.CanEdit)
            {
                MessageBox.Show("У вас нет прав на редактирование задач", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isEditMode = true;
            OnPropertyChanged(nameof(PanelTitle));
            IsEditorVisible = true;
        }

        // ================= DELETE =================
        private void Delete()
        {
            if (!MainViewModel.Instance.CanDelete)
            {
                MessageBox.Show("У вас нет прав на удаление задач", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedTask == null)
                return;

            var result = MessageBox.Show($"Удалить задачу \"{SelectedTask.Title}\"?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Repositories.DeleteTask(SelectedTask.Id);
                Tasks.Remove(SelectedTask);
                SelectedTask = null;
            }
        }

        // ================= SAVE =================
        private void Save()
        {
            if (SelectedTask == null)
                return;

            // Валидация
            if (string.IsNullOrWhiteSpace(SelectedTask.Title))
            {
                MessageBox.Show("Введите название задачи!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка прав при создании новой задачи
            if (!_isEditMode && !MainViewModel.Instance.CanAdd)
            {
                MessageBox.Show("У вас нет прав на добавление задач", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка прав при редактировании задачи
            if (_isEditMode && !MainViewModel.Instance.CanEdit)
            {
                MessageBox.Show("У вас нет прав на редактирование задач", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Repositories.SaveTask(SelectedTask);

            if (!Tasks.Contains(SelectedTask))
                Tasks.Add(SelectedTask);

            Close();
        }

        // ================= CLOSE =================
        private void Close()
        {
            SelectedTask = null;
            IsEditorVisible = false;
        }
    }
}