
using Microsoft.EntityFrameworkCore;
using SecureApiQualityPlatform.Web.Data;
using SecureApiQualityPlatform.Web.Services;

var app = Program.CreateApp(args);
app.Run();

public partial class Program
{
    public static WebApplication CreateApp(
        string[] args,
        Action<IServiceCollection>? configureServices = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ApplicationName = typeof(Program).Assembly.GetName().Name
        });

        builder.Services.AddControllersWithViews();

        // SQLite persistence - preserve the persistence PR.
        builder.Services.AddDbContextFactory<PlatformDbContext>(options =>
            options.UseSqlite(
                builder.Configuration.GetConnectionString("PlatformDatabase")));

        builder.Services.AddSingleton<IPlatformStore, SqlitePlatformStore>();

        // Existing application services.
        builder.Services.AddSingleton<CheckEvaluator>();
        builder.Services.AddSingleton<DefectLifecycleService>();
        builder.Services.AddSingleton<DashboardService>();
        builder.Services.AddSingleton<UrlSafetyService>();

        // RBAC - preserve the role provider from PR #4.
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IRoleProvider, CookieRoleProvider>();

        // API checking HTTP client.
        builder.Services.AddHttpClient<ApiCheckService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "ENSE707-SecureApiQualityPlatform/0.1");
        });

        // Trusted test/development executables can replace services before any
        // database is opened. Normal startup keeps the SQLite registration.
        if (configureServices is not null)
        {
            if (!builder.Environment.IsDevelopment()
                && !builder.Environment.IsEnvironment("Testing"))
                throw new InvalidOperationException("Service overrides require Development or Testing.");

            configureServices(builder.Services);
        }

        var app = builder.Build();

        // A replaced in-memory store must never open the normal database.
        // Initialise persistence only when the application actually uses it.
        if (app.Services.GetRequiredService<IPlatformStore>() is SqlitePlatformStore)
        {
            using var scope = app.Services.CreateScope();
            var dbFactory = scope.ServiceProvider
                .GetRequiredService<IDbContextFactory<PlatformDbContext>>();

            using var db = dbFactory.CreateDbContext();
            db.Database.EnsureCreated();
        }

        // Preserve the existing error handling and security middleware.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        return app;
    }
}
