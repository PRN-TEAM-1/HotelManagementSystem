namespace BusinessObjects.DTOs;

public sealed class LookupItemDto
{
    public int? Id { get; init; }

    public string Value { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string? Description { get; init; }

    public bool IsDisabled { get; init; }

    public override string ToString() => DisplayName;
}
