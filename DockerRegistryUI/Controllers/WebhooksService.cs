using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
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

    public class WebhooksBackgroundService : BackgroundService
    {
        public WebhooksBackgroundService(WebhooksService webhooksService, WebhooksQueue queue)
        {
            WebhooksService = webhooksService;
            Queue = queue;
        }

        public WebhooksService WebhooksService { get; }
        public WebhooksQueue Queue { get; }

        private Dictionary<string, DateTime> LastProcessTimes { get; } = new();

        public async Task ProcessWebhookAsync(string webhookBody)
        {
            var now = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(10));
            Console.WriteLine($"{now} Received webhook POST. Request body is below.");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine(webhookBody);
            Console.WriteLine("-----------------------------------");
            Console.WriteLine();


            
            var request = JsonConvert.DeserializeObject<DockerRegistryEventsRequest>(webhookBody);
            if (request is not null)
            {
                var currentTime = DateTime.Now;
                var minTimeBetweenWebhooks = TimeSpan.FromSeconds(10);
                foreach (var evt in request.Events)
                {
                    if ("push".Equals(evt.Action, StringComparison.OrdinalIgnoreCase) is false)
                    {
                        Console.WriteLine($"Ignoring webhook. Action is '{evt.Action}', not push.");
                        continue;
                    }

                    var repositoryName = evt.Target.Repository;

                    if (LastProcessTimes.TryGetValue(repositoryName, out DateTime lastProcessTime) )
                    {
                        var timeDiff = currentTime - lastProcessTime;
                        if (timeDiff < minTimeBetweenWebhooks)
                        {
                            Console.WriteLine($"Ignoring webhook. A webhook has already been processed for this repository within the time threshold ({minTimeBetweenWebhooks}). Last call was at {lastProcessTime} ({timeDiff} ago).");
                            continue;
                        }
                    }

                    LastProcessTimes[repositoryName] = currentTime;

                    await WebhooksService.HandleEventAsync(evt);
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Webhooks Background Service started");
            try { 
                while (!stoppingToken.IsCancellationRequested)
                {
                    while (Queue.TryDequeue(out var webhookBody))
                    {
                        try {
                            Console.WriteLine("Dequeued webhook. Processing it.");

                            await ProcessWebhookAsync(webhookBody);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }

                    await Task.Delay(500, stoppingToken);
                }
            }
            finally
            {
                Console.WriteLine("Webhooks Background Service exited.");
            }
        }
    }

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
