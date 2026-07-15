using System.Windows.Media.Imaging;

namespace RestaurantPOS.Services;

public interface IBarcodeService
{
    BitmapSource GenerateBarcode(string text, int width, int height);
    BitmapSource GenerateQrCode(string text, int width, int height);
}
