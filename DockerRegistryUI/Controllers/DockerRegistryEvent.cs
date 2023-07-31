namespace DockerRegistryUI.Controllers
{
    class DockerRegistryEvent
    {
        public string Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Action { get; set; }
        public DockerRegistryEventTarget Target { get; set; }
    }
}
