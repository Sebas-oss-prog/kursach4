using System.Windows;
using System.Windows.Controls;
using WpfApp10.ViewModels;

namespace WpfApp10.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Username = LoginBox.Text;
                vm.Password = PasswordBox.Password;

                vm.LoginCommand.Execute(null);
            }
        }
    }
}
