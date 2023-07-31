using DockerRegistry.Configuration;

namespace BigTyre.Configuration;
class AppSettings
{
    public string? MySqlConnectionString { get; set; }
    public RegistrySettings? Registry { get; set; }
}
