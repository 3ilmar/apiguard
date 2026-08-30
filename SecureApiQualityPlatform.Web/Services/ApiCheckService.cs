using System.Diagnostics;
using SecureApiQualityPlatform.Web.Models;

namespace SecureApiQualityPlatform.Web.Services;

public sealed class ApiCheckService
{
    private readonly HttpClient _httpClient;
    private readonly IPlatformStore _store;
    private readonly CheckEvaluator _evaluator;
    private readonly DefectLifecycleService _defects;
    private readonly UrlSafetyService _urlSafety;

    public ApiCheckService(
        HttpClient httpClient,
        IPlatformStore store,
        CheckEvaluator evaluator,
        DefectLifecycleService defects,
        UrlSafetyService urlSafety)
    {
        _httpClient = httpClient;
        _store = store;
        _evaluator = evaluator;
        _defects = defects;
        _urlSafety = urlSafety;
    }

    public async Task<IReadOnlyList<ApiCheckResult>> RunEndpointAsync(RegisteredApi api, ApiEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        var target = BuildTargetUri(api.BaseUrl, endpoint.Path);
        var safety = await _urlSafety.ValidateAsync(target.ToString());
        if (!safety.IsSafe)
        {
            var blocked = _evaluator.AvailabilityFailure(api.Id, endpoint, $"Safety policy blocked the request. {safety.Reason}");
            Save(blocked);
            return new[] { blocked };
        }

        var results = new List<ApiCheckResult> { _evaluator.Https(api.Id, endpoint, target) };

        try
        {
            using var request = new HttpRequestMessage(new HttpMethod(endpoint.Method.ToUpperInvariant()), target);
            var stopwatch = Stopwatch.StartNew();
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            stopwatch.Stop();

            results.Add(_evaluator.AvailabilitySuccess(api.Id, endpoint, (int)response.StatusCode, stopwatch.ElapsedMilliseconds));
            results.Add(_evaluator.ExpectedStatus(api.Id, endpoint, response.StatusCode));
            results.Add(_evaluator.ResponseTime(api.Id, endpoint, stopwatch.ElapsedMilliseconds));
            if (endpoint.RequiresAuthentication)
            {
                results.Add(_evaluator.AuthenticationProtection(api.Id, endpoint, response.StatusCode));
                var cors = response.Headers.TryGetValues("Access-Control-Allow-Origin", out var origins)
                    ? origins
                    : Array.Empty<string>();
                results.Add(_evaluator.Cors(api.Id, endpoint, cors));
            }

            if (!string.IsNullOrWhiteSpace(endpoint.ExpectedContentType))
            {
                results.Add(_evaluator.ContentType(api.Id, endpoint, response.Content.Headers.ContentType?.MediaType));
            }

            var server = response.Headers.TryGetValues("Server", out var servers)
                ? servers
                : Array.Empty<string>();
            results.Add(_evaluator.ServerDisclosure(api.Id, endpoint, server));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            results.Add(_evaluator.AvailabilityFailure(api.Id, endpoint, "Request timed out."));
        }
        catch (HttpRequestException ex)
        {
            results.Add(_evaluator.AvailabilityFailure(api.Id, endpoint, ex.Message));
        }

        foreach (var result in results) Save(result);
        return results;
    }

    public async Task<IReadOnlyList<ApiCheckResult>> RunApiAsync(Guid apiId, CancellationToken cancellationToken = default)
    {
        var api = _store.GetApi(apiId) ?? throw new KeyNotFoundException("API was not found.");
        var combined = new List<ApiCheckResult>();
        foreach (var endpoint in api.Endpoints)
        {
            combined.AddRange(await RunEndpointAsync(api, endpoint, cancellationToken));
        }
        return combined;
    }

    private void Save(ApiCheckResult result)
    {
        _store.AddResult(result);
        _defects.ApplyCheckResult(result);
    }

    private static Uri BuildTargetUri(string baseUrl, string path)
    {
        var baseUri = new Uri(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/");
        return new Uri(baseUri, path.TrimStart('/'));
    }
}
