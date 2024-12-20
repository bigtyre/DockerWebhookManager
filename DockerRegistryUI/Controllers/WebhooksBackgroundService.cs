﻿using DockerRegistryUI.BackgroundServiceStatus;
using Newtonsoft.Json;

namespace DockerRegistryUI.Controllers
{
    public class WebhooksBackgroundService(
        WebhooksService webhooksService,
        WebhooksQueue queue,
        IBackgroundServiceStatusTracker serviceStatusTracker
        ) : BackgroundService
    {
        private Dictionary<string, DateTime> LastProcessTimes { get; } = [];

        public async Task ProcessWebhookAsync(string webhookBody)
        {
            var now = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(10));
            Console.WriteLine($"{now} Received webhook POST. Request body is below.");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine(webhookBody);
            Console.WriteLine("-----------------------------------");
            Console.WriteLine();

            var request = JsonConvert.DeserializeObject<DockerRegistryEventsRequest>(webhookBody);
            if (request is null)
                return;

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

                if (LastProcessTimes.TryGetValue(repositoryName, out DateTime lastProcessTime))
                {
                    var timeDiff = currentTime - lastProcessTime;
                    if (timeDiff < minTimeBetweenWebhooks)
                    {
                        Console.WriteLine($"Ignoring webhook. A webhook has already been processed for this repository within the time threshold ({minTimeBetweenWebhooks}). Last call was at {lastProcessTime} ({timeDiff} ago).");
                        continue;
                    }
                }

                LastProcessTimes[repositoryName] = currentTime;

                await webhooksService.HandleEventAsync(evt);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(async () =>
            {
                Console.WriteLine("Webhooks Background Service started");
                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            serviceStatusTracker.UpdateStatus(nameof(WebhooksBackgroundService), ServiceStatus.Running);

                            while (queue.TryDequeue(out var webhookBody))
                            {
                                try
                                {
                                    serviceStatusTracker.UpdateStatus(nameof(WebhooksBackgroundService), ServiceStatus.Running);

                                    Console.WriteLine("Dequeued webhook. Processing it.");

                                    await ProcessWebhookAsync(webhookBody);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.ToString());
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }

                        await Task.Delay(500, stoppingToken);
                    }
                }
                finally
                {
                    Console.WriteLine("Webhooks Background Service exited.");
                    serviceStatusTracker.UpdateStatus(nameof(WebhooksBackgroundService), ServiceStatus.Stopped);
                }
            }, stoppingToken);
        }
    }
}
