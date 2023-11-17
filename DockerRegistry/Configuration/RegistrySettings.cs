namespace DockerRegistry.Configuration;
public record RegistrySettings
{
    public Uri? Uri { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public RegistrySettings()
    {
    }
}
