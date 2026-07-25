using System.Text;
using System.Text.Json;

namespace Services.AI;

internal sealed class GeminiProviderClient : IAiProviderClient
{
    public async Task<string> GenerateJsonAsync(
        AiProviderRequestOptions options,
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(Math.Clamp(options.TimeoutSeconds, 5, 180))
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, BuildEndpoint(options));

        var body = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = $"{systemPrompt}\n\n{userPrompt}" }
                    }
                }
            },
            generationConfig = new
            {
                temperature = (double)options.Temperature,
                maxOutputTokens = options.MaxOutputTokens,
                responseMimeType = "application/json"
            }
        };

        request.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Gemini request failed ({(int)response.StatusCode}): {TrimForMessage(content)}");
        }

        using var document = JsonDocument.Parse(content);
        if (!document.RootElement.TryGetProperty("candidates", out var candidates)
            || candidates.GetArrayLength() == 0
            || !candidates[0].TryGetProperty("content", out var candidateContent)
            || !candidateContent.TryGetProperty("parts", out var parts)
            || parts.GetArrayLength() == 0
            || !parts[0].TryGetProperty("text", out var text))
        {
            throw new InvalidOperationException("Gemini response did not contain text content.");
        }

        return text.GetString() ?? string.Empty;
    }

    private static string BuildEndpoint(AiProviderRequestOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.EndpointUrl))
        {
            return $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(options.ModelName)}:generateContent?key={Uri.EscapeDataString(options.ApiKey)}";
        }

        var endpoint = options.EndpointUrl.Trim()
            .Replace("{model}", Uri.EscapeDataString(options.ModelName), StringComparison.OrdinalIgnoreCase)
            .Replace("{apiKey}", Uri.EscapeDataString(options.ApiKey), StringComparison.OrdinalIgnoreCase);

        if (endpoint.Contains("key=", StringComparison.OrdinalIgnoreCase)
            || endpoint.Contains(options.ApiKey, StringComparison.Ordinal))
        {
            return endpoint;
        }

        var separator = endpoint.Contains('?') ? "&" : "?";
        return $"{endpoint}{separator}key={Uri.EscapeDataString(options.ApiKey)}";
    }

    private static string TrimForMessage(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "empty response";
        }

        value = value.Replace(Environment.NewLine, " ").Trim();
        return value.Length <= 240 ? value : $"{value[..240]}...";
    }
}
