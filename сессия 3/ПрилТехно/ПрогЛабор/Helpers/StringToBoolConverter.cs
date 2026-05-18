using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

public class StringToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() == parameter?.ToString();
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value is true) ? parameter?.ToString() : null;
    }
}
