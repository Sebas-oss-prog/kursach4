using System.Windows.Controls;
using WpfApp10.Helpers;
using WpfApp10.Models;
using WpfApp10.Views;

namespace WpfApp10.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // ================== SINGLETON ==================
        public static MainViewModel Instance { get; private set; }

        // ================== CURRENT VIEW ==================
        private UserControl _currentView;
        public UserControl CurrentView
        {
            get => _currentView;
            set => Set(ref _currentView, value);
        }

        // ================== LOGIN ==================
        private bool _isLoginVisible = true;
        public bool IsLoginVisible
        {
            get => _isLoginVisible;
            set => Set(ref _isLoginVisible, value);
        }

        // ================== USER ==================
        private UserModel _currentUser;
        public UserModel CurrentUser
        {
            get => _currentUser;
            set
            {
                if (Set(ref _currentUser, value))
                {
                    UserRole = value?.Role ?? "User";
                }
            }
        }

        private string _userRole = "User";
        public string UserRole
        {
            get => _userRole;
            set
            {
                if (Set(ref _userRole, value))
                {
                    OnPropertyChanged(nameof(CanAdd));
                    OnPropertyChanged(nameof(CanEdit));
                    OnPropertyChanged(nameof(CanDelete));
                }
            }
        }

        // ================== PERMISSIONS ==================
        public bool CanAdd =>
            UserRole == "Admin" ||
            UserRole == "Manager" ||
            UserRole == "User";

        public bool CanEdit =>
            UserRole == "Admin" ||
            UserRole == "Manager";

        public bool CanDelete =>
            UserRole == "Admin";

        // ================== VIEWMODELS ==================
        public LoginViewModel LoginVM { get; }

        // ================== COMMANDS ==================
        public RelayCommand NavigateDashboard { get; }
        public RelayCommand NavigateDocuments { get; }
        public RelayCommand NavigateTasks { get; }
        public RelayCommand NavigateProjects { get; }
        public RelayCommand NavigateNotifications { get; }
        public RelayCommand NavigateAnalytics { get; }
        public RelayCommand NavigateSearch { get; }

        // 🔽 🔽 🔽 ДОБАВЛЕНО ДЛЯ КАЛЕНДАРЯ
        public RelayCommand NavigateCalendar { get; }

        public RelayCommand LogoutCommand { get; }

        // ================== CONSTRUCTOR ==================
        public MainViewModel()
        {
            Instance = this;

            LoginVM = new LoginViewModel(this);

            NavigateDashboard = new RelayCommand(_ => OpenDashboard());
            NavigateDocuments = new RelayCommand(_ => OpenDocuments());
            NavigateTasks = new RelayCommand(_ => OpenTasks());
            NavigateProjects = new RelayCommand(_ => OpenProjects());
            NavigateNotifications = new RelayCommand(_ => OpenNotifications());
            NavigateAnalytics = new RelayCommand(_ => OpenAnalytics());
            NavigateSearch = new RelayCommand(_ => OpenSearch());

            // 🔽 🔽 🔽 ИНИЦИАЛИЗАЦИЯ КОМАНДЫ КАЛЕНДАРЯ
            NavigateCalendar = new RelayCommand(_ => OpenCalendar());

            LogoutCommand = new RelayCommand(_ => Logout());

            OpenDashboard();
        }

        // ================== METHODS ==================
        public void CloseLogin()
        {
            IsLoginVisible = false;
            OpenDashboard();
        }

        private void Logout()
        {
            CurrentUser = null;
            UserRole = "User";
            IsLoginVisible = true;
        }

        private void OpenDashboard()
        {
            CurrentView = new DashboardView
            {
                DataContext = new DashboardViewModel()
            };
        }

        private void OpenDocuments()
        {
            CurrentView = new DocumentsView
            {
                DataContext = new DocumentsViewModel()
            };
        }

        private void OpenTasks()
        {
            CurrentView = new TasksView
            {
                DataContext = new TasksViewModel()
            };
        }

        private void OpenProjects()
        {
            CurrentView = new ProjectsView
            {
                DataContext = new ProjectsViewModel()
            };
        }

        private void OpenNotifications()
        {
            CurrentView = new NotificationsView
            {
                DataContext = new NotificationsViewModel()
            };
        }

        private void OpenAnalytics()
        {
            CurrentView = new AnalyticsView
            {
                DataContext = new AnalyticsViewModel()
            };
        }

        private void OpenSearch()
        {
            CurrentView = new SearchView
            {
                DataContext = new SearchViewModel()
            };
        }

        // 🔽 🔽 🔽 МЕТОД ДЛЯ КАЛЕНДАРЯ (ДОБАВЛЕН)
        private void OpenCalendar()
        {
            CurrentView = new CalendarView
            {
                DataContext = new CalendarViewModel()
            };
        }

        // ================== SEARCH NAVIGATION ==================
        public void NavigateFromSearch(SearchResultItem item)
        {
            if (item == null)
                return;

            switch (item.Type)
            {
                case "Задача":
                    OpenTasks();
                    break;

                case "Проект":
                    OpenProjects();
                    break;

                case "Документ":
                    OpenDocuments();
                    break;
            }
        }
    }
}
