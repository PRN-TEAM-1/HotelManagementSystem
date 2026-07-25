namespace Services.AI;

internal interface IAiProviderClient
{
    Task<string> GenerateJsonAsync(
        AiProviderRequestOptions options,
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default);
}
