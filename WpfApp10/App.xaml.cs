using System.Windows;

namespace WpfApp10
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Database.Initialize();
            base.OnStartup(e);
        }
    }
}
