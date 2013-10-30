using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Text.RegularExpressions;
using WebTimer.Client;

namespace WebTimer.ConfigEditor
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

        private static Regex _regex = new Regex(@"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$", RegexOptions.Compiled | RegexOptions.IgnoreCase); 

        public bool IsNextEnabled
        {
            get
            {
                return (NameBox.Text.Length > 0 && 
                        EmailBox.Text.Length > 0 &&
                        _regex.IsMatch(EmailBox.Text) &&
                        PasswordBox.Password.Length >=6 &&
                        VerifyPasswordBox.Password.Length >= 6 &&
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
