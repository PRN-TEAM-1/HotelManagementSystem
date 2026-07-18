using System.Globalization;

namespace WPF.Helpers;

public static class MoneyInputParser
{
    public static bool TryParse(string? value, out decimal amount)
    {
        amount = 0m;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = NormalizeSeparators(value.Trim().Replace(" ", string.Empty));
        return decimal.TryParse(
            normalized,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out amount);
    }

    public static string FormatInput(decimal amount)
    {
        return amount.ToString("0.##", CultureInfo.CurrentCulture);
    }

    private static string NormalizeSeparators(string value)
    {
        var lastComma = value.LastIndexOf(',');
        var lastDot = value.LastIndexOf('.');

        if (lastComma >= 0 && lastDot >= 0)
        {
            var decimalSeparator = lastComma > lastDot ? ',' : '.';
            var groupSeparator = decimalSeparator == ',' ? "." : ",";

            return value
                .Replace(groupSeparator, string.Empty)
                .Replace(decimalSeparator, '.');
        }

        if (lastComma >= 0)
        {
            return NormalizeSingleSeparator(value, ',');
        }

        if (lastDot >= 0)
        {
            return NormalizeSingleSeparator(value, '.');
        }

        return value;
    }

    private static string NormalizeSingleSeparator(string value, char separator)
    {
        var parts = value.Split(separator);
        if (parts.Length > 2)
        {
            return parts.Skip(1).All(part => part.Length == 3)
                ? string.Concat(parts)
                : value;
        }

        var separatorIndex = value.IndexOf(separator);
        var digitsAfterSeparator = value.Length - separatorIndex - 1;

        return digitsAfterSeparator == 3 && separatorIndex > 0
            ? value.Replace(separator.ToString(), string.Empty)
            : value.Replace(separator, '.');
    }
}
