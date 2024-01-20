using Octokit;
using VPExtensionManager.Interfaces.Services;
using VPExtensionManager.Models;

namespace VPExtensionManager.Services;

/// <summary>
/// TODO: Add GitHub Authentication if we add more Extensions to avoid Rate limits
/// </summary>
public class GitHubService : IGitHubService
{
    private readonly GitHubClient _client = new(new ProductHeaderValue("VPExtensionManager_" + DateTimeOffset.Now.ToUnixTimeMilliseconds()));

    public Release GetLatestRelease(string extensionName)
    {
        return _client.Repository.Release.GetLatest(RFXStrings.RatinFX, extensionName)?.Result;
    }

    public int GetRemainingCalls()
    {
        return _client.RateLimit.GetRateLimits().Result.Resources.Core.Remaining;
    }

    public string GetRateLimitResetTime()
    {
        var rem = _client.RateLimit.GetRateLimits().Result.Resources.Core.Reset.Subtract(DateTimeOffset.Now).TotalMinutes;
        return $"{rem:0.00} minutes";
    }
}
