using System.Globalization;

namespace Equaly.Converters
{
    public class BalanceToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal balance)
            {
                if (balance > 0)
                    return Color.FromArgb("#2ECC71"); // yeşil - alacaklı

                if (balance < 0)
                    return Color.FromArgb("#E74C3C"); // kırmızı - borçlu

                return Color.FromArgb("#6C757D"); // gri - sıfır bakiye
            }

            return Color.FromArgb("#6C757D");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
