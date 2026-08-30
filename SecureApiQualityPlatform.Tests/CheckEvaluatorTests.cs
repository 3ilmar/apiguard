using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Tests;

[TestClass]
public class CheckEvaluatorTests
{
    private readonly CheckEvaluator _evaluator = new();
    private readonly ApiEndpoint _endpoint = new()
    {
        ApiId = Guid.NewGuid(),
        Name = "Orders",
        Path = "/orders",
        ExpectedStatusCode = 200,
        MaxResponseTimeMs = 1000,
        ExpectedContentType = "application/json"
    };

    [TestMethod]
    public void ExpectedStatus_WhenMatching_ReturnsPass()
    {
        var result = _evaluator.ExpectedStatus(_endpoint.ApiId, _endpoint, HttpStatusCode.OK);
        Assert.IsTrue(result.Passed);
    }

    [TestMethod]
    public void ExpectedStatus_WhenDifferent_ReturnsFail()
    {
        var result = _evaluator.ExpectedStatus(_endpoint.ApiId, _endpoint, HttpStatusCode.BadRequest);
        Assert.IsFalse(result.Passed);
        Assert.AreEqual(DefectSeverity.High, result.FailureSeverity);
    }

    [TestMethod]
    public void ResponseTime_AtThreshold_ReturnsPass()
    {
        var result = _evaluator.ResponseTime(_endpoint.ApiId, _endpoint, 1000);
        Assert.IsTrue(result.Passed);
    }

    [TestMethod]
    public void ResponseTime_OverThreshold_ReturnsFail()
    {
        var result = _evaluator.ResponseTime(_endpoint.ApiId, _endpoint, 1001);
        Assert.IsFalse(result.Passed);
    }

    [TestMethod]
    public void AuthenticationProtection_WhenRequiredAndUnauthorized_ReturnsPass()
    {
        _endpoint.RequiresAuthentication = true;
        var result = _evaluator.AuthenticationProtection(_endpoint.ApiId, _endpoint, HttpStatusCode.Unauthorized);
        Assert.IsTrue(result.Passed);
    }

    [TestMethod]
    public void AuthenticationProtection_WhenRequiredButReturnsOk_ReturnsCriticalFail()
    {
        _endpoint.RequiresAuthentication = true;
        var result = _evaluator.AuthenticationProtection(_endpoint.ApiId, _endpoint, HttpStatusCode.OK);
        Assert.IsFalse(result.Passed);
        Assert.AreEqual(DefectSeverity.Critical, result.FailureSeverity);
    }

    [TestMethod]
    public void Cors_WhenAuthRequiredAndWildcard_ReturnsFail()
    {
        _endpoint.RequiresAuthentication = true;
        var result = _evaluator.Cors(_endpoint.ApiId, _endpoint, new[] { "*" });
        Assert.IsFalse(result.Passed);
    }

    [TestMethod]
    public void ContentType_WhenExpectedMatches_ReturnsPass()
    {
        var result = _evaluator.ContentType(_endpoint.ApiId, _endpoint, "application/json; charset=utf-8");
        Assert.IsTrue(result.Passed);
    }
}