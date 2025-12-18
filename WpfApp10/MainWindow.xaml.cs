using System.Windows;
using WpfApp10.ViewModels;

namespace WpfApp10
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();   // ← ДОЛЖНО БЫТЬ!!!
        }

    }
}
