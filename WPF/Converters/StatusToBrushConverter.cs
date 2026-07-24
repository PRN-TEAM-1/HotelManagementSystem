using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WPF.Converters;

public sealed class StatusToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value?.ToString() ?? string.Empty;

        if (string.Equals(status, "Available", StringComparison.OrdinalIgnoreCase))
        {
            return new SolidColorBrush(Color.FromRgb(46, 125, 50)); // Green #2E7D32
        }

        if (string.Equals(status, "Occupied", StringComparison.OrdinalIgnoreCase))
        {
            return new SolidColorBrush(Color.FromRgb(198, 40, 40)); // Red #C62828
        }

        if (string.Equals(status, "Reserved", StringComparison.OrdinalIgnoreCase))
        {
            return new SolidColorBrush(Color.FromRgb(21, 101, 192)); // Blue #1565C0
        }

        if (string.Equals(status, "Cleaning", StringComparison.OrdinalIgnoreCase))
        {
            return new SolidColorBrush(Color.FromRgb(245, 127, 23)); // Amber #F57F17
        }

        // Maintenance, Inactive, Default
        return new SolidColorBrush(Color.FromRgb(97, 97, 97)); // Gray #616161
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
