﻿using BigTyre;
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

        return responseObject?.tags ?? new();
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

    private Uri ToAbsoluteUrl(string relativeUrl)
    {
        return new Uri(Url.Combine(Settings.Uri.ToString(), "/v2/", relativeUrl));
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
