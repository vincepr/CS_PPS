namespace PpsCommon.Models.PpsModels;

/// Endpoint: https://[domain]:[port]/api/{version}/rest/configuration/{client}
public record ClientConfig
{
    public required string Name { get; init; }
    public required string ConfigXml { get; init; }
}