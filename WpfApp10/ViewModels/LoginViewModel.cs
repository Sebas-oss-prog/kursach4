using WpfApp10.Helpers;
using WpfApp10.Models;

namespace WpfApp10.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainVM;

        private string _username;
        public string Username
        {
            get => _username;
            set => Set(ref _username, value);
        }

        // 🔹 ВАЖНО: Password ДОЛЖЕН БЫТЬ
        private string _password;
        public string Password
        {
            get => _password;
            set => Set(ref _password, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => Set(ref _errorMessage, value);
        }

        public RelayCommand LoginCommand { get; }

        public LoginViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            LoginCommand = new RelayCommand(_ => Login());
        }

        private void Login()
        {
            ErrorMessage = "";

            if (string.IsNullOrWhiteSpace(Username))
            {
                ErrorMessage = "Введите логин.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Введите пароль.";
                return;
            }

            // 🔹 АВТОРИЗАЦИЯ ЧЕРЕЗ БД
            UserModel user = Repositories.GetUser(Username, Password);

            if (user == null)
            {
                ErrorMessage = "Неверный логин или пароль.";
                return;
            }

            // 🔹 СОХРАНЯЕМ ПОЛЬЗОВАТЕЛЯ
            _mainVM.CurrentUser = user;
            _mainVM.UserRole = user.Role;

            // 🔹 СКРЫВАЕМ ОКНО ЛОГИНА
            _mainVM.IsLoginVisible = false;

            // 🔹 ПЕРЕХОД В DASHBOARD
            _mainVM.NavigateDashboard.Execute(null);
        }
    }
}
