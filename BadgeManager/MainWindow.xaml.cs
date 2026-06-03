using System.Windows;

namespace BadgeManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnGoogleLogin_Click(object sender, RoutedEventArgs e)
        {
            TxtStatus.Text = "Status: Login Google em desenvolvimento...";
        }
    }
}