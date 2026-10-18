using SecureApiQualityPlatform.Web.Services;

var app = Program.CreateApp(args);
app.Run();

public partial class Program
{
    // The executable and integration tests share the same registrations and MVC pipeline.
    // Each call owns a fresh service provider, including its own in-memory store.
    public static WebApplication CreateApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ApplicationName = typeof(Program).Assembly.GetName().Name
        });

        builder.Services.AddControllersWithViews();
        builder.Services.AddSingleton<IPlatformStore, InMemoryPlatformStore>();
        builder.Services.AddSingleton<CheckEvaluator>();
        builder.Services.AddSingleton<DefectLifecycleService>();
        builder.Services.AddSingleton<DashboardService>();
        builder.Services.AddSingleton<UrlSafetyService>();
        builder.Services.AddHttpClient<ApiCheckService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("ENSE707-SecureApiQualityPlatform/0.1");
        });

        var app = builder.Build();

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
