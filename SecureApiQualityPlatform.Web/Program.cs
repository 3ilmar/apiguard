using Microsoft.EntityFrameworkCore;
using SecureApiQualityPlatform.Web.Data;
using SecureApiQualityPlatform.Web.Services;

var app = Program.CreateApp(args);
app.Run();

public partial class Program
{
    public static WebApplication CreateApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ApplicationName = typeof(Program).Assembly.GetName().Name
        });

        builder.Services.AddControllersWithViews();

        // Persistent SQLite storage.
        builder.Services.AddDbContextFactory<PlatformDbContext>(options =>
            options.UseSqlite(
                builder.Configuration.GetConnectionString("PlatformDatabase")));

        builder.Services.AddSingleton<IPlatformStore, SqlitePlatformStore>();
        builder.Services.AddSingleton<CheckEvaluator>();
        builder.Services.AddSingleton<DefectLifecycleService>();
        builder.Services.AddSingleton<DashboardService>();
        builder.Services.AddSingleton<UrlSafetyService>();

        builder.Services.AddHttpClient<ApiCheckService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "ENSE707-SecureApiQualityPlatform/0.1");
        });

        var app = builder.Build();

        // Ensure the SQLite database exists before handling requests.
        using (var scope = app.Services.CreateScope())
        {
            var dbFactory = scope.ServiceProvider
                .GetRequiredService<IDbContextFactory<PlatformDbContext>>();

            using var db = dbFactory.CreateDbContext();
            db.Database.EnsureCreated();
        }

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