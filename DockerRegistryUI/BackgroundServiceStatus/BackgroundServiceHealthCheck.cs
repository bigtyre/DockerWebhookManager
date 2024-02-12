using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Hosting;

namespace DockerRegistryUI.BackgroundServiceStatus;

public class BackgroundServiceHealthCheck : IHealthCheck
{
    private readonly IBackgroundServiceStatusTracker _statusTracker;

    public BackgroundServiceHealthCheck(IBackgroundServiceStatusTracker statusTracker)
    {
        _statusTracker = statusTracker;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var allServices = _statusTracker.GetServices();

        var unhealthyServices = allServices.Where(service => service.Status == ServiceStatus.Stopped);

        if (unhealthyServices.Any())
        {
            return Task.FromResult(HealthCheckResult.Unhealthy($"The following services are stopped: {string.Join(", ", unhealthyServices)}"));
        }

        return Task.FromResult(HealthCheckResult.Healthy("All background services are running."));
    }
}
