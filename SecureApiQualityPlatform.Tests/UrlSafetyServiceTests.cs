using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Tests;

[TestClass]
public class UrlSafetyServiceTests
{
    [TestMethod]
    public async Task ValidateAsync_ShouldRejectUrlsWithCredentials()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var service = new UrlSafetyService(config);

        var url = "http://user:pass@example.com";

        // Act
        var result = await service.ValidateAsync(url);

        // Assert
        Assert.IsFalse(result.IsSafe);
    }
}