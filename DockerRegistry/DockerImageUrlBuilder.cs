using DockerRegistry.Configuration;
using Flurl;

namespace DockerRegistryUI.Data;

public class DockerImageUrlBuilder(RegistrySettings settings)
{
    public Uri GetImageUrl(string repositoryName, string tag)
    {
        var baseUrl = settings.Uri;
        return new(Url.Combine(baseUrl.ToString(), $"{repositoryName}:{tag}"));
    }
}
