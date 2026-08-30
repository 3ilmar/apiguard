using System.Net;
using SecureApiQualityPlatform.Web.Models;

namespace SecureApiQualityPlatform.Web.Services;

public sealed class CheckEvaluator
{
    public ApiCheckResult ExpectedStatus(Guid apiId, ApiEndpoint endpoint, HttpStatusCode actual)
    {
        var passed = (int)actual == endpoint.ExpectedStatusCode;
        return Result(apiId, endpoint, "Expected status", passed,
            passed
                ? $"Returned expected HTTP {(int)actual}."
                : $"Expected HTTP {endpoint.ExpectedStatusCode} but received {(int)actual}.",
            (int)actual, null, DefectSeverity.High);
    }

    public ApiCheckResult ResponseTime(Guid apiId, ApiEndpoint endpoint, long elapsedMs)
    {
        var passed = elapsedMs <= endpoint.MaxResponseTimeMs;
        return Result(apiId, endpoint, "Response time", passed,
            passed
                ? $"Response completed in {elapsedMs} ms (threshold {endpoint.MaxResponseTimeMs} ms)."
                : $"Response took {elapsedMs} ms, exceeding the {endpoint.MaxResponseTimeMs} ms threshold.",
            null, elapsedMs, DefectSeverity.Medium);
    }

    public ApiCheckResult AuthenticationProtection(Guid apiId, ApiEndpoint endpoint, HttpStatusCode actual)
    {
        var protectedStatus = actual is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden;
        var passed = !endpoint.RequiresAuthentication || protectedStatus;
        var message = endpoint.RequiresAuthentication
            ? (passed
                ? $"Unauthenticated request was blocked with HTTP {(int)actual}."
                : $"Endpoint is marked authentication-required but unauthenticated request returned HTTP {(int)actual}.")
            : "Endpoint is not marked as authentication-required; authentication check is informational.";

        return Result(apiId, endpoint, "Authentication protection", passed, message,
            (int)actual, null, DefectSeverity.Critical);
    }

    public ApiCheckResult ContentType(Guid apiId, ApiEndpoint endpoint, string? actualContentType)
    {
        if (string.IsNullOrWhiteSpace(endpoint.ExpectedContentType))
        {
            return Result(apiId, endpoint, "Content type", true,
                "No expected content type was configured for this endpoint.");
        }

        var passed = actualContentType?.Contains(endpoint.ExpectedContentType, StringComparison.OrdinalIgnoreCase) == true;
        return Result(apiId, endpoint, "Content type", passed,
            passed
                ? $"Content type '{actualContentType}' matched the expected value."
                : $"Expected content type containing '{endpoint.ExpectedContentType}' but received '{actualContentType ?? "none"}'.",
            failureSeverity: DefectSeverity.Medium);
    }

    public ApiCheckResult Cors(Guid apiId, ApiEndpoint endpoint, IEnumerable<string> allowOrigins)
    {
        var wildcard = allowOrigins.Any(v => v.Trim() == "*");
        var passed = !(endpoint.RequiresAuthentication && wildcard);
        return Result(apiId, endpoint, "CORS policy", passed,
            passed
                ? "No unsafe wildcard CORS policy was observed for an authentication-required endpoint."
                : "Authentication-required endpoint returned Access-Control-Allow-Origin: *, which requires security review.",
            failureSeverity: DefectSeverity.High);
    }

    public ApiCheckResult ServerDisclosure(Guid apiId, ApiEndpoint endpoint, IEnumerable<string> serverHeaders)
    {
        var values = serverHeaders.Where(v => !string.IsNullOrWhiteSpace(v)).ToArray();
        var passed = values.Length == 0;
        return Result(apiId, endpoint, "Server disclosure", passed,
            passed
                ? "No Server header disclosure was observed."
                : $"Server header disclosed: {string.Join(", ", values)}.",
            failureSeverity: DefectSeverity.Low);
    }

    public ApiCheckResult Https(Guid apiId, ApiEndpoint endpoint, Uri target)
    {
        var passed = target.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
        return Result(apiId, endpoint, "HTTPS transport", passed,
            passed ? "Endpoint uses HTTPS." : "Endpoint uses HTTP rather than HTTPS.",
            failureSeverity: DefectSeverity.High);
    }

    public ApiCheckResult AvailabilityFailure(Guid apiId, ApiEndpoint endpoint, string reason) =>
        Result(apiId, endpoint, "Availability", false, $"Request failed: {reason}", failureSeverity: DefectSeverity.High);

    public ApiCheckResult AvailabilitySuccess(Guid apiId, ApiEndpoint endpoint, int statusCode, long elapsedMs) =>
        Result(apiId, endpoint, "Availability", true, $"Endpoint responded with HTTP {statusCode} in {elapsedMs} ms.",
            statusCode, elapsedMs, DefectSeverity.High);

    private static ApiCheckResult Result(
        Guid apiId,
        ApiEndpoint endpoint,
        string type,
        bool passed,
        string message,
        int? status = null,
        long? elapsedMs = null,
        DefectSeverity failureSeverity = DefectSeverity.Medium) => new()
        {
            ApiId = apiId,
            EndpointId = endpoint.Id,
            CheckType = type,
            Passed = passed,
            Message = message,
            ActualStatusCode = status,
            ResponseTimeMs = elapsedMs,
            FailureSeverity = failureSeverity
        };
}
