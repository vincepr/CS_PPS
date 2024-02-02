namespace PpsCommon.Models.PpsModels;

/// Endpoints: https://[domain]:[port]/api/{version}/rest/search
/// POST requests ONLY. Request body should contain a JSON or XML object with a single field 'Search' with the string that is to be searched for. For example:
public record SearchResults
{
    public required List<CredentialSearchResult>? Credentials { get; init; }
    public required List<CredentialGroupSearchResult>? Groups { get; init; }
}