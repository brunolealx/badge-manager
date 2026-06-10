using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BadgeManager.Services;
using PdfSharp.Pdf.IO;
using UglyToad.PdfPig;
using Microsoft.Win32;
using Google.Apis.Upload;
using PdfSharp.Pdf;

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

            using var pdfPigDocument = UglyToad.PdfPig.PdfDocument.Open(tempPath);

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
                ExcluirPagina(numeroPagina);
            };

            btnTrocar.Click += (_, _) =>
            {
                _paginaSelecionada = numeroPagina;
                TrocarPagina(numeroPagina);
            };

            btnAdicionar.Click += (_, _) =>
            {
                _paginaSelecionada = numeroPagina;
                AdicionarPaginaAbaixo(numeroPagina);
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

        private async void TrocarPagina(int numeroPagina)
        {
            var dialog = new OpenFileDialog
            {
                Title = $"Selecionar PDF para substituir a página {numeroPagina}",
                Filter = "Arquivos PDF (*.pdf)|*.pdf"
            };

            if (dialog.ShowDialog() != true)
                return;

            var confirmar = MessageBox.Show(
      $"ATENÇÃO!\n\nA página {numeroPagina} será substituída permanentemente no Google Drive.\n\nDeseja continuar?",
                  "Confirmar substituição",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmar != MessageBoxResult.Yes)
                return;

            try
            {
                TxtStatus.Text = "Atualizando PDF no Google Drive...";

                await SubstituirPaginaEAtualizarDriveAsync(numeroPagina, dialog.FileName);

                MessageBox.Show("Página substituída e PDF atualizado no Google Drive com sucesso!");

                await CarregarPaginasReaisAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao substituir página:\n{ex.Message}");
            }
        }
        private async Task SubstituirPaginaEAtualizarDriveAsync(int numeroPagina, string novoPdfPath)
        {
            var driveService = GoogleDriveSession.DriveService;

            if (driveService == null)
                throw new Exception("Google Drive não conectado.");

            var pdfOriginalPath = Path.Combine(Path.GetTempPath(), _fileName);
            var pdfAtualizadoPath = Path.Combine(
                Path.GetTempPath(),
                $"updated_{Guid.NewGuid()}_{_fileName}");

            using (var stream = new FileStream(pdfOriginalPath, FileMode.Create, FileAccess.Write))
            {
                var downloadRequest = driveService.Files.Get(_fileId);
                await downloadRequest.DownloadAsync(stream);
            }

            using var pdfOriginal = PdfReader.Open(pdfOriginalPath, PdfDocumentOpenMode.Import);
            using var pdfNovo = PdfReader.Open(novoPdfPath, PdfDocumentOpenMode.Import);

            if (numeroPagina < 1 || numeroPagina > pdfOriginal.PageCount)
                throw new Exception("Número de página inválido.");

            if (pdfNovo.PageCount < 1)
                throw new Exception("O PDF selecionado não possui páginas.");

            var pdfFinal = new PdfSharp.Pdf.PdfDocument();

            for (int i = 0; i < pdfOriginal.PageCount; i++)
            {
                if (i == numeroPagina - 1)
                {
                    pdfFinal.AddPage(pdfNovo.Pages[0]);
                }
                else
                {
                    pdfFinal.AddPage(pdfOriginal.Pages[i]);
                }
            }

            pdfFinal.Save(pdfAtualizadoPath);

            using var uploadStream = new FileStream(pdfAtualizadoPath, FileMode.Open, FileAccess.Read);

            var updateRequest = driveService.Files.Update(
                new Google.Apis.Drive.v3.Data.File(),
                _fileId,
                uploadStream,
                "application/pdf");

            var uploadResult = await updateRequest.UploadAsync();

            if (uploadResult.Status != UploadStatus.Completed)
                throw new Exception(uploadResult.Exception?.Message ?? "Falha ao atualizar o PDF no Google Drive.");
        }

        private void BtnTrocarPagina_Click(object sender, RoutedEventArgs e)
        {
            if (_paginaSelecionada == null)
            {
                MessageBox.Show("Selecione uma página antes de trocar.");
                return;
            }

            TrocarPagina(_paginaSelecionada.Value);

        }
        private async void ExcluirPagina(int numeroPagina)
        {
            var confirmar = MessageBox.Show(
                $"ATENÇÃO!\n\nA página {numeroPagina} será excluída permanentemente do PDF no Google Drive.\n\nDeseja continuar?",
                "Confirmar exclusão",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmar != MessageBoxResult.Yes)
                return;

            try
            {
                TxtStatus.Text = "Excluindo página e atualizando Google Drive...";

                await ExcluirPaginaEAtualizarDriveAsync(numeroPagina);

                MessageBox.Show("Página excluída e PDF atualizado no Google Drive com sucesso!");

                await CarregarPaginasReaisAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao excluir página:\n{ex.Message}");
            }
        }
        private async Task ExcluirPaginaEAtualizarDriveAsync(int numeroPagina)
        {
            var driveService = GoogleDriveSession.DriveService;

            if (driveService == null)
                throw new Exception("Google Drive não conectado.");

            var pdfOriginalPath = Path.Combine(Path.GetTempPath(), _fileName);
            var pdfAtualizadoPath = Path.Combine(
                Path.GetTempPath(),
                $"updated_{Guid.NewGuid()}_{_fileName}");

            using (var stream = new FileStream(pdfOriginalPath, FileMode.Create, FileAccess.Write))
            {
                var downloadRequest = driveService.Files.Get(_fileId);
                await downloadRequest.DownloadAsync(stream);
            }

            using var pdfOriginal = PdfReader.Open(pdfOriginalPath, PdfDocumentOpenMode.Import);

            if (pdfOriginal.PageCount <= 1)
                throw new Exception("Não é possível excluir a única página do PDF.");

            if (numeroPagina < 1 || numeroPagina > pdfOriginal.PageCount)
                throw new Exception("Número de página inválido.");

            var pdfFinal = new PdfSharp.Pdf.PdfDocument();

            for (int i = 0; i < pdfOriginal.PageCount; i++)
            {
                if (i != numeroPagina - 1)
                    pdfFinal.AddPage(pdfOriginal.Pages[i]);
            }

            pdfFinal.Save(pdfAtualizadoPath);

            using var uploadStream = new FileStream(pdfAtualizadoPath, FileMode.Open, FileAccess.Read);

            var updateRequest = driveService.Files.Update(
                new Google.Apis.Drive.v3.Data.File(),
                _fileId,
                uploadStream,
                "application/pdf");

            var uploadResult = await updateRequest.UploadAsync();

            if (uploadResult.Status != Google.Apis.Upload.UploadStatus.Completed)
                throw new Exception(uploadResult.Exception?.Message ?? "Falha ao atualizar o PDF no Google Drive.");
        }

        private async void AdicionarPaginaAbaixo(int numeroPagina)
        {
            var dialog = new OpenFileDialog
            {
                Title = $"Selecionar PDF para adicionar abaixo da página {numeroPagina}",
                Filter = "Arquivos PDF (*.pdf)|*.pdf"
            };

            if (dialog.ShowDialog() != true)
                return;

            var confirmar = MessageBox.Show(
                $"Deseja adicionar uma página abaixo da página {numeroPagina} e atualizar o PDF no Google Drive?",
                "Confirmar adição",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmar != MessageBoxResult.Yes)
                return;

            try
            {
                TxtStatus.Text = "Adicionando página e atualizando Google Drive...";

                await AdicionarPaginaAbaixoEAtualizarDriveAsync(numeroPagina, dialog.FileName);

                MessageBox.Show("Página adicionada e PDF atualizado no Google Drive com sucesso!");

                await CarregarPaginasReaisAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao adicionar página:\n{ex.Message}");
            }
        }

        private async Task AdicionarPaginaAbaixoEAtualizarDriveAsync(int numeroPagina, string novoPdfPath)
        {
            var driveService = GoogleDriveSession.DriveService;

            if (driveService == null)
                throw new Exception("Google Drive não conectado.");

            var pdfOriginalPath = Path.Combine(Path.GetTempPath(), _fileName);
            var pdfAtualizadoPath = Path.Combine(
                Path.GetTempPath(),
                $"updated_{Guid.NewGuid()}_{_fileName}");

            using (var stream = new FileStream(pdfOriginalPath, FileMode.Create, FileAccess.Write))
            {
                var downloadRequest = driveService.Files.Get(_fileId);
                await downloadRequest.DownloadAsync(stream);
            }

            using var pdfOriginal = PdfReader.Open(pdfOriginalPath, PdfDocumentOpenMode.Import);
            using var pdfNovo = PdfReader.Open(novoPdfPath, PdfDocumentOpenMode.Import);

            if (numeroPagina < 1 || numeroPagina > pdfOriginal.PageCount)
                throw new Exception("Número de página inválido.");

            if (pdfNovo.PageCount < 1)
                throw new Exception("O PDF selecionado não possui páginas.");

            var pdfFinal = new PdfSharp.Pdf.PdfDocument();

            for (int i = 0; i < pdfOriginal.PageCount; i++)
            {
                pdfFinal.AddPage(pdfOriginal.Pages[i]);

                if (i == numeroPagina - 1)
                    pdfFinal.AddPage(pdfNovo.Pages[0]);
            }

            pdfFinal.Save(pdfAtualizadoPath);

            using var uploadStream = new FileStream(pdfAtualizadoPath, FileMode.Open, FileAccess.Read);

            var updateRequest = driveService.Files.Update(
                new Google.Apis.Drive.v3.Data.File(),
                _fileId,
                uploadStream,
                "application/pdf");

            var uploadResult = await updateRequest.UploadAsync();

            if (uploadResult.Status != Google.Apis.Upload.UploadStatus.Completed)
                throw new Exception(uploadResult.Exception?.Message ?? "Falha ao atualizar o PDF no Google Drive.");
        }

    }
}