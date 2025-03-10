using Octokit;
using System.Diagnostics;
using VPExtensionManager.Helpers;
using VPExtensionManager.Interfaces.Services;

namespace VPExtensionManager.Services;

/// <summary>
/// TODO: Add GitHub Authentication if we add more Extensions to avoid Rate limits
/// </summary>
public class GitHubService : IGitHubService
{
    private readonly GitHubClient _client = new(new ProductHeaderValue("VPExtensionManager_" + DateTimeHelper.GetCurrentUnixTime()));

    public Release GetLatestRelease(string creator, string repositoryName)
    {
        return _client.Repository.Release.GetLatest(creator, repositoryName)?.Result;
    }

    public int GetRemainingCalls()
    {
        try
        {
            return _client.RateLimit.GetRateLimits().Result.Resources.Core.Remaining;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            throw;
        }
    }

    public string GetRateLimitResetTime()
    {
        var rem = _client.RateLimit.GetRateLimits().Result.Resources.Core.Reset.Subtract(DateTimeOffset.Now).TotalMinutes;
        return $"{rem:0.00} minutes";
    }

    public static string GetRateLimitExceptionErrorMessage(Exception ex)
    {
        if (ex is not RateLimitExceededException)
        {
            return ex.Message;
        }

        // Let's not show the IP of the user for now...
        var rateLimited = ex as RateLimitExceededException;
        var remaining = rateLimited.Reset.Subtract(DateTimeOffset.Now).TotalMinutes;
        return string.Format(Properties.Resources.NotificationErrorGitHubRateLimited, remaining);
    }
}
