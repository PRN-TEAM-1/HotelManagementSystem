using BusinessObjects.Enums;

namespace Services.AI;

public sealed class AiProviderClientFactory
{
    private readonly OpenAiProviderClient _openAiProviderClient = new();
    private readonly GeminiProviderClient _geminiProviderClient = new();

    internal IAiProviderClient Create(AiProviderName providerName)
    {
        return providerName switch
        {
            AiProviderName.OpenAI => _openAiProviderClient,
            AiProviderName.Gemini => _geminiProviderClient,
            _ => throw new NotSupportedException($"AI provider '{providerName}' is not supported.")
        };
    }
}
