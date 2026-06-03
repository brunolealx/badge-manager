using System.Windows;
using System.Windows.Controls;
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

        private void BtnFuncionarios_Click(object sender, RoutedEventArgs e)
        {
            TxtTituloPagina.Text = "Funcionários";
            TxtDescricaoPagina.Text = "Lista de funcionários cadastrados.";

            AreaConteudo.Children.Clear();

            var lista = new ListView();

            lista.Items.Add("Bruno Leal");
            lista.Items.Add("Funcionário Exemplo 1");
            lista.Items.Add("Funcionário Exemplo 2");
            lista.Items.Add("Funcionário Exemplo 3");

            AreaConteudo.Children.Add(lista);
        }
    }
}