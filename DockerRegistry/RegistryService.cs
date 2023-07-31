using DockerRegistry.Configuration;
using Flurl;
using Newtonsoft.Json;

namespace DockerRegistryUI.Data;

public class RegistryService
{
    public RegistryService(RegistrySettings settings)
    {
        Settings = settings;
    }

    public RegistrySettings Settings { get; }

    private HttpClient CreateHttpClient()
    {
        var baseAddress = new Uri(Settings.Uri, "/v2/");

        var client = new HttpClient
        {
            BaseAddress = baseAddress
        };

        return client;
    }

    public async Task<List<DockerRepository>> GetRepositoriesAsync()
    {
        var httpClient = CreateHttpClient();

        HttpResponseMessage response = await httpClient.GetAsync("_catalog");
        
        if (response.IsSuccessStatusCode is false)
        {
            var responseCode = response.StatusCode;
            throw new HttpRequestException($"Failed to get repositories from catalog. Response code was {responseCode}.");
        }

        // Read the response content
        string jsonContent = await response.Content.ReadAsStringAsync();

        var responseObject = JsonConvert.DeserializeAnonymousType(
            jsonContent,
            new
            {
                Repositories = new List<string>()
            }
        ) ?? throw new Exception("Failed to deserialize respone.");

        var repositories = responseObject.Repositories.Select(s => new DockerRepository(s)).ToList();

        return repositories;
    }
}
