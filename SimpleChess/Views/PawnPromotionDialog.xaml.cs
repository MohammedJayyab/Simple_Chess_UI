using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using SimpleChess.Models;

namespace SimpleChess.Views
{
    public partial class PawnPromotionDialog : Window
    {
        public PieceType SelectedPieceType { get; private set; }

        public PawnPromotionDialog(PlayerColor pawnColor)
        {
            InitializeComponent();

            // Get the color suffix for the image files
            string colorSuffix = pawnColor == PlayerColor.White ? "w" : "b";

            // Load images directly from files
            try 
            {
                string imagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "images");
                QueenImage.Source = new BitmapImage(new Uri(Path.Combine(imagesPath, $"queen-{colorSuffix}.png")));
                RookImage.Source = new BitmapImage(new Uri(Path.Combine(imagesPath, $"rook-{colorSuffix}.png")));
                BishopImage.Source = new BitmapImage(new Uri(Path.Combine(imagesPath, $"bishop-{colorSuffix}.png")));
                KnightImage.Source = new BitmapImage(new Uri(Path.Combine(imagesPath, $"knight-{colorSuffix}.png")));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading images: {ex.Message}");
            }
        }



        private void OnPieceSelected(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string pieceType)
            {
                SelectedPieceType = pieceType switch
                {
                    "Queen" => PieceType.Queen,
                    "Rook" => PieceType.Rook,
                    "Bishop" => PieceType.Bishop,
                    "Knight" => PieceType.Knight,
                    _ => PieceType.Queen // Default to Queen
                };

                DialogResult = true;
                Close();
            }
        }
    }
}