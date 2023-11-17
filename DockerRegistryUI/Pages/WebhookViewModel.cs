using DockerRegistryUI.Controllers;

namespace DockerRegistryUI.Pages
{
    public class WebhookViewModel : ViewModel
    {
        public WebhookViewModel(WebhookRepository webhookRepository, Webhook webhook)
        {
            WebhookRepository = webhookRepository;
            Webhook = webhook;
        }

        public WebhookRepository WebhookRepository { get; }
        public Webhook Webhook { get; }
        public List<WebhookResult> CallHistory { get; } = new();


        private bool _hasLoadedCallHistory;
        public bool HasLoadedCallHistory { get => _hasLoadedCallHistory; set => SetProperty(ref _hasLoadedCallHistory, value); }

        public event EventHandler? CallHistoryChanged;

        public void LoadCallHistory()
        {
            var calls = WebhookRepository.GetWebhookResults(Webhook.Id);
            calls = calls.OrderByDescending(c => c.Time).ToList();

            CallHistory.Clear();
            CallHistory.AddRange(calls);
            HasLoadedCallHistory = true;

            OnCallHistoryChanged();
        }

        private void OnCallHistoryChanged()
        {
            CallHistoryChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
