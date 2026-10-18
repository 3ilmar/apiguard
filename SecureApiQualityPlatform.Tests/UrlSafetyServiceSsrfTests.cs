
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Tests;

[TestClass]
public class UrlSafetyServiceSsrfTests
{
    private static UrlSafetyService Create() =>
        new(new ConfigurationBuilder().Build());

    [TestMethod]
    [DataRow("http://127.0.0.1")]
    [DataRow("http://localhost")]
    [DataRow("http://10.0.0.5")]
    [DataRow("http://172.16.0.1")]
    [DataRow("http://172.31.255.254")]
    [DataRow("http://192.168.1.10")]
    [DataRow("http://169.254.169.254")]
    [DataRow("http://[::1]")]
    public async Task BlockedTargets_AreRejected_WithSafetyReason(string url)
    {
        var result = await Create().ValidateAsync(url);

        Assert.IsFalse(result.IsSafe, $"{url} was accepted: {result.Reason}");
        StringAssert.Contains(
            result.Reason,
            "blocked by default",
            $"{url}: {result.Reason}");
    }

    [TestMethod]
    public async Task UnspecifiedIpv4Target_IsRejected()
    {
        var result = await Create().ValidateAsync("http://0.0.0.0");

        Assert.IsFalse(result.IsSafe, result.Reason);
    }

    [TestMethod]
    [DataRow("https://172.15.255.255")]
    [DataRow("https://172.32.0.1")]
    [DataRow("https://8.8.8.8")]
    public async Task PublicTargets_JustOutsidePrivateRanges_AreAccepted(string url)
    {
        var result = await Create().ValidateAsync(url);

        Assert.IsTrue(result.IsSafe, $"{url} was rejected: {result.Reason}");
    }

    [TestMethod]
    [DataRow("ftp://example.com")]
    [DataRow("file:///etc/passwd")]
    [DataRow("not a url")]
    public async Task NonHttpOrMalformedUrls_AreRejected(string url)
    {
        var result = await Create().ValidateAsync(url);

        Assert.IsFalse(result.IsSafe);
        StringAssert.Contains(result.Reason, "Only absolute http");
    }

    // Known gap flagged in the A1 Copilot review: IPv6 forms of private addresses.
    [TestMethod]
    [DataRow("http://[::ffff:10.0.0.1]")]
    [DataRow("http://[::ffff:127.0.0.1]")]
    [DataRow("http://[fc00::1]")]
    [DataRow("http://[fd12:3456:789a::1]")]
    public async Task Ipv6PrivateForms_AreRejected_WithSafetyReason(string url)
    {
        var result = await Create().ValidateAsync(url);

        Assert.IsFalse(result.IsSafe, $"{url} was accepted: {result.Reason}");
        StringAssert.Contains(
            result.Reason,
            "blocked by default",
            $"{url}: {result.Reason}");
    }
}
