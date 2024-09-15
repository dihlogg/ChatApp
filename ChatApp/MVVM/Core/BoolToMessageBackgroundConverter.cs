using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

namespace ChatClient.MVVM.Core
{
    public class BoolToMessageBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSentByMe)
            {
                return isSentByMe ? Brushes.LightGray : Brushes.LightBlue;
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BoolToHorizontalAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSentByMe)
            {
                return isSentByMe ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            }
            return HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
