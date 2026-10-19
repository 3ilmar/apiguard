using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureApiQualityPlatform.Web.Data;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;
using Microsoft.Data.Sqlite;

namespace SecureApiQualityPlatform.Tests;

[TestClass]
public class SqlitePlatformStoreIntegrationTests
{
    private string _databasePath = string.Empty;
    private TestDbContextFactory _factory = null!;

    [TestInitialize]
    public void Setup()
    {
        _databasePath = Path.Combine(
            Path.GetTempPath(),
            $"apiguard-test-{Guid.NewGuid():N}.db");

        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseSqlite($"Data Source={_databasePath};Pooling=False")
            .Options;

        _factory = new TestDbContextFactory(options);

        using var db = _factory.CreateDbContext();
        db.Database.EnsureCreated();
    }

    [TestCleanup]
    public void Cleanup()
    {
        SqliteConnection.ClearAllPools();

        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }

        var shmPath = _databasePath + "-shm";
        var walPath = _databasePath + "-wal";

        if (File.Exists(shmPath))
        {
            File.Delete(shmPath);
        }

        if (File.Exists(walPath))
        {
            File.Delete(walPath);
        }
    }

    [TestMethod]
    public void ApiAndEndpoint_PersistAcrossNewStoreInstance()
    {
        var firstStore = new SqlitePlatformStore(_factory);

        var api = new RegisteredApi
        {
            Name = "Persistence API",
            BaseUrl = "https://example.com"
        };

        Assert.IsTrue(firstStore.TryAddApi(api));

        var endpoint = new ApiEndpoint
        {
            ApiId = api.Id,
            Name = "Get Users",
            Path = "/users",
            Method = "GET",
            ExpectedStatusCode = 200,
            MaxResponseTimeMs = 1500
        };

        Assert.IsTrue(firstStore.TryAddEndpoint(api.Id, endpoint));

        var secondStore = new SqlitePlatformStore(_factory);

        var reloadedApi = secondStore.GetApi(api.Id);

        Assert.IsNotNull(reloadedApi);
        Assert.AreEqual("Persistence API", reloadedApi.Name);
        Assert.AreEqual(1, reloadedApi.Endpoints.Count);
        Assert.AreEqual("/users", reloadedApi.Endpoints[0].Path);
        Assert.AreEqual("GET", reloadedApi.Endpoints[0].Method);
    }

    [TestMethod]
    public void CheckResult_PersistsAcrossNewStoreInstance()
    {
        var firstStore = new SqlitePlatformStore(_factory);

        var api = new RegisteredApi
        {
            Name = "Results API",
            BaseUrl = "https://example.com"
        };

        Assert.IsTrue(firstStore.TryAddApi(api));

        var endpoint = new ApiEndpoint
        {
            ApiId = api.Id,
            Name = "Health",
            Path = "/health",
            Method = "GET"
        };

        Assert.IsTrue(firstStore.TryAddEndpoint(api.Id, endpoint));

        var result = new ApiCheckResult
        {
            ApiId = api.Id,
            EndpointId = endpoint.Id,
            CheckType = "Availability",
            Passed = true,
            Message = "Endpoint responded successfully.",
            ActualStatusCode = 200,
            ResponseTimeMs = 120
        };

        firstStore.AddResult(result);

        var secondStore = new SqlitePlatformStore(_factory);

        var results = secondStore.GetResultsForApi(api.Id);

        Assert.AreEqual(1, results.Count);
        Assert.AreEqual(result.Id, results[0].Id);
        Assert.AreEqual("Availability", results[0].CheckType);
        Assert.IsTrue(results[0].Passed);
    }

    [TestMethod]
    public void Defect_PersistsAcrossNewStoreInstance()
    {
        var firstStore = new SqlitePlatformStore(_factory);

        var api = new RegisteredApi
        {
            Name = "Defect API",
            BaseUrl = "https://example.com"
        };

        Assert.IsTrue(firstStore.TryAddApi(api));

        var endpoint = new ApiEndpoint
        {
            ApiId = api.Id,
            Name = "Users",
            Path = "/users",
            Method = "GET"
        };

        Assert.IsTrue(firstStore.TryAddEndpoint(api.Id, endpoint));

        var defect = new Defect
        {
            ApiId = api.Id,
            EndpointId = endpoint.Id,
            Title = "Server disclosure check failed",
            Description = "Server header was disclosed.",
            CheckType = "Server disclosure"
        };

        firstStore.AddDefect(defect);

        var secondStore = new SqlitePlatformStore(_factory);

        var reloadedDefect = secondStore.GetDefect(defect.Id);

        Assert.IsNotNull(reloadedDefect);
        Assert.AreEqual(defect.Id, reloadedDefect.Id);
        Assert.AreEqual(
            "Server disclosure check failed",
            reloadedDefect.Title);
        Assert.AreEqual(endpoint.Id, reloadedDefect.EndpointId);
    }

    [TestMethod]
    public void TryAddEndpoint_SamePathDifferentCase_ReturnsTrue()
    {
        var store = new SqlitePlatformStore(_factory);

        var api = new RegisteredApi
        {
            Name = "Case Sensitive API",
            BaseUrl = "https://example.com"
        };

        Assert.IsTrue(store.TryAddApi(api));

        var lowerCaseEndpoint = new ApiEndpoint
        {
            ApiId = api.Id,
            Name = "Lowercase Users",
            Path = "/users",
            Method = "GET"
        };

        var upperCaseEndpoint = new ApiEndpoint
        {
            ApiId = api.Id,
            Name = "Uppercase Users",
            Path = "/Users",
            Method = "GET"
        };

        Assert.IsTrue(
            store.TryAddEndpoint(api.Id, lowerCaseEndpoint));

        Assert.IsTrue(
            store.TryAddEndpoint(api.Id, upperCaseEndpoint));

        var savedApi = store.GetApi(api.Id);

        Assert.IsNotNull(savedApi);
        Assert.AreEqual(2, savedApi.Endpoints.Count);
    }

    [TestMethod]
    public void TryAddEndpoint_SameMethodDifferentCase_ReturnsFalse()
    {
        var store = new SqlitePlatformStore(_factory);

        var api = new RegisteredApi
        {
            Name = "Method Normalization API",
            BaseUrl = "https://example.com"
        };

        Assert.IsTrue(store.TryAddApi(api));

        var firstEndpoint = new ApiEndpoint
        {
            ApiId = api.Id,
            Name = "First",
            Path = "/users",
            Method = "GET"
        };

        var duplicateEndpoint = new ApiEndpoint
        {
            ApiId = api.Id,
            Name = "Duplicate",
            Path = "/users/",
            Method = "get"
        };

        Assert.IsTrue(
            store.TryAddEndpoint(api.Id, firstEndpoint));

        Assert.IsFalse(
            store.TryAddEndpoint(api.Id, duplicateEndpoint));

        var savedApi = store.GetApi(api.Id);

        Assert.IsNotNull(savedApi);
        Assert.AreEqual(1, savedApi.Endpoints.Count);
        Assert.AreEqual("GET", savedApi.Endpoints[0].Method);
        Assert.AreEqual("/users", savedApi.Endpoints[0].Path);
    }

    private sealed class TestDbContextFactory
        : IDbContextFactory<PlatformDbContext>
    {
        private readonly DbContextOptions<PlatformDbContext> _options;

        public TestDbContextFactory(
            DbContextOptions<PlatformDbContext> options)
        {
            _options = options;
        }

        public PlatformDbContext CreateDbContext()
        {
            return new PlatformDbContext(_options);
        }
    }
}