using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Challenges_App.Controls.Converter
{
    class SpacingToMarginConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Thickness(0, (double)value, 0, 0);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
