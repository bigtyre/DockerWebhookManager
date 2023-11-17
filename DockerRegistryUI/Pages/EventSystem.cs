namespace DockerRegistryUI.Pages
{
    public class EventSystem
    {
        public event EventHandler<WebhookEventArgs>? WebhookCalled;

        internal void OnWebhookCalled(object sender, WebhookEventArgs args)
        {
            WebhookCalled?.Invoke(sender, args);
        }
    }
}
