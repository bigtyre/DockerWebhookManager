namespace TrivyAPIClient
{
    public class TrivyScanResult
    {
        public string? ArtifactName { get; set; }
        public string? ArtifactType { get; set; }
        public DateTime CreatedAt { get; set; }
        public Metadata? Metadata { get; set; }
        public List<TrivyResult> Results { get; set; } = [];
        public int SchemaVersion { get; set; }
    }
}
