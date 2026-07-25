namespace Services.AI;

internal static class AiJsonResponseHelper
{
    public static string ExtractJsonObject(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "{}";
        }

        var trimmed = value.Trim();
        if (trimmed.StartsWith('{') && trimmed.EndsWith('}'))
        {
            return trimmed;
        }

        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');

        return start >= 0 && end > start
            ? trimmed[start..(end + 1)]
            : "{}";
    }
}
