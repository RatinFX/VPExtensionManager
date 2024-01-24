using Octokit;

namespace VPExtensionManager.Interfaces.Services;

public interface IGitHubService
{
    Release GetLatestRelease(string extensionName);
    int GetRemainingCalls();
    string GetRateLimitResetTime();
    string GetRateLimitExceptionErrorMessage(Exception ex);
}
