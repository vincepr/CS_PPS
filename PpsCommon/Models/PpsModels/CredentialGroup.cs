namespace PpsCommon.Models.PpsModels;

/// CredentialGroup(Folder)
/// Endpoints: https://[domain]:[port]/api/v5/rest/folders/{id}
public record CredentialGroup
{
    public required Guid Id { get; init; }
    public required string Name { get; set; }
    public required Guid ParentId { get; set; }
    
    public string? Notes { get; set; }
    public DateTimeOffset? Expires { get; set; }
    public Dictionary<string, string>? CustomUserFields { get; set; }
    public Dictionary<string, string>? CustomApplicationFields { get; set; }
    public List<Tag>? Tags { get; set; }

    // readonly
    public required List<CredentialGroup> Children { get; init; }
    public required List<Credential> Credentials { get; init; }
    public required DateTimeOffset Created { get; init; }
    public required DateTimeOffset Modified { get; init; }
    public required bool HasModifyEntriesAccess { get; init; }
    public required bool HasViewEntryContentsAccess { get; init; }
    public required CommentPrompt CommentPrompts { get; init; }
}