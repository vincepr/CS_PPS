using PpsCommon.Models.PpsModels;

namespace PpsCommon.PpsClientExtensions;

public static class CredentialGroupExtensions
{
    public static Task<CredentialGroup> GetEntireTree(this PpsClient ppsClient) 
        => ppsClient.GenericGet<CredentialGroup>("/api/v5/rest/folders/");
}