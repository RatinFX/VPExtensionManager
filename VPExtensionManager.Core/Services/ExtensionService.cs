using System;
using System.Net;
using System.Text.RegularExpressions;
using VPExtensionManager.Core.Contracts.Services;
using VPExtensionManager.Core.Models;

namespace VPExtensionManager.Core.Services;

public class ExtensionService : IExtensionService
{
    public static string VersionPattern => @"(?<=\[version=)(.*?)(?=\])";

    public VPExtension CreateExtension(string creator, string extensionName, string slug, ExtensionType type)
    {
        var extension = new VPExtension(creator, extensionName, slug, type);

        CheckUpdateFor(extension, error => { /* Handle erros */ });

        LocateInstallsFor(extension);

        return extension;
    }

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

    public void CheckUpdateFor(VPExtension extension, Action<string> error = null)
    {
        try
        {
            ServicePointManager.Expect100Continue = true;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
            //       | SecurityProtocolType.Tls11
            //       | SecurityProtocolType.Tls12
            //       | SecurityProtocolType.Ssl3;

            using var newClient = new HttpClient();

            var content = newClient.GetStringAsync($"https://ratinfx.github.io/version/{extension.Slug}").Result;

            var githubTest = newClient.GetStringAsync($"https://github.com/ratinfx/{extension.GithubSlug}/releases/latest").Result;

            var latestVersion = Regex.Match(content, VersionPattern).Value;

            if (string.IsNullOrEmpty(latestVersion) || latestVersion.Length < 6)
            {
                error?.Invoke("Failed to check for Update.");
                return;
            }

            var updateAvailable = !extension.InstalledVersion.Equals(latestVersion);
            if (updateAvailable)
            {
                extension.LatestVersion = latestVersion;
            }
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
            VPVersion = "14",
            InstallPath = @"D:\Folder-14\somewhere\else\on\this\pc"
        });

        extension.Installs.Add(new Install
        {
            VPVersion = "18",
            InstallPath = extension.DownloadLink("18")
        });
    }

    public void UpdateExtension(VPExtension extension)
    {

    }
}
