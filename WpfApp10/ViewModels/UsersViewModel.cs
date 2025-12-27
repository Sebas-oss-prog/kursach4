using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WpfApp10.Helpers;
using WpfApp10.Models;

namespace WpfApp10.ViewModels
{
    public class UsersViewModel : BaseViewModel
    {
        public ObservableCollection<UserModel> Users { get; }
            = new ObservableCollection<UserModel>();

        public ObservableCollection<string> Roles { get; }
            = new ObservableCollection<string> { "Admin", "Manager", "User" };

        private UserModel _selectedUser;
        public UserModel SelectedUser
        {
            get => _selectedUser;
            set
            {
                Set(ref _selectedUser, value);
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
        public string PanelTitle => _isEditMode ? "Редактирование пользователя" : "Новый пользователь";

        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand CloseCommand { get; }

        public UsersViewModel()
        {
            LoadUsers();

            AddCommand = new RelayCommand(_ => AddUser(), _ => MainViewModel.Instance.CanManageUsers);
            EditCommand = new RelayCommand(_ => EditUser(), _ => SelectedUser != null && MainViewModel.Instance.CanManageUsers);
            DeleteCommand = new RelayCommand(_ => DeleteUser(), _ => SelectedUser != null && MainViewModel.Instance.CanManageUsers);
            SaveCommand = new RelayCommand(_ => SaveUser());
            CloseCommand = new RelayCommand(_ => CloseEditor());
        }

        private void LoadUsers()
        {
            Users.Clear();
            foreach (var user in Repositories.GetUsers())
                Users.Add(user);
        }

        private void AddUser()
        {
            if (!MainViewModel.Instance.CanManageUsers)
            {
                MessageBox.Show("У вас нет прав на управление пользователями", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isEditMode = false;
            SelectedUser = new UserModel();
            OnPropertyChanged(nameof(PanelTitle));
            IsEditorVisible = true;
        }

        private void EditUser()
        {
            if (!MainViewModel.Instance.CanManageUsers)
            {
                MessageBox.Show("У вас нет прав на управление пользователями", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isEditMode = true;
            OnPropertyChanged(nameof(PanelTitle));
            IsEditorVisible = true;
        }

        private void DeleteUser()
        {
            if (!MainViewModel.Instance.CanManageUsers)
            {
                MessageBox.Show("У вас нет прав на управление пользователями", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedUser == null) return;

            // Нельзя удалить самого себя
            if (SelectedUser.Id == MainViewModel.Instance.CurrentUser?.Id)
            {
                MessageBox.Show("Нельзя удалить текущего пользователя!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить пользователя {SelectedUser.FullName}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Repositories.DeleteUser(SelectedUser.Id);
                Users.Remove(SelectedUser);
                SelectedUser = null;
            }
        }

        private void SaveUser()
        {
            if (SelectedUser == null) return;

            if (!MainViewModel.Instance.CanManageUsers)
            {
                MessageBox.Show("У вас нет прав на управление пользователями", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Валидация
            if (string.IsNullOrWhiteSpace(SelectedUser.Login))
            {
                MessageBox.Show("Введите логин!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedUser.Password))
            {
                MessageBox.Show("Введите пароль!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedUser.FullName))
            {
                MessageBox.Show("Введите ФИО!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_isEditMode)
            {
                Repositories.UpdateUser(
                    SelectedUser.Id,
                    SelectedUser.Login,
                    SelectedUser.Password,
                    SelectedUser.Role,
                    SelectedUser.FullName
                );
            }
            else
            {
                // Проверка на уникальность логина
                var existingUser = Users.FirstOrDefault(u => u.Login == SelectedUser.Login);
                if (existingUser != null)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Repositories.AddUser(
                    SelectedUser.Login,
                    SelectedUser.Password,
                    SelectedUser.Role,
                    SelectedUser.FullName
                );
            }

            LoadUsers();
            CloseEditor();
        }

        private void CloseEditor()
        {
            IsEditorVisible = false;
            SelectedUser = null;
        }
    }
}