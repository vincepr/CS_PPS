namespace PpsCommon.Models.PpsModels;

/// POST requests ONLY. Request body should contain a JSON or XML object with a single field 'Password' with the password that is to be mesaured. For example:
/// Endpoints: https://[domain]:[port]/api/{version}/rest/passwordstrength
public record PasswordStrength
{
    public required decimal NumericalStrength { get; init; }
    public required string DescriptiveStrength { get; init; }
}