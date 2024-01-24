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

    public string GetRateLimitExceptionErrorMessage(Exception ex)
    {
        if (ex is not RateLimitExceededException)
            return ex.Message;

        // Let's not show the IP of the user for now...
        var rateLimited = ex as RateLimitExceededException;
        var remaining = rateLimited.Reset.Subtract(DateTimeOffset.Now).TotalMinutes;
        return $"GitHub API rate limit exceeded (60 per hour), please wait {remaining} minutes before retrying.";
    }
}
