namespace PpsCommon.Models.PpsModels;

/// Endpoints: https://[domain]:[port]/api/v5/rest/offlinepackage
public record OfflinePackage
{
    public required CredentialGroup Root { get; init; }
    public required List<Attachment> Attachment { get; init; }
    public required DateTimeOffset Expiry { get; init; }
}