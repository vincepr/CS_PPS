namespace PpsCommon.Models.PpsModels;

/// Used by entries and folders to hold file attachments.
public record Attachment
{
    public required Guid CredentialObjectId { get; set; }
    public Guid? AttachmentId { get; init; }
    public required string FileName { get; set; }
    public byte[]? FileData { get; set; }
    public long? FileSize { get; init; }
}