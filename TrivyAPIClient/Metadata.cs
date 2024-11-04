namespace TrivyAPIClient
{
    public class Metadata
    {
        public List<string> DiffIDs { get; set; } = [];
        public ImageConfig? ImageConfig { get; set; }
        public string? ImageID { get; set; }
        public OSInfo? OS { get; set; }
        public List<string> RepoDigests { get; set; } = [];
        public List<string> RepoTags { get; set; } = [];
    }
}
