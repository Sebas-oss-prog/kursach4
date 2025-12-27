using System.Windows.Controls;
using WpfApp10.ViewModels;

namespace WpfApp10.Views
{
    public partial class UsersView : UserControl
    {
        public UsersView()
        {
            InitializeComponent();
            DataContext = new UsersViewModel();
            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }

        private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is UsersViewModel vm && vm.SelectedUser != null)
            {
                vm.SelectedUser.Password = PasswordBox.Password;
            }
        }
    }
}