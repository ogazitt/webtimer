using System.Windows;
using System.Windows.Controls;

namespace WebTimer.ConfigEditor
{
    /// <summary>
    /// Interaction logic for Setup.xaml
    /// </summary>
    public partial class Setup : Page
    {
        public Setup()
        {
            InitializeComponent();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (NoAccountRadio.IsChecked.HasValue && NoAccountRadio.IsChecked.Value)
                NavigationService.Navigate(new Register());
            else
                NavigationService.Navigate(new Login());
        } 
    }
}
