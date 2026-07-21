using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace KHAONPOS.Converters;

public class ImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is byte[] imageData && imageData.Length > 0)
        {
            try
            {
                using var ms = new MemoryStream(imageData);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // Crucial for MemoryStream
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze(); // Crucial for cross-thread operations
                return image;
            }
            catch
            {
                return null;
            }
        }
        
        if (value is string imagePath && !string.IsNullOrWhiteSpace(imagePath))
        {
            try
            {
                // WPF automatically handles valid URLs or local paths when given to BitmapImage
                return new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
