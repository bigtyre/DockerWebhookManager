using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;

namespace TrivyAPIClient
{
    public record TrivyApiClientSettings(string BaseUrl);

    public class TrivyApiClient(TrivyApiClientSettings settings, IHttpClientFactory httpClientFactory)
    {
        private readonly JsonSerializerOptions serializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly string _baseUrl = settings.BaseUrl;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

        public async Task<TrivyScanResult> GetScanResultAsync(string imageName, string username, string password)
        {
            if (string.IsNullOrEmpty(imageName))
                throw new ArgumentException("Image name cannot be null or empty", nameof(imageName));

            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));

            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new(_baseUrl);

            var uri = new Uri(imageName);
            var urlWithoutScheme = uri.Host + uri.PathAndQuery + uri.Fragment;

            var scanRequest = new
            {
                Image = urlWithoutScheme,
                Username = username,
                Password = password
            };

            // Set Basic Auth
            //var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            var json = JsonSerializer.Serialize(scanRequest);            // Serialize object to JSON
            using var requestBody = new StringContent(json, Encoding.UTF8, "application/json");


            // Send the request
            using var response = await httpClient.PostAsync("/scan", requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to retrieve scan result. Status Code: {response.StatusCode}. Details: {errorContent}");
            }

            // Deserialize JSON response
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TrivyScanResult>(responseContent, serializerOptions) ?? throw new SerializationException("Failed to interpret response as Trivy scan result.");
        }
    }
}
