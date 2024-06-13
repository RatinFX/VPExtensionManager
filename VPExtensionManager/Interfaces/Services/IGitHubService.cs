using Octokit;

namespace VPExtensionManager.Interfaces.Services;

public interface IGitHubService
{
    Release GetLatestRelease(string repositoryName);
    int GetRemainingCalls();
    string GetRateLimitResetTime();
}
