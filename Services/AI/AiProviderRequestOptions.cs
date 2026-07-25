using BusinessObjects.Enums;

namespace Services.AI;

internal sealed record AiProviderRequestOptions(
    AiProviderName ProviderName,
    string ModelName,
    string ApiKey,
    string? EndpointUrl,
    decimal Temperature,
    int MaxOutputTokens,
    int TimeoutSeconds);
