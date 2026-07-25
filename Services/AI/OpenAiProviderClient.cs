using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Services.AI;

internal sealed class OpenAiProviderClient : IAiProviderClient
{
    private const string DefaultEndpoint = "https://api.openai.com/v1/chat/completions";

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

        var endpoint = string.IsNullOrWhiteSpace(options.EndpointUrl)
            ? DefaultEndpoint
            : options.EndpointUrl.Trim();

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);

        var body = new
        {
            model = options.ModelName,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = (double)options.Temperature,
            max_tokens = options.MaxOutputTokens,
            response_format = new { type = "json_object" }
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
                $"OpenAI request failed ({(int)response.StatusCode}): {TrimForMessage(content)}");
        }

        using var document = JsonDocument.Parse(content);
        if (!document.RootElement.TryGetProperty("choices", out var choices)
            || choices.GetArrayLength() == 0
            || !choices[0].TryGetProperty("message", out var message)
            || !message.TryGetProperty("content", out var messageContent))
        {
            throw new InvalidOperationException("OpenAI response did not contain message content.");
        }

        return messageContent.GetString() ?? string.Empty;
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
