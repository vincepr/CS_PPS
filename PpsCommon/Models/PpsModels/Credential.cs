namespace PpsCommon.Models.PpsModels;

/// Credential(Entry)
/// Endpoints: https://[domain]:[port]/api/v5/rest/entries/{id}
public record Credential
{
    public required Guid Id { get; set; }
    public required Guid GroupId { get; set; }

    public string? Name { get; set; }
    public string? Password { get; set; }
    public string? Url { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset? Expires { get; set; }
    public Dictionary<string, string>? CustomUserFields { get; set; }
    public Dictionary<string, string>? CustomApplicationFields { get; set; }
    public List<Tag>? Tags { get; set; }

    public required DateTimeOffset Created { get; init; }
    public required DateTimeOffset Modified { get; init; }
    public required bool HasModifyEntriesAccess { get; init; }
    public required bool HasViewEntryContentsAccess { get; init; }
    public required CommentPrompt CommentPrompts { get; init; }
}