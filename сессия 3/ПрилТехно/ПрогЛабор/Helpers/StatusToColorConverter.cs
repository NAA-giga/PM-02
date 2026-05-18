using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ПрогЛабор.Helpers
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value as string;
            return status switch
            {
                "pending" => new SolidColorBrush(Colors.LightGray),
                "in_progress" => new SolidColorBrush(Colors.LightYellow),
                "approved" => new SolidColorBrush(Colors.LightGreen),
                "blocked" => new SolidColorBrush(Colors.LightCoral),
                _ => new SolidColorBrush(Colors.White)
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}