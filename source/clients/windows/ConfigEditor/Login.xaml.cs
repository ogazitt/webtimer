using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

using WebTimer.Client;
using WebTimer.Client.Models;


namespace WebTimer.ConfigEditor
{
    /// <summary>
    /// Interaction logic for Setup.xaml
    /// </summary>
    public partial class Login : Page, INotifyPropertyChanged
    {
        public Login()
        {
            InitializeComponent();

            ComputerNameBox.Text = System.Net.Dns.GetHostName();
            DataContext = this;
        }

        public bool IsNextEnabled
        {
            get
            {
                return (EmailBox.Text.Length > 0 && PasswordBox.Password.Length > 0 && ComputerNameBox.Text.Length > 0);
            }
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            var link = sender as Hyperlink;
            if (link == null)
                return;

            string navigateUri = link.NavigateUri.ToString();
            // if the URI somehow came from an untrusted source, make sure to
            // validate it before calling Process.Start(), e.g. check to see
            // the scheme is HTTP, etc.
            Process.Start(new ProcessStartInfo(navigateUri));
            e.Handled = true;
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            StatusBlock.Text = "Logging in...";
            var user = new User()
            {
                UserName = EmailBox.Text,
                Password = PasswordBox.Password
            };
            WebServiceHelper.VerifyAccount(
                user,
                new WebServiceHelper.AccountDelegate((username) =>
                    {
                        Dispatcher.BeginInvoke((Action) (() =>
                        {
                            StatusBlock.Text = "";
                            if (username == null)
                                StatusBlock.Text = "Login failed.  Please try again.";
                            else
                            {
                                if (username.StartsWith("Error: "))
                                    StatusBlock.Text = username;
                                else
                                {
                                    // save the creds in config file
                                    string credentials = string.Format("{0}:{1}", user.UserName, user.Password);
                                    string encodedCreds = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(credentials));
                                    ConfigClient.Write(ConfigClient.Credentials, encodedCreds);
                                    ConfigClient.Write(ConfigClient.Email, username);
                                    ConfigClient.Write(ConfigClient.DeviceName, ComputerNameBox.Text);

                                    NavigationService.Navigate(new Done());
                                }
                            }
                        }));
                    }),
                null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void TextBoxKeyUp(object sender, KeyEventArgs e)
        {
            NotifyPropertyChanged("IsNextEnabled");
        }               
    }
}
