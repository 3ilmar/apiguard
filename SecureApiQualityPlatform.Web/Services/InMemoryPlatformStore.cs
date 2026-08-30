using SecureApiQualityPlatform.Web.Models;

namespace SecureApiQualityPlatform.Web.Services;

public sealed class InMemoryPlatformStore : IPlatformStore
{
    private readonly object _gate = new();
    private readonly List<RegisteredApi> _apis = new();
    private readonly List<ApiCheckResult> _results = new();
    private readonly List<Defect> _defects = new();

    public IReadOnlyList<RegisteredApi> GetApis()
    {
        lock (_gate) return _apis.ToList();
    }

    public RegisteredApi? GetApi(Guid id)
    {
        lock (_gate) return _apis.SingleOrDefault(a => a.Id == id);
    }

    public void AddApi(RegisteredApi api)
    {
        lock (_gate) _apis.Add(api);
    }

    public bool TryAddApi(RegisteredApi api)
    {
        lock (_gate)
        {
            var normalizedBaseUrl = api.BaseUrl.TrimEnd('/');

            var duplicateExists = _apis.Any(existingApi =>
                existingApi.BaseUrl.TrimEnd('/').Equals(
                    normalizedBaseUrl,
                    StringComparison.OrdinalIgnoreCase));

            if (duplicateExists)
            {
                return false;
            }

            _apis.Add(api);
            return true;
        }
    }

    public void AddEndpoint(Guid apiId, ApiEndpoint endpoint)
    {
        lock (_gate)
        {
            var api = _apis.Single(a => a.Id == apiId);
            api.Endpoints.Add(endpoint);
        }
    }

    public bool TryAddEndpoint(Guid apiId, ApiEndpoint endpoint)
    {
        lock (_gate)
        {
            var api = _apis.Single(a => a.Id == apiId);

            var duplicateExists = api.Endpoints.Any(existingEndpoint =>
                existingEndpoint.Method.Equals(
                    endpoint.Method,
                    StringComparison.OrdinalIgnoreCase) &&
                existingEndpoint.Path.TrimEnd('/').Equals(
                    endpoint.Path.TrimEnd('/'),
                    StringComparison.OrdinalIgnoreCase));

            if (duplicateExists)
            {
                return false;
            }

            api.Endpoints.Add(endpoint);
            return true;
        }
    }

    public IReadOnlyList<ApiCheckResult> GetResults()
    {
        lock (_gate) return _results.OrderByDescending(r => r.ExecutedAtUtc).ToList();
    }

    public IReadOnlyList<ApiCheckResult> GetResultsForApi(Guid apiId)
    {
        lock (_gate) return _results.Where(r => r.ApiId == apiId).OrderByDescending(r => r.ExecutedAtUtc).ToList();
    }

    public void AddResult(ApiCheckResult result)
    {
        lock (_gate) _results.Add(result);
    }

    public IReadOnlyList<Defect> GetDefects()
    {
        lock (_gate) return _defects.OrderByDescending(d => d.UpdatedAtUtc).ToList();
    }

    public Defect? GetDefect(Guid id)
    {
        lock (_gate) return _defects.SingleOrDefault(d => d.Id == id);
    }

    public Defect? FindOpenDefect(Guid endpointId, string checkType)
    {
        lock (_gate)
        {
            return _defects.FirstOrDefault(d =>
                d.EndpointId == endpointId &&
                d.CheckType.Equals(checkType, StringComparison.OrdinalIgnoreCase) &&
                d.Status != DefectStatus.Closed &&
                d.Status != DefectStatus.Deferred);
        }
    }

    public void AddDefect(Defect defect)
    {
        lock (_gate) _defects.Add(defect);
    }

    public void UpdateDefect(Defect defect)
    {
        lock (_gate)
        {
            defect.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }
    }
}
