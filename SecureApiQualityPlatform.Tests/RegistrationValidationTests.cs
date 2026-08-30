using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureApiQualityPlatform.Web.Models;
using SecureApiQualityPlatform.Web.Services;

namespace SecureApiQualityPlatform.Tests;

[TestClass]
public class RegistrationValidationTests
{
    [TestMethod]
    public void TryAddApi_DuplicateBaseUrl_ReturnsFalse()
    {
        var store = new InMemoryPlatformStore();

        var firstApi = new RegisteredApi
        {
            Name = "First API",
            BaseUrl = "https://example.com"
        };

        var duplicateApi = new RegisteredApi
        {
            Name = "Different Name",
            BaseUrl = "https://example.com/"
        };

        Assert.IsTrue(store.TryAddApi(firstApi));
        Assert.IsFalse(store.TryAddApi(duplicateApi));
        Assert.AreEqual(1, store.GetApis().Count);
    }

    [TestMethod]
    public void TryAddApi_DifferentBaseUrl_ReturnsTrue()
    {
        var store = new InMemoryPlatformStore();

        var firstApi = new RegisteredApi
        {
            Name = "First API",
            BaseUrl = "https://example.com"
        };

        var secondApi = new RegisteredApi
        {
            Name = "Second API",
            BaseUrl = "https://example.org"
        };

        Assert.IsTrue(store.TryAddApi(firstApi));
        Assert.IsTrue(store.TryAddApi(secondApi));
        Assert.AreEqual(2, store.GetApis().Count);
    }

    [TestMethod]
    public void TryAddEndpoint_DuplicateMethodAndPath_ReturnsFalse()
    {
        var store = new InMemoryPlatformStore();

        var api = new RegisteredApi
        {
            Name = "Test API",
            BaseUrl = "https://example.com"
        };

        store.AddApi(api);

        var firstEndpoint = new ApiEndpoint
        {
            ApiId = api.Id,
            Name = "Get Post",
            Path = "/posts/1",
            Method = "GET"
        };

        var duplicateEndpoint = new ApiEndpoint
        {
            ApiId = api.Id,
            Name = "Duplicate",
            Path = "/posts/1/",
            Method = "get"
        };

        Assert.IsTrue(store.TryAddEndpoint(api.Id, firstEndpoint));
        Assert.IsFalse(store.TryAddEndpoint(api.Id, duplicateEndpoint));
        Assert.AreEqual(1, api.Endpoints.Count);
    }

    [TestMethod]
    public void TryAddEndpoint_SamePathDifferentMethod_ReturnsTrue()
    {
        var store = new InMemoryPlatformStore();

        var api = new RegisteredApi
        {
            Name = "Test API",
            BaseUrl = "https://example.com"
        };

        store.AddApi(api);

        var getEndpoint = new ApiEndpoint
        {
            ApiId = api.Id,
            Name = "Get Post",
            Path = "/posts/1",
            Method = "GET"
        };

        var postEndpoint = new ApiEndpoint
        {
            ApiId = api.Id,
            Name = "Update Post",
            Path = "/posts/1",
            Method = "POST"
        };

        Assert.IsTrue(store.TryAddEndpoint(api.Id, getEndpoint));
        Assert.IsTrue(store.TryAddEndpoint(api.Id, postEndpoint));
        Assert.AreEqual(2, api.Endpoints.Count);
    }
}