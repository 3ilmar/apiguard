using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Tests;

[TestClass]
public class EndpointPathValidatorTests
{
    [TestMethod]
    [DataRow("/health")]
    [DataRow("/users/42")]
    [DataRow("users")]
    [DataRow("/")]
    [DataRow("/items/a%20b")]
    public void SafePaths_AreAccepted(string path)
    {
        Assert.IsTrue(EndpointPathValidator.IsSafe(path, out _));
    }

    [TestMethod]
    [DataRow("/../admin")]
    [DataRow("/%2e%2e/admin")]
    [DataRow("/%2E%2E/admin")]
    [DataRow("/.%2e/admin")]
    [DataRow("/%252e%252e/admin")]
    [DataRow("/..%2fadmin")]
    [DataRow("/a%5c..%5cb")]
    [DataRow("/a\\..\\b")]
    [DataRow("//evil.com/x")]
    [DataRow("/%2f%2fevil.com")]
    [DataRow("https://evil.com/x")]
    [DataRow("/ok%0d%0aX-Injected: 1")]
    public void UnsafePaths_AreRejected_WithAReason(string path)
    {
        Assert.IsFalse(EndpointPathValidator.IsSafe(path, out var reason));
        Assert.IsFalse(string.IsNullOrWhiteSpace(reason));
    }

    [TestMethod]
    public void ExcessiveEncoding_IsRejected()
    {
        Assert.IsFalse(EndpointPathValidator.IsSafe("/%2525252e", out var reason));
        StringAssert.Contains(reason, "too many levels");
    }
}