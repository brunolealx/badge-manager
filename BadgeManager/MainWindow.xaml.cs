using System.Windows;
using BadgeManager.Services;

namespace BadgeManager
{
    public partial class MainWindow : Window
    {
        private readonly GoogleDriveAuthService _googleDriveAuthService;

        public MainWindow()
        {
            InitializeComponent();
            _googleDriveAuthService = new GoogleDriveAuthService();
        }

        private async void BtnGoogleLogin_Click(object sender, RoutedEventArgs e)
        {
            TxtStatus.Text = "Status: Conectando ao Google...";
            BtnGoogleLogin.IsEnabled = false;

            var driveService = await _googleDriveAuthService.LoginAsync();

            if (driveService != null)
            {
                TxtStatus.Text = "Status: Conectado ao Google Drive";
                MessageBox.Show("Login realizado com sucesso!");
            }
            else
            {
                TxtStatus.Text = "Status: Erro ao conectar";
                MessageBox.Show("Não foi possível conectar ao Google Drive.");
                BtnGoogleLogin.IsEnabled = true;
            }
        }
    }
}