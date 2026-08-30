using SecureApiQualityPlatform.Web.Models;

namespace SecureApiQualityPlatform.Web.Services;

public interface IPlatformStore
{
    IReadOnlyList<RegisteredApi> GetApis();
    RegisteredApi? GetApi(Guid id);
    void AddApi(RegisteredApi api);
    void AddEndpoint(Guid apiId, ApiEndpoint endpoint);

    IReadOnlyList<ApiCheckResult> GetResults();
    IReadOnlyList<ApiCheckResult> GetResultsForApi(Guid apiId);
    void AddResult(ApiCheckResult result);

    IReadOnlyList<Defect> GetDefects();
    Defect? GetDefect(Guid id);
    Defect? FindOpenDefect(Guid endpointId, string checkType);
    void AddDefect(Defect defect);
    void UpdateDefect(Defect defect);
}
