using Microsoft.EntityFrameworkCore;
using SecureApiQualityPlatform.Web.Models;

namespace SecureApiQualityPlatform.Web.Data;

public sealed class PlatformDbContext : DbContext
{
    public PlatformDbContext(DbContextOptions<PlatformDbContext> options)
        : base(options)
    {
    }

    public DbSet<RegisteredApi> Apis => Set<RegisteredApi>();
    public DbSet<ApiEndpoint> Endpoints => Set<ApiEndpoint>();
    public DbSet<ApiCheckResult> CheckResults => Set<ApiCheckResult>();
    public DbSet<Defect> Defects => Set<Defect>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RegisteredApi>()
            .HasMany(api => api.Endpoints)
            .WithOne()
            .HasForeignKey(endpoint => endpoint.ApiId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RegisteredApi>()
            .HasIndex(api => api.BaseUrl);

        modelBuilder.Entity<ApiEndpoint>()
            .HasIndex(endpoint => new
            {
                endpoint.ApiId,
                endpoint.Method,
                endpoint.Path
            });

        modelBuilder.Entity<ApiCheckResult>()
            .HasIndex(result => result.ApiId);

        modelBuilder.Entity<Defect>()
            .HasIndex(defect => defect.EndpointId);
    }
}