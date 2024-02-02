namespace PpsCommon.Models.PpsModels;

/// This object is used by entry and folder to mark if usage comments are required in certain situations.
/// These objects are not required for Updates or Inserts of entries and folders.
public record CommentPrompt
{
    public required bool AskForCommentOnViewPassword { get; init; }
    public required bool AskForCommentOnViewOffline { get; init; }
    public required bool AskForCommentOnModifyEntries { get; init; }
    public required bool AskForCommentOnMoveEntries { get; init; }
    public required bool AskForCommentOnMoveFolders { get; init; }
    public required bool AskForCommentOnModifyFolders { get; init; }
}