namespace Services.AI;

internal static class AiProviderDefaults
{
    public const string GeminiModelName = "gemini-3.5-flash";
    public const string GeminiBaseEndpointUrl = "https://generativelanguage.googleapis.com/v1/models";

    private const string LegacyGeminiModelName = "gemini-1.5-flash";
    private const string LegacyGeminiEndpointUrl = "https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";

    public static string NormalizeGeminiModelName(string modelName)
    {
        return string.Equals(modelName, LegacyGeminiModelName, StringComparison.OrdinalIgnoreCase)
            ? GeminiModelName
            : modelName;
    }

    public static string? NormalizeGeminiEndpointUrl(string? endpointUrl)
    {
        if (string.IsNullOrWhiteSpace(endpointUrl))
        {
            return GeminiBaseEndpointUrl;
        }

        return string.Equals(endpointUrl.Trim(), LegacyGeminiEndpointUrl, StringComparison.OrdinalIgnoreCase)
            ? GeminiBaseEndpointUrl
            : endpointUrl.Trim();
    }
}
