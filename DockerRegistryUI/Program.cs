using BigTyre;
using BigTyre.Configuration;
using Dapper;
using DockerRegistry.Configuration;
using DockerRegistryUI.BackgroundServiceStatus;
using DockerRegistryUI.Controllers;
using DockerRegistryUI.Data;
using DockerRegistryUI.Pages;
using TrivyAPIClient;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddKeyPerFile("/var/run/secrets", optional: true);

var appSettings = new AppSettings();
var config = builder.Configuration;
config.Bind(appSettings);

var dbConnectionString = appSettings.MySqlConnectionString ?? throw new AppConfigurationException("MySQL connection string not provided");
var registrySettings = appSettings.Registry ?? throw new AppConfigurationException("Registry settings not provided");

var registryUri = registrySettings.Uri ?? throw new AppConfigurationException("Registry URI not configured.");
var registryUsername = registrySettings.Username ?? throw new AppConfigurationException("Registry username not configured.");
var registryPassword = registrySettings.Password ?? throw new AppConfigurationException("Registry password not configured.");

var trivyApiUrl = appSettings.TrivyApiUrl ?? throw new AppConfigurationException("Trivy API URL not configured.");

SqlMapper.AddTypeHandler(new UriTypeHandler());
SqlMapper.AddTypeHandler(new GuidTypeHandler());

// Add services to the container.
var services = builder.Services;
services.AddRazorPages();
services.AddServerSideBlazor();
services.AddSingleton(new RegistrySettings(registryUri, registryUsername, registryPassword));
services.AddTransient(s => new WebhookRepository(dbConnectionString));
services.AddTransient<WebhooksService>();
services.AddSingleton<WebhooksQueue>();
services.AddTransient<VulnerabilityScanner>();
services.AddTransient<TrivyApiClient>();
services.AddSingleton(svc => new TrivyApiClientSettings(trivyApiUrl.ToString()));

services.AddTransient<RegistryService>();
services.AddSingleton<EventSystem>();

services.AddSingleton<IBackgroundServiceStatusTracker, BackgroundServiceStatusTracker>();

services.AddHostedService<WebhooksBackgroundService>();

services.AddHealthChecks().AddCheck<BackgroundServiceHealthCheck>("background_service_health_check");

// Configure the HTTP request pipeline.
string basePath = appSettings.BasePath ?? "/docker-registry/";
if (basePath.StartsWith('/') is false)
    basePath = $"/{basePath}";

if (basePath.EndsWith('/') is false)
    basePath = $"{basePath}/";

services.AddSingleton(svc => new DockerRegistryUI.Settings(basePath));
services.AddHttpClient();

var app = builder.Build();


app.UsePathBase(basePath);

app.UseStaticFiles();

app.UseRouting();

// Map health checks to a specific endpoint
app.MapHealthChecks("/health");

app.MapBlazorHub();
app.MapControllers();
app.MapFallbackToPage("/_Host");

app.Run();

namespace DockerRegistryUI
{

    public class Settings(string basePath)
    {
        public string BasePath { get; } = basePath;
    }
}