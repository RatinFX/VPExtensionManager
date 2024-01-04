using Octokit;
using System;
using System.Collections.Generic;
using System.Text;

namespace VPExtensionManager.Core.Services;

public class GitHubService
{
    public static GitHubClient Client { get; private set; } = new GitHubClient(new ProductHeaderValue("VPExtensionManager_" + DateTime.Now.Ticks));
    public static string GitHubUserName => "RatinFX";

    public static Release GetLatestRelease(string repo)
    {
        return Client.Repository.Release.GetLatest(GitHubUserName, repo).Result;
    }
}
