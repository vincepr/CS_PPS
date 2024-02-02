namespace PpsCommon.Models.PpsModels;

/// one of the two search result objects
public record CredentialGroupSearchResult
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string FullPath { get; init; }
}