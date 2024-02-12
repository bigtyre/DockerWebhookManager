using BigTyre;
using BigTyre.Configuration;
using Dapper;
using DockerRegistryUI.BackgroundServiceStatus;
using DockerRegistryUI.Controllers;
using DockerRegistryUI.Data;
using DockerRegistryUI.Pages;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

var appSettings = new AppSettings();
var config = builder.Configuration;
config.Bind(appSettings);

var dbConnectionString = appSettings.MySqlConnectionString ?? throw new AppConfigurationException("MySQL connection string not provided");
var registrySettings = appSettings.Registry ?? throw new AppConfigurationException("Registry settings not provided");

SqlMapper.AddTypeHandler(new UriTypeHandler());
SqlMapper.AddTypeHandler(new GuidTypeHandler());

// Add services to the container.
var services = builder.Services;
services.AddRazorPages();
services.AddServerSideBlazor();
services.AddSingleton(registrySettings);
services.AddTransient(s => new WebhookRepository(dbConnectionString));
services.AddTransient<WebhooksService>();
services.AddSingleton<WebhooksQueue>();
services.AddTransient<RegistryService>();
services.AddSingleton<EventSystem>();

services.AddSingleton<IBackgroundServiceStatusTracker, BackgroundServiceStatusTracker>();

services.AddHostedService<WebhooksBackgroundService>();

services.AddHealthChecks().AddCheck<BackgroundServiceHealthCheck>("background_service_health_check");


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UsePathBase("/docker/registry");

app.UseStaticFiles();

app.UseRouting();

app.MapHealthChecks("/health");

app.MapBlazorHub();
app.MapControllers();
app.MapFallbackToPage("/_Host");

app.Run();
