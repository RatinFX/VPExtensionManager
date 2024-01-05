using VPExtensionManager.Core.Contracts.Services;
using VPExtensionManager.Core.Models;

namespace VPExtensionManager.Core.Services;

public class ExtensionService : IExtensionService
{
    public async Task<IEnumerable<VPExtension>> GetListDetailsDataAsync()
    {
        var extensions = new List<VPExtension>
        {
            CreateExtension("RatinFX", "VPConsole", ExtensionType.Extension, "vpconsole"),
            CreateExtension("RatinFX", "VPFlow", ExtensionType.Extension, "vegas-pro-flow"),
            CreateExtension("RatinFX", "ShortenExtendMedia", ExtensionType.Script, "shorten-extend-media"),
            CreateExtension("RatinFX", "CustomFades", ExtensionType.Script, "custom-fades"),
        };

        await Task.CompletedTask;
        return extensions;
    }

    public VPExtension CreateExtension(string creator, string extensionName, ExtensionType type, string websiteSlug)
    {
        var extension = new VPExtension(creator, extensionName, type, websiteSlug);
        GetLatest(extension, error => { /* Handle erros */ });
        LocateInstallsFor(extension);
        return extension;
    }

    public void GetLatest(VPExtension extension, Action<string> error = null)
    {
        try
        {
            var release = GitHubService.GetLatestRelease(extension.RepositoryName);
            var latestVersion = release.TagName;
            var assets = release.Assets.Where(x => x.BrowserDownloadUrl.EndsWith(".dll") || x.BrowserDownloadUrl.EndsWith(".zip")).ToList();

            extension.LatestVersion = latestVersion;
            extension.Assets = assets;
        }
        catch (Exception ex)
        {
            extension.RepositoryWasFound = false;
            extension.LatestVersion = "GitHub was not found";
            error?.Invoke("Failed to get the Latest version.\n" + ex.Message);
            return;
        }
    }

    public void LocateInstallsFor(VPExtension extension)
    {
        // placeholder install locations
        extension.Installs.Add(new Install
        {
            VPVersion = "13",
            InstallPath = @"D:\Folder-14\somewhere\else\on\this\pc"
        });

        extension.Installs.Add(new Install
        {
            VPVersion = "14",
            // DL link test in the mean time
            InstallPath = extension.GetDownloadLink("14")
        });
    }

    public void UpdateExtension(VPExtension extension)
    {

    }

    public void RefreshExtensions()
    {

    }
}
