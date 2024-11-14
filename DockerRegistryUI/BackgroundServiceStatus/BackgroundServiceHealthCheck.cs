using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DockerRegistryUI.BackgroundServiceStatus;

public class BackgroundServiceHealthCheck(
    ILogger<BackgroundServiceHealthCheck> logger, 
    IBackgroundServiceStatusTracker statusTracker
) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var allServices = statusTracker.GetServices();

        var unhealthyServices = allServices.Where(service => service.Status == ServiceStatus.Stopped);

        if (unhealthyServices.Any())
        {
            logger.LogDebug("{serviceName} - Reporting unhealthy.", nameof(BackgroundServiceHealthCheck));
            return Task.FromResult(HealthCheckResult.Unhealthy($"The following services are stopped: {string.Join(", ", unhealthyServices)}"));
        }

        return Task.FromResult(HealthCheckResult.Healthy("All background services are running."));
    }
}
