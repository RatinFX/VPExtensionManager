using Octokit;
using VPExtensionManager.Models;

namespace VPExtensionManager.Services;

public class GitHubService
{
    public static GitHubClient Client { get; private set; } = new GitHubClient(new ProductHeaderValue("VPExtensionManager_" + DateTime.Now.Ticks));

    public static Release GetLatestRelease(string extensionName)
    {
        // Fake release in case GitHub got timed out
        //return new Release("", "", "", "", 0, "", "1.5.2", "", "", "", false, false, DateTime.Now, null, null, "", "", null);
        return Client.Repository.Release.GetLatest(RFXStrings.RatinFX, extensionName)?.Result;
    }
}
