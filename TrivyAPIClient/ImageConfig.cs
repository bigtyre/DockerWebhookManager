namespace TrivyAPIClient
{
    public class ImageConfig
    {
        public string? Architecture { get; set; }
        public Config? Config { get; set; }
        public DateTime Created { get; set; }
        public List<History> History { get; set; } = [];
        public string? OS { get; set; }
        public Rootfs? Rootfs { get; set; }
    }
}
