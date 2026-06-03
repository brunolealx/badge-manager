using System.Windows;
using BadgeManager.Services;

namespace BadgeManager
{
    public partial class AuthWindow : Window
    {
        private readonly GoogleDriveAuthService _googleDriveAuthService;

        public AuthWindow()
        {
            InitializeComponent();
            _googleDriveAuthService = new GoogleDriveAuthService();
        }

        private async void BtnEntrarGoogle_Click(object sender, RoutedEventArgs e)
        {
            TxtStatusAuth.Text = "Status: Conectando...";
            BtnEntrarGoogle.IsEnabled = false;

            var driveService = await _googleDriveAuthService.LoginAsync();

            if (driveService != null)
            {
                GoogleDriveSession.DriveService = driveService;

                var mainWindow = new MainWindow();
                mainWindow.Show();

                Close();
            }
            else
            {
                TxtStatusAuth.Text = "Status: Erro ao conectar";
                BtnEntrarGoogle.IsEnabled = true;
            }
        }
    }
}