using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Tests;

[TestClass]
public class FailureMessageTests
{
    // These guards verify the evidence in the messages. A representative user's
    // ability to understand them still requires the manual usability case.
    [DataTestMethod]
    [DataRow("Status", "Expected HTTP 200", "received 400")]
    [DataRow("Timing", "1001 ms", "1000 ms threshold")]
    [DataRow("Authentication", "authentication-required", "HTTP 200")]
    [DataRow("ContentType", "application/json", "text/html")]
    [DataRow("Availability", "Request failed", "Request timed out.")]
    public void FailedCheck_MessageIncludesExpectationAndObservedEvidence(
        string scenario, string firstEvidence, string secondEvidence)
    {
        var evaluator = new CheckEvaluator();
        var endpoint = new ApiEndpoint
        {
            ExpectedStatusCode = 200,
            MaxResponseTimeMs = 1000,
            RequiresAuthentication = true,
            ExpectedContentType = "application/json"
        };
        var result = scenario switch
        {
            "Status" => evaluator.ExpectedStatus(endpoint.ApiId, endpoint, HttpStatusCode.BadRequest),
            "Timing" => evaluator.ResponseTime(endpoint.ApiId, endpoint, 1001),
            "Authentication" => evaluator.AuthenticationProtection(endpoint.ApiId, endpoint, HttpStatusCode.OK),
            "ContentType" => evaluator.ContentType(endpoint.ApiId, endpoint, "text/html"),
            "Availability" => evaluator.AvailabilityFailure(endpoint.ApiId, endpoint, "Request timed out."),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };

        Assert.IsFalse(result.Passed);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.CheckType));
        StringAssert.Contains(result.Message, firstEvidence);
        StringAssert.Contains(result.Message, secondEvidence);
    }
}
