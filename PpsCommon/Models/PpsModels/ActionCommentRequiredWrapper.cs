namespace PpsCommon.Models.PpsModels;

/// This wrapper is used with entries or folders when a comment is required.
/// used for ActionCommentRequiredWrapper<Credential> or ActionCommentRequiredWrapper<CredentialGroup> 
public record ActionCommentRequiredWrapper<T>
{
    public required T Item { get; set; }
    public required string Comment { get; set; }
}