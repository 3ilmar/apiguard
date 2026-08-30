using System.Net;

namespace SecureApiQualityPlatform.Web.Services;

public sealed class UrlSafetyService
{
    private readonly bool _allowPrivate;

    public UrlSafetyService(IConfiguration configuration)
    {
        _allowPrivate = configuration.GetValue<bool>("Safety:AllowPrivateNetworkTargets");
    }

    public async Task<(bool IsSafe, string Reason)> ValidateAsync(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return (false, "Only absolute http:// or https:// URLs are allowed.");
        }

        if (_allowPrivate) return (true, "Private-network targets are enabled by configuration.");

        if (uri.IsLoopback || uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            return (false, "Loopback and localhost targets are blocked by default to reduce SSRF risk.");

        try
        {
            var addresses = await Dns.GetHostAddressesAsync(uri.Host);
            if (addresses.Length == 0)
                return (false, "The host could not be resolved.");

            if (addresses.Any(IsPrivateOrLocal))
                return (false, "Private, loopback, link-local, or unspecified network targets are blocked by default.");
        }
        catch (Exception ex) when (ex is System.Net.Sockets.SocketException or ArgumentException)
        {
            return (false, "The host could not be resolved safely.");
        }

        return (true, "Target is a public HTTP(S) URL.");
    }

    private static bool IsPrivateOrLocal(IPAddress ip)
    {
        if (IPAddress.IsLoopback(ip) || ip.Equals(IPAddress.Any) || ip.Equals(IPAddress.IPv6Any))
            return true;

        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            var b = ip.GetAddressBytes();
            return b[0] == 10 ||
                   (b[0] == 172 && b[1] >= 16 && b[1] <= 31) ||
                   (b[0] == 192 && b[1] == 168) ||
                   (b[0] == 169 && b[1] == 254) ||
                   b[0] == 127;
        }

        return ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal || ip.IsIPv6Multicast;
    }
}
