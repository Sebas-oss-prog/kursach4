using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using WpfApp10.Models;

namespace WpfApp10.ViewModels
{
    public class NotificationsViewModel : BaseViewModel
    {
        private ObservableCollection<NotificationModel> _notifications;
        public ObservableCollection<NotificationModel> Notifications
        {
            get => _notifications;
            set => Set(ref _notifications, value);
        }

        private DispatcherTimer _refreshTimer;

        public NotificationsViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
                return;

            Load();

            // Таймер для автообновления уведомлений каждые 30 секунд
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = System.TimeSpan.FromSeconds(30);
            _refreshTimer.Tick += (s, e) => Load();
            _refreshTimer.Start();
        }

        public void Load()
        {
            var notifications = Repositories.GetNotifications();
            Notifications = new ObservableCollection<NotificationModel>(notifications);
        }
    }
}