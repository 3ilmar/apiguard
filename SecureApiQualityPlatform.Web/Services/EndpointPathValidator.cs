namespace SecureApiQualityPlatform.Web.Services;

public static class EndpointPathValidator
{
    private const int MaxDecodeRounds = 3;

    public static bool IsSafe(string? path, out string reason)
    {
        var decoded = path?.Trim() ?? string.Empty;

        // Decode until the value stops changing, so %2e%2e and %252e%252e are both seen as "..".
        for (var round = 0; ; round++)
        {
            var next = Uri.UnescapeDataString(decoded);
            if (next == decoded) break;

            if (round >= MaxDecodeRounds)
            {
                reason = "Endpoint path uses too many levels of encoding.";
                return false;
            }

            decoded = next;
        }

        if (decoded.Contains("://", StringComparison.Ordinal) ||
            decoded.StartsWith("//", StringComparison.Ordinal))
        {
            reason = "Endpoint path must be relative to the registered API.";
            return false;
        }

        if (decoded.Contains('\\') ||
            decoded.Any(char.IsControl) ||
            decoded.Split('/', StringSplitOptions.RemoveEmptyEntries).Any(segment => segment == ".."))
        {
            reason = "Endpoint path must stay within the registered API.";
            return false;
        }

        reason = string.Empty;
        return true;
    }
}