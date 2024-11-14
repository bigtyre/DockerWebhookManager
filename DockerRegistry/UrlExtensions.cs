namespace DockerRegistryUI
{
    public static class UrlExtensions
    {
        public static string WithoutScheme(this Uri uri)
        {
            var urlWithoutScheme = uri.Host + uri.PathAndQuery + uri.Fragment;
            return urlWithoutScheme;
        }
    }
}
