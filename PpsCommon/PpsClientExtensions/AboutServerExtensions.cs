using PpsCommon.Dtos;
using PpsCommon.Models.PpsModels;

namespace PpsCommon.PpsClientExtensions;

public static class AboutServerExtensions
{
    public static Task<AboutServer> AboutServer(this PpsClient ppsClient, CancellationToken token = default) 
        => ppsClient.GenericGet<AboutServer>("/api/v5/rest/about", token);
    
    public static Task<bool> IsOfflineAvailable(this PpsClient ppsClient, CancellationToken token = default) 
        => ppsClient.GenericBool("/api/v5/rest/IsOfflineAvailable", token);

    public static Task<OfflinePackage> GetOfflinePackage(this PpsClient ppsClient, CancellationToken token = default)
        => ppsClient.GenericGet<OfflinePackage>("/api/v5/rest/offlinepackage", token);
    
    public static Task<SearchResults> Search(this PpsClient ppsClient, string searchTerm, CancellationToken token = default)
        => ppsClient.GenericPost<SearchRequestDto, SearchResults>(
            new(searchTerm), "/api/v5/rest/search", token);
    
    public static Task<ClientConfig> ClientConfiguration(this PpsClient ppsClient, string client, CancellationToken token = default) 
        => ppsClient.GenericGet<ClientConfig>($"/api/v5/rest/configuration/{client}", token);
}