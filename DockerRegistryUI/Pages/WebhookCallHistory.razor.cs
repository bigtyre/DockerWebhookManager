using DockerRegistryUI.Controllers;
using Microsoft.AspNetCore.Components;

namespace DockerRegistryUI.Pages
{
    public partial class WebhookCallHistory
    {
        [Inject]
        public WebhookRepository WebhookRepository { get; set; } = null!;

        public List<WebhookResultViewModel> WebhookResults { get; } = new();

        public DateTime StartDate { get; set; } = DateTime.Now.AddDays(-30);
        public bool WebhookResultsLoading { get; set; }
        public bool WebhookResultsLoaded { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            await LoadDataAsync();
            await base.OnParametersSetAsync();
        }

        private async Task LoadDataAsync()
        {
            await LoadWebhookResultHistory();
        }

        private async Task LoadWebhookResultHistory()
        {
            WebhookResultsLoading = true;
            try
            {
                StateHasChanged();

                var webhookResultViewModels = await Task.Run(() =>
                {
                    var webhooks = WebhookRepository.GetWebhooks().ToDictionary(s => s.Id, s => s);
                    var results = WebhookRepository.GetWebhookResults().OrderByDescending(c => c.Time);
                    return results.Select(s => CreateWebhookResultViewModel(s, webhooks));
                });

                WebhookResults.Clear();
                WebhookResults.AddRange(webhookResultViewModels);
                WebhookResultsLoaded = true;
            }
            finally
            {
                WebhookResultsLoading = false;
                StateHasChanged();
            }
        }

        private static WebhookResultViewModel CreateWebhookResultViewModel(WebhookResult webhookCallResult, Dictionary<int, Webhook> webhooks)
        {
            var webhookId = webhookCallResult.WebhookId;
            webhooks.TryGetValue(webhookId, out Webhook? webhook);

            var webhookCallViewModel = new WebhookResultViewModel(
                webhook,
                webhookCallResult.Time,
                webhookCallResult.ResponseCode
            );

            return webhookCallViewModel;
        }

        public string GetRepositoryUrl(string repositoryName) => $"repositories/{repositoryName}";
    }

    public record WebhookResultViewModel(Webhook? Webhook, DateTimeOffset Time, int ResponseCode);
}
