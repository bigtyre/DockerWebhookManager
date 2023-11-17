namespace DockerRegistryUI.Controllers
{
    public class Webhook
    {
        public int Id { get; set; }
        public string RepositoryName { get; set; }
        public Uri Url { get; set; }
    }

    public class WebhookResult
    {
        public int WebhookId { get; set; }
        public DateTimeOffset Time { get; set; }
        public int ResponseCode { get; set; }
    }
}
