using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Collector;

namespace CaptureConfig
{
    /// <summary>
    /// Interaction logic for Setup.xaml
    /// </summary>
    public partial class Register : Page, INotifyPropertyChanged
    {
        public Register()
        {
            InitializeComponent();

            ComputerNameBox.Text = System.Net.Dns.GetHostName();
            DataContext = this;
        }

        public bool IsNextEnabled
        {
            get
            {
                return (NameBox.Text.Length > 0 && 
                        EmailBox.Text.Length > 0 &&
                        PasswordBox.Password.Length > 0 &&
                        VerifyPasswordBox.Password.Length > 0 &&
                        PasswordBox.Password == VerifyPasswordBox.Password &&
                        ComputerNameBox.Text.Length > 0 &&
                        TermsBox.IsChecked.HasValue && TermsBox.IsChecked.Value);
            }
        }

        private void TOS_Click(object sender, RoutedEventArgs e)
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
            StatusBlock.Text = "Creating account...";
            var user = new RegisterUser()
            {
                UserName = EmailBox.Text,
                Password = PasswordBox.Password,
                ConfirmPassword = VerifyPasswordBox.Password,
                Name = NameBox.Text
            };

            WebServiceHelper.CreateAcccount(
                user,
                new WebServiceHelper.AccountDelegate((username) =>
                    {
                        Dispatcher.BeginInvoke((Action) (() =>
                        {
                            StatusBlock.Text = "";
                            if (username == null)
                                StatusBlock.Text = "Account creation failed.  Please try again later.";
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
                                    ConfigClient.Write(ConfigClient.Name, user.Name);
                                    ConfigClient.Write(ConfigClient.ComputerName, ComputerNameBox.Text);

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

        private void TextBoxKeyUp(object sender, EventArgs e)
        {
            NotifyPropertyChanged("IsNextEnabled");
        }               
    }
}
