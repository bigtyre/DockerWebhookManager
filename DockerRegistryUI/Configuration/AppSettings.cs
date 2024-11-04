using DockerRegistry.Configuration;

namespace BigTyre.Configuration;
class AppSettings
{
    public string? MySqlConnectionString { get; set; }
    public RegistrySettings? Registry { get; set; }
    public string? BasePath { get; set; }
    public Uri? TrivyApiUrl { get; set; }

}
