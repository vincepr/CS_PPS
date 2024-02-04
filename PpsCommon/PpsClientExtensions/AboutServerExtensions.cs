using PpsCommon.Dtos;
using PpsCommon.Models.PpsModels;

namespace PpsCommon.PpsClientExtensions;

public static class AboutServerExtensions
{
    public static Task<AboutServer> AboutServer(this PpsClient ppsClient) 
        => ppsClient.GenericGet<AboutServer>("/api/v5/rest/about");
    
    public static Task<bool> IsOfflineAvailable(this PpsClient ppsClient) 
        => ppsClient.GenericBool("/api/v5/rest/IsOfflineAvailable");

    public static Task<OfflinePackage> GetOfflinePackage(this PpsClient ppsClient)
        => ppsClient.GenericGet<OfflinePackage>("/api/v5/rest/offlinepackage");
    
    public static Task<SearchResults> Search(this PpsClient ppsClient, string searchTerm)
        => ppsClient.GenericPost<SearchRequestDto, SearchResults>(new(searchTerm), "/api/v5/rest/search");
    
    public static Task<ClientConfig> ClientConfiguration(this PpsClient ppsClient, string client) 
        => ppsClient.GenericGet<ClientConfig>($"/api/v5/rest/configuration/{client}");
}