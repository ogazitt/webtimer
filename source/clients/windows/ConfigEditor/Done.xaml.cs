using System.Windows;
using System.Windows.Controls;

namespace WebTimer.ConfigEditor
{
    /// <summary>
    /// Interaction logic for Setup.xaml
    /// </summary>
    public partial class Done : Page
    {
        public Done()
        {
            InitializeComponent();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        } 
    }
}
