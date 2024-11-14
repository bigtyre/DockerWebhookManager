using System.Security.Cryptography;
using System.Text;

namespace DockerRegistryUI.Data;

public static class HashHelper
{
    public static string ComputeSha256Hash(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var bytes = Encoding.UTF8.GetBytes(text);
        var hashBytes = SHA256.HashData(bytes);

        var hashStringBuilder = new StringBuilder(64);
        foreach (var b in hashBytes)
            hashStringBuilder.Append(b.ToString("x2"));  // Convert to hex

        return hashStringBuilder.ToString();
    }
}