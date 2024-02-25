namespace PpsCommon;

public record PpsClientConfiguration(string Url, string Username, string Password)
{
    public static PpsClientConfiguration ParseEnvironmentVariablesStrict()
    {
        var url = Environment.GetEnvironmentVariable("PPS_URL") 
                      ?? throw new Exception("Missing ENV-Variable PPS_URL");
        var username = Environment.GetEnvironmentVariable("PPS_USERNAME")
                      ?? throw new Exception("Missing ENV-Variable PPS_USERNAME");
        var password = Environment.GetEnvironmentVariable("PPS_PASSWORD")
                      ?? throw new Exception("Missing ENV-Variable PPS_PASSWORD");
        return new PpsClientConfiguration(url, username, password);
    }
    
    public static (string? Url,string? Username,string? Password) ParseEnvironmentVariables()
    {
        var url = Environment.GetEnvironmentVariable("PPS_URL");
        var username = Environment.GetEnvironmentVariable("PPS_USERNAME");
        var password = Environment.GetEnvironmentVariable("PPS_PASSWORD");
        return (url, username, password);
    }
};