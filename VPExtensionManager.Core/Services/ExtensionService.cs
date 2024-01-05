using System;
using System.Net;
using System.Text.RegularExpressions;
using VPExtensionManager.Core.Contracts.Services;
using VPExtensionManager.Core.Models;

namespace VPExtensionManager.Core.Services;

public class ExtensionService : IExtensionService
{
    public async Task<IEnumerable<VPExtension>> GetListDetailsDataAsync()
    {
        var extensions = new List<VPExtension>
        {
            CreateExtension("RatinFX", "VPConsole", "vpconsole", ExtensionType.Extension),
            CreateExtension("RatinFX", "VPFlow", "vegas-pro-flow", ExtensionType.Extension),
            CreateExtension("RatinFX", "ShortenExtendMedia", "shorten-extend-media", ExtensionType.Script),
            CreateExtension("RatinFX", "CustomFades", "custom-fades", ExtensionType.Script),
        };

        await Task.CompletedTask;
        return extensions;
    }

    public VPExtension CreateExtension(string creator, string extensionName, string websiteSlug, ExtensionType type)
    {
        var extension = new VPExtension(creator, extensionName, websiteSlug, type);

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
            error?.Invoke("Failed to check for Update.\n" + ex.Message);
            return;
        }
    }

    public void LocateInstallsFor(VPExtension extension)
    {
        // test install locations
        extension.Installs.Add(new Install
        {
            VPVersion = "13",
            InstallPath = @"D:\Folder-14\somewhere\else\on\this\pc"
        });

        extension.Installs.Add(new Install
        {
            VPVersion = "14",
            InstallPath = extension.GetDownloadLink("14")
        });
    }

    public void UpdateExtension(VPExtension extension)
    {

    }
}
