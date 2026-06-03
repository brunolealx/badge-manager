using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BadgeManager.Services;
using Google.Apis.Drive.v3;

namespace BadgeManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void BtnFuncionarios_Click(object sender, RoutedEventArgs e)
        {
            TxtTituloPagina.Text = "Funcionários";
            TxtDescricaoPagina.Text = "PDFs encontrados no Google Drive.";

            AreaConteudo.Children.Clear();

            var botoes = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 20)
            };

            botoes.Children.Add(new Button
            {
                Content = "+ Incluir",
                Width = 120,
                Height = 38,
                Margin = new Thickness(0, 0, 10, 0)
            });

            botoes.Children.Add(new Button
            {
                Content = "- Excluir",
                Width = 120,
                Height = 38
            });

            AreaConteudo.Children.Add(botoes);

            var painelCards = new WrapPanel();

            AreaConteudo.Children.Add(painelCards);

            var driveService = GoogleDriveSession.DriveService;

            if (driveService == null)
            {
                MessageBox.Show("Google Drive não conectado.");
                return;
            }

            var request = driveService.Files.List();
            request.Q = "mimeType='application/pdf' and trashed=false";
            request.Fields = "files(id, name, webViewLink)";
            request.PageSize = 50;

            var result = await request.ExecuteAsync();

            foreach (var arquivo in result.Files)
            {
                string nomeFuncionario = arquivo.Name.Replace(".pdf", "");

                var card = new Border
                {
                    Width = 180,
                    Height = 240,
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(12),
                    Margin = new Thickness(0, 0, 20, 20),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(229, 231, 235)),
                    BorderThickness = new Thickness(1)
                };

                var stack = new StackPanel();

                var fotoFake = new Border
                {
                    Height = 180,
                    Background = new SolidColorBrush(Color.FromRgb(229, 231, 235)),
                    CornerRadius = new CornerRadius(12, 12, 0, 0),
                    Child = new TextBlock
                    {
                        Text = "PDF",
                        FontSize = 28,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };

                var nome = new TextBlock
                {
                    Text = nomeFuncionario,
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(10, 16, 10, 0)
                };

                stack.Children.Add(fotoFake);
                stack.Children.Add(nome);

                card.Child = stack;
                painelCards.Children.Add(card);
            }
        }
    }
}