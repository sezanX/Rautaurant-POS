using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace KHAONPOS.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var status = value as string;

        if (status == "Pending") return new SolidColorBrush(Colors.Orange);
        if (status == "Preparing") return new SolidColorBrush(Colors.Blue);
        if (status == "Served") return new SolidColorBrush(Colors.Green);
        if (status == "Paid") return new SolidColorBrush(Colors.Gray);
        if (status == "Available") return new SolidColorBrush(Colors.LightGreen);
        if (status == "Occupied") return new SolidColorBrush(Colors.Red);
        if (status == "Reserved") return new SolidColorBrush(Colors.Purple);

        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
