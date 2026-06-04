using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BadgeManager.Services;
using PdfSharp.Pdf.IO;
using UglyToad.PdfPig;

namespace BadgeManager
{
    public partial class PdfEditorWindow : Window
    {
        private readonly string _fileId;
        private readonly string _fileName;
        private int? _paginaSelecionada;

        public PdfEditorWindow(string fileId, string fileName)
        {
            InitializeComponent();

            _fileId = fileId;
            _fileName = fileName;

            TxtNomePdf.Text = fileName;

            _ = CarregarPaginasReaisAsync();
        }

        private async Task CarregarPaginasReaisAsync()
        {
            PainelPaginas.Children.Clear();
            TxtStatus.Text = "Baixando PDF...";

            var driveService = GoogleDriveSession.DriveService;

            if (driveService == null)
            {
                MessageBox.Show("Google Drive não conectado.");
                return;
            }

            var tempPath = Path.Combine(Path.GetTempPath(), _fileName);

            using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
            {
                var request = driveService.Files.Get(_fileId);
                await request.DownloadAsync(stream);
            }

            using var document = PdfReader.Open(tempPath, PdfDocumentOpenMode.Import);
            int totalPaginas = document.PageCount;

            TxtStatus.Text = $"Total de páginas: {totalPaginas}";

            using var pdfPigDocument = PdfDocument.Open(tempPath);

            for (int i = 1; i <= totalPaginas; i++)
            {
                var page = pdfPigDocument.GetPage(i);
                string textoPagina = page.Text ?? "";
                string resumo = IdentificarDocumento(textoPagina);

                CriarLinhaPagina(i, resumo);
            }
        }

        private string IdentificarDocumento(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "Documento não identificado";

            string t = texto.ToUpper();

            if (t.Contains("ATESTADO DE SAÚDE OCUPACIONAL") || t.Contains("ASO"))
                return "ASO";

            if (t.Contains("NR35") || t.Contains("NR 35") || t.Contains("TRABALHO EM ALTURA"))
                return "Certificado NR35";

            if (t.Contains("NR10") || t.Contains("NR 10") || t.Contains("SEGURANÇA EM INSTALAÇÕES"))
                return "Certificado NR10";

            if (t.Contains("CARTEIRA DE IDENTIDADE") || t.Contains("REGISTRO GERAL") || t.Contains("RG"))
                return "RG";

            if (t.Contains("CADASTRO DE PESSOA FÍSICA") || t.Contains("CPF"))
                return "CPF";

            if (t.Contains("CNH") || t.Contains("CARTEIRA NACIONAL DE HABILITAÇÃO"))
                return "CNH";

            if (t.Contains("FICHA DE EPI") || t.Contains("EQUIPAMENTO DE PROTEÇÃO INDIVIDUAL"))
                return "Ficha de EPI";

            if (t.Contains("CONTRATO"))
                return "Contrato";

            var resumo = texto.Replace("\r", " ").Replace("\n", " ").Trim();

            if (resumo.Length > 60)
                resumo = resumo.Substring(0, 60) + "...";

            return resumo;
        }

        private void CriarLinhaPagina(int numeroPagina, string resumo)
        {
            var linha = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(12),
                Margin = new Thickness(0, 0, 0, 16),
                Padding = new Thickness(18),
                BorderBrush = new SolidColorBrush(Color.FromRgb(229, 231, 235)),
                BorderThickness = new Thickness(1)
            };

            var stack = new StackPanel();

            var titulo = new TextBlock
            {
                Text = $"Página {numeroPagina}",
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(17, 24, 39))
            };

            var descricao = new TextBlock
            {
                Text = resumo,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(75, 85, 99)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 6, 0, 16)
            };

            var painelBotoes = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var btnExcluir = new Button
            {
                Content = "Excluir",
                Width = 90,
                Height = 34,
                Margin = new Thickness(0, 0, 10, 0)
            };

            var btnTrocar = new Button
            {
                Content = "Trocar",
                Width = 90,
                Height = 34,
                Margin = new Thickness(0, 0, 10, 0)
            };

            var btnAdicionar = new Button
            {
                Content = "Adicionar abaixo",
                Width = 150,
                Height = 34
            };

            btnExcluir.Click += (_, _) =>
            {
                _paginaSelecionada = numeroPagina;
                MessageBox.Show($"Excluir página {numeroPagina}");
            };

            btnTrocar.Click += (_, _) =>
            {
                _paginaSelecionada = numeroPagina;
                MessageBox.Show($"Trocar página {numeroPagina}");
            };

            btnAdicionar.Click += (_, _) =>
            {
                _paginaSelecionada = numeroPagina;
                MessageBox.Show($"Adicionar página abaixo da página {numeroPagina}");
            };

            painelBotoes.Children.Add(btnExcluir);
            painelBotoes.Children.Add(btnTrocar);
            painelBotoes.Children.Add(btnAdicionar);

            stack.Children.Add(titulo);
            stack.Children.Add(descricao);
            stack.Children.Add(painelBotoes);

            linha.Child = stack;

            PainelPaginas.Children.Add(linha);
        }

        private void BtnTrocarPagina_Click(object sender, RoutedEventArgs e)
        {
            if (_paginaSelecionada == null)
            {
                MessageBox.Show("Selecione uma página antes de trocar.");
                return;
            }

            MessageBox.Show($"Trocar página {_paginaSelecionada} do PDF {_fileName}");
        }
    }
}