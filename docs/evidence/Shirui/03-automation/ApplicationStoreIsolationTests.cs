using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureApiQualityPlatform.Web.Data;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Tests;

[TestClass]
public class ApplicationStoreIsolationTests
{
    [TestMethod]
    [DataRow("Development")]
    [DataRow("Testing")]
    public async Task InMemoryOverride_CreatesSeparateEmptyStores_WithoutOpeningConfiguredDatabase(string environment)
    {
        var directory = NewTemporaryDirectory();
        var blockedParent = Path.Combine(directory, "not-a-directory");
        const string sentinel = "The configured database parent is deliberately a file.";
        File.WriteAllText(blockedParent, sentinel);

        try
        {
            // SQLite cannot open a database below a file. Successful construction
            // therefore proves the normal persistence initializer was bypassed.
            var databasePath = Path.Combine(blockedParent, "must-not-open.db");
            var args = Arguments(environment, databasePath);
            await using var first = Program.CreateApp(args, UseInMemoryStore);
            await using var second = Program.CreateApp(args, UseInMemoryStore);
            var firstStore = first.Services.GetRequiredService<IPlatformStore>();
            var secondStore = second.Services.GetRequiredService<IPlatformStore>();

            Assert.IsInstanceOfType<InMemoryPlatformStore>(firstStore);
            Assert.IsInstanceOfType<InMemoryPlatformStore>(secondStore);
            Assert.AreNotSame(firstStore, secondStore);
            AssertEmpty(firstStore);
            AssertEmpty(secondStore);

            firstStore.AddApi(new RegisteredApi { Name = "Isolated fixture", BaseUrl = "https://fixture.invalid" });
            Assert.AreEqual(1, firstStore.GetApis().Count);
            AssertEmpty(secondStore);
            Assert.AreEqual(sentinel, File.ReadAllText(blockedParent));
            Assert.AreEqual(1, Directory.GetFileSystemEntries(directory).Length);
        }
        finally
        {
            File.Delete(blockedParent);
            Directory.Delete(directory);
        }
    }

    [TestMethod]
    public async Task NormalFactory_KeepsSqliteStore_AndInitializesExplicitDatabase()
    {
        var directory = NewTemporaryDirectory();
        var databasePath = Path.Combine(directory, "normal-startup.db");
        try
        {
            // Supply an explicit disposable database, never the user's normal path.
            await using var app = Program.CreateApp(Arguments("Testing", databasePath));
            var store = app.Services.GetRequiredService<IPlatformStore>();
            Assert.IsInstanceOfType<SqlitePlatformStore>(store);
            Assert.IsTrue(File.Exists(databasePath));
            AssertEmpty(store);

            var api = new RegisteredApi { Name = "Persistence check", BaseUrl = "https://persistence.invalid" };
            store.AddApi(api);
            using var db = app.Services.GetRequiredService<IDbContextFactory<PlatformDbContext>>().CreateDbContext();
            Assert.IsTrue(db.Database.CanConnect());
            Assert.AreEqual(api.Id, db.Apis.AsNoTracking().Single().Id);
        }
        finally
        {
            foreach (var suffix in new[] { "", "-wal", "-shm", "-journal" })
                File.Delete(databasePath + suffix);
            Directory.Delete(directory);
        }
    }

    [TestMethod]
    public void ServiceOverride_InProduction_IsRejectedBeforeDatabaseInitialization()
    {
        var directory = NewTemporaryDirectory();
        var databasePath = Path.Combine(directory, "must-not-create.db");
        try
        {
            var error = Assert.ThrowsException<InvalidOperationException>(() =>
                Program.CreateApp(Arguments("Production", databasePath), UseInMemoryStore));
            StringAssert.Contains(error.Message, "Development or Testing");
            Assert.AreEqual(0, Directory.GetFileSystemEntries(directory).Length);
        }
        finally
        {
            Directory.Delete(directory);
        }
    }

    private static void UseInMemoryStore(IServiceCollection services)
    {
        services.RemoveAll<IPlatformStore>();
        services.AddSingleton<IPlatformStore, InMemoryPlatformStore>();
    }

    private static void AssertEmpty(IPlatformStore store)
    {
        Assert.AreEqual(0, store.GetApis().Count);
        Assert.AreEqual(0, store.GetResults().Count);
        Assert.AreEqual(0, store.GetDefects().Count);
    }

    private static string[] Arguments(string environment, string databasePath) =>
    [
        "--environment", environment,
        "--contentRoot", FindWebRoot(),
        "--Logging:LogLevel:Default", "Warning",
        "--ConnectionStrings:PlatformDatabase", $"Data Source={databasePath};Pooling=False"
    ];

    private static string NewTemporaryDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"apiguard-store-isolation-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static string FindWebRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "SecureApiQualityPlatform.Web");
            if (File.Exists(Path.Combine(candidate, "SecureApiQualityPlatform.Web.csproj")))
                return candidate;
        }
        throw new DirectoryNotFoundException("Tests require the source Web project.");
    }
}
