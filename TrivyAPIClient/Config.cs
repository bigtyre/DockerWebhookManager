namespace TrivyAPIClient
{
    public class Config
    {
        public List<string> Entrypoint { get; set; } = [];
        public List<string> Env { get; set; } = [];
        public Dictionary<string, object> ExposedPorts { get; set; } = [];
        public Dictionary<string, string> Labels { get; set; } = [];
        public string? WorkingDir { get; set; }
    }
}
