using System;
using System.Globalization;
using System.Windows.Data;

namespace KHAONPOS.Converters;

public class EqualityToBooleanConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2 || values[0] == null || values[1] == null)
            return false;

        // Compare by Id if both are Categories, or standard Equals comparison
        if (values[0] is KHAONPOS.Data.Entities.Category cat1 && values[1] is KHAONPOS.Data.Entities.Category cat2)
        {
            return cat1.Id == cat2.Id;
        }

        return values[0].Equals(values[1]);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
