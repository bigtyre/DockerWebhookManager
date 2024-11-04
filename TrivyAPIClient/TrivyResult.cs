namespace TrivyAPIClient
{
    public class TrivyResult
    {
        public string? Class { get; set; }
        public string? Target { get; set; }
        public string? Type { get; set; }
        public List<Vulnerability> Vulnerabilities { get; set; } = [];
    }
}
