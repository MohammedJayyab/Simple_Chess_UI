using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using SimpleChess.Models;

namespace SimpleChess.Converters
{
    public class GameStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GameState state)
            {
                switch (state)
                {
                    case GameState.Check:
                        return new SolidColorBrush(Color.FromRgb(255, 200, 100)); // Orange
                    case GameState.Checkmate:
                        return new SolidColorBrush(Color.FromRgb(255, 120, 120)); // Red
                    case GameState.Stalemate:
                    case GameState.Draw:
                        return new SolidColorBrush(Color.FromRgb(120, 120, 255)); // Blue
                    default:
                        return new SolidColorBrush(Color.FromRgb(240, 240, 240)); // Light Gray
                }
            }
            return new SolidColorBrush(Color.FromRgb(240, 240, 240));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class SquareColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isLight)
            {
                return isLight ? 
                    Application.Current.Resources["LightSquareColor"] : 
                    Application.Current.Resources["DarkSquareColor"];
            }
            return Application.Current.Resources["LightSquareColor"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isVisible)
            {
                return isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PieceToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
