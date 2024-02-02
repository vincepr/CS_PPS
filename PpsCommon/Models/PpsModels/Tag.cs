namespace PpsCommon.Models.PpsModels;

/// Used by entries and folders to hold Tags
public record Tag
{
    public required string Name { get; set; }
}