using System.Security.Cryptography;
using System.Text;
using CosmicWiki.Newsletter.Application.Abstractions;

namespace CosmicWiki.Newsletter.Infrastructure.Delivery;

public sealed class ConfirmationTokenService : IConfirmationTokenService
{
    public (string RawToken, string Hash) Create()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var raw = Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
        return (raw, Hash(raw));
    }

    public string Hash(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
