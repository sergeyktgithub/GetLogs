#region

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#endregion

namespace GetLogsClient.Convertors
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() != typeof(bool))
                throw new Exception("Входное значение конвертора должно быть типа bool");
            if (targetType != typeof(Visibility))
                throw new Exception("Целевой тип должен быть Visibility");

            var val = (bool) value;

            return val ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}