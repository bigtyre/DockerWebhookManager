namespace DockerRegistryUI.BackgroundServiceStatus
{
    public interface IBackgroundServiceStatusTracker
    {
        void UpdateStatus(string serviceName, ServiceStatus status);
        ServiceStatus GetServiceStatus(string serviceName);
        List<ServiceStatusSummary> GetServices();
    }
}
