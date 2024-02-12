namespace DockerRegistryUI.BackgroundServiceStatus
{
    public class BackgroundServiceStatusTracker : IBackgroundServiceStatusTracker
    {
        private readonly Dictionary<string, ServiceStatus> _statuses = new();

        public void UpdateStatus(string serviceName, ServiceStatus status)
        {
            _statuses[serviceName] = status;
        }

        public ServiceStatus GetServiceStatus(string serviceName)
        {
            if (_statuses.TryGetValue(serviceName, out var status))
            {
                return status;
            }

            return ServiceStatus.Stopped; // Default or consider an Unknown status
        }

        public List<ServiceStatusSummary> GetServices()
        {
            return _statuses.Select(s => new ServiceStatusSummary(s.Key, s.Value)).ToList();
        }
    }
}
