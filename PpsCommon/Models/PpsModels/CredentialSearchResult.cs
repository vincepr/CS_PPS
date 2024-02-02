namespace PpsCommon.Models.PpsModels;

/// one of the two search result objects
public record CredentialSearchResult
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Username { get; init; }
    public required string Url { get; init; }
    public required string Notes { get; init; }
    public required Guid GroupId { get; init; }
    public required Guid Path { get; init; }
}