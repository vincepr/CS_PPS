namespace PpsCommon.Models.PpsModels;

/// Endpoints: https://[domain]:[port]/api/{version}/rest/about
public record AboutServer
{
    public required string ServerVersion { get; set; }
    public required string NetVersion { get; set; }
    public required string OsVersion { get; set; }
    public required string PortSettings { get; set; }
    public required string DnsInformation { get; set; }
    public required string Ping { get; set; }
}