namespace DockerRegistryUI.Controllers
{
    public class WebhooksService
    {
        public WebhooksService(WebhookRepository webhookRepository)
        {
            WebhookRepository = webhookRepository;
        }

        public WebhookRepository WebhookRepository { get; }

        internal async Task HandleEventAsync(DockerRegistryEvent evt)
        {
            var action = evt.Action;

            bool isPush = "push".Equals(action, StringComparison.OrdinalIgnoreCase);
            if (isPush is false)
                return;

            var repositoryName = evt.Target.Repository;
            if (repositoryName == null)
                return;

            Console.WriteLine($"PUSH received on repository '{repositoryName}'. Checking for registered webhooks.");

            var webhooks = WebhookRepository.GetWebhooksByRepositoryName(repositoryName).ToList();

            int numWebhooks = webhooks.Count;
            Console.WriteLine($"{numWebhooks} webhook{(numWebhooks == 1 ? "" : "s")} found for '{repositoryName}'.");
            if (numWebhooks == 0)
            {
                return;
            }

            int i = 0;
            foreach (var webhook in webhooks)
            {
                i++;
                try
                {
                    Console.WriteLine($"Executing webhook {i}/{numWebhooks}");
                    await ExecuteWebhookAsync(webhook);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to execute webhook: " + ex.Message);
                }
            }
        }

        private async Task ExecuteWebhookAsync(Webhook webhook)
        {
            Uri? url = webhook.Url;
            var webhookId = webhook.Id;

            // TODO Record the attempt

            Console.WriteLine($"POSTing to {url}");

            int httpStatusCode = 0;
            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(url, null);
                var httpStatus = response.StatusCode;
                httpStatusCode = (int)httpStatus;
                
                Console.WriteLine($"POST to {url} for webhook ID {webhookId} returned status {httpStatus}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"POST to {url} for webhook ID {webhookId} failed. " + ex.Message);
            }
            finally
            {
                try
                {
                    WebhookRepository.CreateWebhookCallHistory(webhookId, DateTimeOffset.Now, httpStatusCode);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to record webhook call history: " + ex.Message);
                    Console.WriteLine(ex.Message);
                }
            }

            // TODO Record the result
        }
    }
}
