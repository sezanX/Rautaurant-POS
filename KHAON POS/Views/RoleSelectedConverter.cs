using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RestaurantPOS.Views;

public class RoleSelectedBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var selectedRole = value as string;
        var role = parameter as string;
        var isSelected = selectedRole == role;

        return isSelected
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#66BD76"))
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCE9DE"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class RoleSelectedBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var selectedRole = value as string;
        var role = parameter as string;
        var isSelected = selectedRole == role;

        return isSelected
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F8EE"))
            : new SolidColorBrush(Colors.White);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class RoleSelectedThicknessConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var selectedRole = value as string;
        var role = parameter as string;
        var isSelected = selectedRole == role;

        return isSelected ? new Thickness(2) : new Thickness(1);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
