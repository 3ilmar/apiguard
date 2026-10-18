using Microsoft.EntityFrameworkCore;
using SecureApiQualityPlatform.Web.Data;
using SecureApiQualityPlatform.Web.Models;

namespace SecureApiQualityPlatform.Web.Services;

public sealed class SqlitePlatformStore : IPlatformStore
{
    private readonly IDbContextFactory<PlatformDbContext> _dbFactory;
    private readonly object _gate = new();

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

            api.BaseUrl = NormalizeBaseUrl(api.BaseUrl);

            db.Apis.Add(api);
            db.SaveChanges();
        }
    }

    public bool TryAddApi(RegisteredApi api)
    {
        lock (_gate)
        {
            using var db = _dbFactory.CreateDbContext();

            var normalizedBaseUrl = NormalizeBaseUrl(api.BaseUrl);

            var existingUrls = db.Apis
                .AsNoTracking()
                .Select(existingApi => existingApi.BaseUrl)
                .ToList();

            var duplicateExists = existingUrls.Any(existingUrl =>
                NormalizeBaseUrl(existingUrl).Equals(
                    normalizedBaseUrl,
                    StringComparison.OrdinalIgnoreCase));

            if (duplicateExists)
            {
                return false;
            }

            api.BaseUrl = normalizedBaseUrl;

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

            endpoint.Method = NormalizeMethod(endpoint.Method);
            endpoint.Path = NormalizePath(endpoint.Path);

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

            var normalizedMethod = NormalizeMethod(endpoint.Method);
            var normalizedPath = NormalizePath(endpoint.Path);

            var existingEndpoints = db.Endpoints
                .AsNoTracking()
                .Where(existingEndpoint => existingEndpoint.ApiId == apiId)
                .ToList();

            var duplicateExists = existingEndpoints.Any(existingEndpoint =>
                NormalizeMethod(existingEndpoint.Method).Equals(
                    normalizedMethod,
                    StringComparison.Ordinal) &&
                NormalizePath(existingEndpoint.Path).Equals(
                    normalizedPath,
                    StringComparison.Ordinal));

            if (duplicateExists)
            {
                return false;
            }

            endpoint.Method = normalizedMethod;
            endpoint.Path = normalizedPath;

            db.Endpoints.Add(endpoint);
            db.SaveChanges();

            return true;
        }
    }

    public IReadOnlyList<ApiCheckResult> GetResults()
    {
        using var db = _dbFactory.CreateDbContext();

        return db.CheckResults
            .AsNoTracking()
            .ToList()
            .OrderByDescending(result => result.ExecutedAtUtc)
            .ToList();
    }

    public IReadOnlyList<ApiCheckResult> GetResultsForApi(Guid apiId)
    {
        using var db = _dbFactory.CreateDbContext();

        return db.CheckResults
            .AsNoTracking()
            .Where(result => result.ApiId == apiId)
            .ToList()
            .OrderByDescending(result => result.ExecutedAtUtc)
            .ToList();
    }

    public void AddResult(ApiCheckResult result)
    {
        lock (_gate)
        {
            using var db = _dbFactory.CreateDbContext();

            var apiExists = db.Apis.Any(api => api.Id == result.ApiId);

            var endpointExists = db.Endpoints.Any(endpoint =>
                endpoint.Id == result.EndpointId &&
                endpoint.ApiId == result.ApiId);

            if (!apiExists || !endpointExists)
            {
                throw new InvalidOperationException(
                    "Check result must reference an existing API and endpoint.");
            }

            db.CheckResults.Add(result);
            db.SaveChanges();
        }
    }

    public IReadOnlyList<Defect> GetDefects()
    {
        using var db = _dbFactory.CreateDbContext();

        return db.Defects
            .AsNoTracking()
            .ToList()
            .OrderByDescending(defect => defect.UpdatedAtUtc)
            .ToList();
    }

    public Defect? GetDefect(Guid id)
    {
        using var db = _dbFactory.CreateDbContext();

        return db.Defects
            .AsNoTracking()
            .SingleOrDefault(defect => defect.Id == id);
    }

    public Defect? FindOpenDefect(Guid endpointId, string checkType)
    {
        using var db = _dbFactory.CreateDbContext();

        return db.Defects
            .AsNoTracking()
            .Where(defect =>
                defect.EndpointId == endpointId &&
                defect.Status != DefectStatus.Closed &&
                defect.Status != DefectStatus.Deferred)
            .ToList()
            .FirstOrDefault(defect =>
                defect.CheckType.Equals(
                    checkType,
                    StringComparison.OrdinalIgnoreCase));
    }

    public void AddDefect(Defect defect)
    {
        lock (_gate)
        {
            using var db = _dbFactory.CreateDbContext();

            var apiExists = db.Apis.Any(api => api.Id == defect.ApiId);

            var endpointExists = db.Endpoints.Any(endpoint =>
                endpoint.Id == defect.EndpointId &&
                endpoint.ApiId == defect.ApiId);

            if (!apiExists || !endpointExists)
            {
                throw new InvalidOperationException(
                    "Defect must reference an existing API and endpoint.");
            }

            if (defect.SourceCheckResultId.HasValue)
            {
                var sourceResultExists = db.CheckResults.Any(result =>
                    result.Id == defect.SourceCheckResultId.Value);

                if (!sourceResultExists)
                {
                    throw new InvalidOperationException(
                        "Defect source check result does not exist.");
                }
            }

            db.Defects.Add(defect);
            db.SaveChanges();
        }
    }

    public void UpdateDefect(Defect defect)
    {
        lock (_gate)
        {
            using var db = _dbFactory.CreateDbContext();

            var defectExists = db.Defects.Any(existing =>
                existing.Id == defect.Id);

            if (!defectExists)
            {
                throw new InvalidOperationException(
                    "Cannot update a defect that does not exist.");
            }

            defect.UpdatedAtUtc = DateTimeOffset.UtcNow;

            db.Defects.Update(defect);
            db.SaveChanges();
        }
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        return (baseUrl ?? string.Empty)
            .Trim()
            .TrimEnd('/');
    }

    private static string NormalizeMethod(string method)
    {
        return (method ?? string.Empty)
            .Trim()
            .ToUpperInvariant();
    }

    private static string NormalizePath(string path)
    {
        var normalizedPath = (path ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(normalizedPath))
        {
            return "/";
        }

        if (!normalizedPath.StartsWith('/'))
        {
            normalizedPath = "/" + normalizedPath;
        }

        if (normalizedPath.Length > 1)
        {
            normalizedPath = normalizedPath.TrimEnd('/');
        }

        return normalizedPath;
    }
}