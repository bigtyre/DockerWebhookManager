using DockerRegistryUI.Controllers;
using Microsoft.AspNetCore.Components;
using static DockerRegistryUI.Pages.RepositoryList;

namespace DockerRegistryUI.Pages
{
    public partial class WebhookList : IDisposable
    {
        private bool _isDisposed;

        List<Webhook> Webhooks { get; } = new();

        List<WebhookViewModel> WebhookViewModels { get; } = new();

        [Inject]
        public WebhookRepository WebhookRepository { get; set; } = null!;

        [Inject]
        public EventSystem EventSystem { get; set; } = null!;

        [Parameter]
        public LoadStatus LoadStatus { get; set; } = LoadStatus.Pending;

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            EventSystem.WebhookCalled += WebhookCalled;
        }

        private void WebhookCalled(object? sender, WebhookEventArgs e)
        {
            try {

                var webhookId = e.WebhookId;
                Console.WriteLine($"Webhook {webhookId} called.");

                var webhook = WebhookViewModels.FirstOrDefault(c => c.Webhook.Id == webhookId);
                if (webhook is null)
                    return;

                if (webhook.HasLoadedCallHistory)
                {
                    Console.WriteLine($"Loading webhook call history for {webhookId} following webhook called event.");
                    webhook.LoadCallHistory();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error while handling webhook called event: {ex.Message}");
            }
        }

        public void SetWebhooks(IEnumerable<Webhook> webhooks)
        {
            Webhooks.Clear();
            Webhooks.AddRange(webhooks);

            var viewModels = webhooks.Select(s => new WebhookViewModel(WebhookRepository, s)).ToList();
            var existingVms = WebhookViewModels.ToList();
            foreach (var vm in existingVms)
            {
                RemoveWebhookViewModel(vm);
            }
            WebhookViewModels.AddRange(viewModels);
            foreach (var vm in viewModels)
            {
                vm.CallHistoryChanged += WebhookCallHistoryChanged;
            }
        }

        public void DeleteWebhook(Webhook webhook)
        {
            WebhookRepository.DeleteWebhook(webhook.Id);
            RemoveWebhook(webhook);

            var vms = WebhookViewModels.Where(w => w.Webhook == webhook).ToList();
            foreach (var vm in vms)
            {
                RemoveWebhookViewModel(vm);
            }
        }

        private void RemoveWebhookViewModel(WebhookViewModel vm)
        {
            WebhookViewModels.Remove(vm);
            vm.CallHistoryChanged -= WebhookCallHistoryChanged;
        }

        private void WebhookCallHistoryChanged(object? sender, EventArgs e)
        {
            StateHasChanged();
        }

        private void RemoveWebhook(Webhook webhook)
        {
            if (Webhooks.Contains(webhook))
            {
                Webhooks.Remove(webhook);
            }
        }


        protected virtual void Dispose(bool isDisposing)
        {
            if (_isDisposed)
                return;

            if (isDisposing)
            {
                EventSystem.WebhookCalled -= WebhookCalled;
            }

            _isDisposed = true;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(isDisposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
