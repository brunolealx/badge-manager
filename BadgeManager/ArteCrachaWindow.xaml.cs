using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BadgeManager
{
    public partial class ArteCrachaWindow : Window
    {
        private readonly string _artePath;

        public ArteCrachaWindow()
        {
            InitializeComponent();

            _artePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "arte_cracha.png");

            ImgArteCracha.Source = new BitmapImage(new Uri(_artePath));
        }

        private void ImgArteCracha_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                FileName = "arte_cracha.png",
                Filter = "PNG (*.png)|*.png"
            };

            if (dialog.ShowDialog() == true)
            {
                File.Copy(_artePath, dialog.FileName, true);
                MessageBox.Show("Arte salva com sucesso!");
            }
        }
    }
}