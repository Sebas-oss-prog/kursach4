using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WpfApp10.ViewModels;

namespace WpfApp10.Views
{
    public partial class AnalyticsView : UserControl
    {
        public AnalyticsView()
        {
            InitializeComponent();

            // Не создавать VM в режиме дизайна (Visual Studio Designer)
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            this.DataContext = new AnalyticsViewModel();
        }
    }
}
