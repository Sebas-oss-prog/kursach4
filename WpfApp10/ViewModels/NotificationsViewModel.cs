using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public NotificationsViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
                return;

            Load();
        }

        public void Load()
        {
            Notifications = new ObservableCollection<NotificationModel>(
                WpfApp10.Repositories.GetNotifications()
            );
        }
    }
}
