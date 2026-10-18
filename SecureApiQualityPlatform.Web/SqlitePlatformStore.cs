using Microsoft.EntityFrameworkCore;
using SecureApiQualityPlatform.Web.Data;
using SecureApiQualityPlatform.Web.Models;

namespace SecureApiQualityPlatform.Web.Services;

public sealed class SqlitePlatformStore : IPlatformStore
{
    private readonly IDbContextFactory<PlatformDbContext> _dbFactory;
    private readonly object _gate = new();

    // Results and defects will be persisted in the next storage commit.
    private readonly List<ApiCheckResult> _results = new();
    private readonly List<Defect> _defects = new();

    public SqlitePlatformStore(IDbContextFactory<PlatformDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public IReadOnlyList<RegisteredApi> GetApis()
    {
        using var db = _dbFactory.CreateDbContext();

        return db.Apis
            .AsNoTracking()
            .Include(api => api.Endpoints)
            .ToList()
            .OrderBy(api => api.CreatedAtUtc)
            .ToList();
    }

    public RegisteredApi? GetApi(Guid id)
    {
        using var db = _dbFactory.CreateDbContext();

        return db.Apis
            .AsNoTracking()
            .Include(api => api.Endpoints)
            .SingleOrDefault(api => api.Id == id);
    }

    public void AddApi(RegisteredApi api)
    {
        lock (_gate)
        {
            using var db = _dbFactory.CreateDbContext();

            db.Apis.Add(api);
            db.SaveChanges();
        }
    }

    public bool TryAddApi(RegisteredApi api)
    {
        lock (_gate)
        {
            using var db = _dbFactory.CreateDbContext();

            var normalizedBaseUrl = api.BaseUrl.TrimEnd('/');

            var existingUrls = db.Apis
                .AsNoTracking()
                .Select(existingApi => existingApi.BaseUrl)
                .ToList();

            var duplicateExists = existingUrls.Any(existingUrl =>
                existingUrl.TrimEnd('/').Equals(
                    normalizedBaseUrl,
                    StringComparison.OrdinalIgnoreCase));

            if (duplicateExists)
            {
                return false;
            }

            db.Apis.Add(api);
            db.SaveChanges();

            return true;
        }
    }

    public void AddEndpoint(Guid apiId, ApiEndpoint endpoint)
    {
        lock (_gate)
        {
            using var db = _dbFactory.CreateDbContext();

            var apiExists = db.Apis.Any(api => api.Id == apiId);

            if (!apiExists)
            {
                throw new InvalidOperationException(
                    "Cannot add an endpoint to an API that does not exist.");
            }

            if (endpoint.ApiId != apiId)
            {
                throw new InvalidOperationException(
                    "Endpoint API ID does not match the target API.");
            }

            db.Endpoints.Add(endpoint);
            db.SaveChanges();
        }
    }

    public bool TryAddEndpoint(Guid apiId, ApiEndpoint endpoint)
    {
        lock (_gate)
        {
            using var db = _dbFactory.CreateDbContext();

            var apiExists = db.Apis.Any(api => api.Id == apiId);

            if (!apiExists)
            {
                return false;
            }

            if (endpoint.ApiId != apiId)
            {
                return false;
            }

            var existingEndpoints = db.Endpoints
                .AsNoTracking()
                .Where(existingEndpoint => existingEndpoint.ApiId == apiId)
                .ToList();

            var duplicateExists = existingEndpoints.Any(existingEndpoint =>
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

            db.Endpoints.Add(endpoint);
            db.SaveChanges();

            return true;
        }
    }

    public IReadOnlyList<ApiCheckResult> GetResults()
    {
        lock (_gate)
        {
            return _results
                .OrderByDescending(result => result.ExecutedAtUtc)
                .ToList();
        }
    }

    public IReadOnlyList<ApiCheckResult> GetResultsForApi(Guid apiId)
    {
        lock (_gate)
        {
            return _results
                .Where(result => result.ApiId == apiId)
                .OrderByDescending(result => result.ExecutedAtUtc)
                .ToList();
        }
    }

    public void AddResult(ApiCheckResult result)
    {
        lock (_gate)
        {
            _results.Add(result);
        }
    }

    public IReadOnlyList<Defect> GetDefects()
    {
        lock (_gate)
        {
            return _defects
                .OrderByDescending(defect => defect.UpdatedAtUtc)
                .ToList();
        }
    }

    public Defect? GetDefect(Guid id)
    {
        lock (_gate)
        {
            return _defects.SingleOrDefault(defect => defect.Id == id);
        }
    }

    public Defect? FindOpenDefect(Guid endpointId, string checkType)
    {
        lock (_gate)
        {
            return _defects.FirstOrDefault(defect =>
                defect.EndpointId == endpointId &&
                defect.CheckType.Equals(
                    checkType,
                    StringComparison.OrdinalIgnoreCase) &&
                defect.Status != DefectStatus.Closed &&
                defect.Status != DefectStatus.Deferred);
        }
    }

    public void AddDefect(Defect defect)
    {
        lock (_gate)
        {
            _defects.Add(defect);
        }
    }

    public void UpdateDefect(Defect defect)
    {
        lock (_gate)
        {
            defect.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }
    }
}