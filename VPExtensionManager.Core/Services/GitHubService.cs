using Octokit;

namespace VPExtensionManager.Core.Services;

public class GitHubService
{
    public static GitHubClient Client { get; private set; } = new GitHubClient(new ProductHeaderValue("VPExtensionManager_" + DateTime.Now.Ticks));
    public static string GitHubUserName => "RatinFX";

    public static Release GetLatestRelease(string extensionName)
    {
        return Client.Repository.Release.GetLatest(GitHubUserName, extensionName).Result;
    }
}
