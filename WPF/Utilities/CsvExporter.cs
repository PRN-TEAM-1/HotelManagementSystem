using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Win32;

namespace WPF.Utilities;

public static class CsvExporter
{
    public static void ExportToCsv<T>(IEnumerable<T> data, string defaultFileName)
    {
        var items = data.ToList();

        var saveFileDialog = new SaveFileDialog
        {
            FileName = defaultFileName,
            DefaultExt = ".csv",
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
        };

        if (saveFileDialog.ShowDialog() != true)
        {
            return;
        }

        var csvContent = BuildCsv(items);

        File.WriteAllText(
            saveFileDialog.FileName,
            "\uFEFF" + csvContent,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    private static string BuildCsv<T>(IReadOnlyCollection<T> items)
    {
        var properties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var builder = new StringBuilder();

        builder.AppendLine(string.Join(",", properties.Select(p => Escape(p.Name))));

        foreach (var item in items)
        {
            var values = properties.Select(property =>
            {
                var value = property.GetValue(item);
                return Escape(FormatValue(value));
            });

            builder.AppendLine(string.Join(",", values));
        }

        return builder.ToString();
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => string.Empty,
            DateTime date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            decimal number => number.ToString(CultureInfo.InvariantCulture),
            double number => number.ToString("0.##", CultureInfo.InvariantCulture),
            float number => number.ToString("0.##", CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}