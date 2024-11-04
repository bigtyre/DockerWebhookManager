namespace DockerRegistry.Configuration;
public record RegistrySettings(Uri Uri, string Username, string Password);

public record AppRegistrySettings
{
    public Uri? Uri { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }


    public AppRegistrySettings()
    {
    }
}