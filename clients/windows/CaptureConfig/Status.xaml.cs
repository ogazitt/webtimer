using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Collector;

namespace CaptureConfig
{
    /// <summary>
    /// Interaction logic for Setup.xaml
    /// </summary>
    public partial class Status : Page, INotifyPropertyChanged
    {
        public Status()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string GroupBoxTitle
        {
            get
            {
                var computerName = ConfigClient.Read(ConfigClient.ComputerName) ?? System.Net.Dns.GetHostName();
                return string.Format("Account information for this computer ({0})", computerName);
            }
        }

        public string AccountText
        {
            get
            {
                var text = new StringBuilder("This computer is linked to ");
                var name = ConfigClient.Read(ConfigClient.Name);
                var email = ConfigClient.Read(ConfigClient.Email);
                if (!string.IsNullOrEmpty(name))
                {
                    text.Append(name);
                    text.Append("'s (");
                    text.Append(email);
                    text.Append(") ");
                }
                else
                {
                    text.Append(email);
                    text.Append("'s ");
                }
                text.Append("WebTimer account.");
                return text.ToString();
            }
        }

        public string VersionText
        {
            get
            {                
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                return string.Format("WebTimer v{0} ({1})", fvi.ProductVersion, fvi.FileVersion);
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void UnlinkButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigClient.Clear();
            NavigationService.Navigate(new Setup());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
