namespace DockerRegistry.Configuration;
public record RegistrySettings
{
    public Uri Uri { get; set; }
    public RegistrySettings()
    {
    }
}
