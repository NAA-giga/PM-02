using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

public class IsBlockedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() == "blocked";
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
