using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace FerretFoodSolver.Converters
{
    class PercentConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return $"{value as double? ?? double.NaN:0.00%}";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                return double.Parse((value as string)!.TrimEnd(new char[] { ' ', '%' }))/100;
            } catch (FormatException e)
            {
                return new BindingNotification(e, BindingErrorType.DataValidationError);
            }
        }
    }
}
