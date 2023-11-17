using System.Collections.Concurrent;

namespace DockerRegistryUI.Controllers
{
    public class WebhooksQueue
    {
        private ConcurrentQueue<string> QueuedWebhooks = new ConcurrentQueue<string>();

        public void HandleWebhook(string webhookBody)
        {
            QueuedWebhooks.Enqueue(webhookBody);
        }

        public bool TryDequeue(out string webhookBody)
        {
            return QueuedWebhooks.TryDequeue(out webhookBody);
        }
    }
}
