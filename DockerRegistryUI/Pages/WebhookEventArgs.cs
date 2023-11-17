namespace DockerRegistryUI.Pages
{
    public class WebhookEventArgs : EventArgs
    {
        public WebhookEventArgs(int id)
        {
            WebhookId = id;
        }

        public int WebhookId { get; set; }
    }
}
