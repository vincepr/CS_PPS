namespace PpsCommon.Models.PpsModels;

/// Used by entries and folders to hold file attachments.
public record Attachment
{
    public required Guid CredentialObjectId { get; set; }
    public required string FieldName { get; set; }
    public required byte[] FileData { get; set; }
    
    public long? FileSize { get; init; }
}