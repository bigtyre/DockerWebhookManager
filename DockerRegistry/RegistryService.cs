using BigTyre;
using DockerRegistry.Configuration;
using Flurl;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace DockerRegistryUI.Data;

public class RegistryService(RegistrySettings settings)
{
    private HttpClient CreateHttpClient()
    {
        var baseUrl = settings.Uri;
        var baseAddress = new Uri(baseUrl, "/v2/");

        var client = new HttpClient
        {
            BaseAddress = baseAddress
        };

        var username = settings.Username;
        if (!string.IsNullOrWhiteSpace(username))
        {
            var password = settings.Password;

            string base64Credentials = Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes($"{username}:{password}")
            );

            // Set the Authorization header with Basic Authentication
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
        }

        return client;
    }

    public async Task<List<string>> GetTagsAsync(string repositoryName)
    {
        var httpClient = CreateHttpClient();

        HttpResponseMessage response = await httpClient.GetAsync($"{repositoryName}/tags/list");

        if (response.IsSuccessStatusCode is false)
        {
            var responseCode = response.StatusCode;
            throw new HttpRequestException($"Failed to get tags from catalog. Response code was {responseCode}.");
        }

        string textContent = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeAnonymousType(textContent, new { tags = new List<string>() });

        return responseObject?.tags ?? [];
    }

    public Uri GetImageUrl(string repositoryName, string tag)
    {
        var baseUrl = settings.Uri;
        return new(Url.Combine(baseUrl.ToString(), $"{repositoryName}:{tag}"));
    }

    public async Task DeleteTagAsync(string repositoryName, string tag)
    {
        var httpClient = CreateHttpClient();
        HttpResponseMessage response = await httpClient.DeleteAsync($"{repositoryName}/manifests/{tag}");
        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException($"Failed to delete tag. API returned status code {response.StatusCode}.");
        }
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
        string textContent = await response.Content.ReadAsStringAsync();

        var responseObject = JsonConvert.DeserializeAnonymousType(
            textContent,
            new
            {
                Repositories = new List<string>()
            }
        ) ?? throw new Exception("Failed to deserialize respone.");

        var repositories = responseObject.Repositories.Select(s => new DockerRepository(s)).ToList();

        return repositories;
    }
}
