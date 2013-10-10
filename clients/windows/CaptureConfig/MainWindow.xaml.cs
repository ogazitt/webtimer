using System.Windows;
using Collector;

namespace CaptureConfig
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            /*
            Dispatcher.BeginInvoke(ApplicationPriority.Render, new Action(() =>
            {
                var navWindow = Window.GetWindow(this) as NavigationWindow;
                if (navWindow != null) navWindow.ShowsNavigationUI = false;
            }));
             */

            var credentials = ConfigClient.Read(ConfigClient.Credentials);
            if (credentials != null)
                MainFrame.Navigate(new Status());    
            else
                MainFrame.Navigate(new Setup());
        }
    }
}
