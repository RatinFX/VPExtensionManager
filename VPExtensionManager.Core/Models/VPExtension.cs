using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPExtensionManager.Core.Models;

public class Install
{
    public string VPVersion { get; set; }
    public string InstallPath { get; set; }
}

public enum ExtensionType
{
    Extension,
    Script,
}

public class VPExtension
{
    public string Creator { get; set; } = "creator";
    public string ExtensionName { get; set; } = "name";

    public string InstalledVersion { get; set; } = "not installed";
    public string LatestVersion { get; set; } = string.Empty;
    public bool UpdateAvailable => !string.IsNullOrEmpty(LatestVersion);
    public string Version => UpdateAvailable
        ? $"{InstalledVersion} -> {LatestVersion}"
        : InstalledVersion;

    public List<Install> Installs { get; set; } = new();

    public string Slug { get; set; }
    public string GithubSlug => Slug.Replace("-", "");

    public ExtensionType Type { get; set; }
    public string TypeName => Type.ToString("G");

    public VPExtension() { }

    public VPExtension(string creator, string extensionName, string slug, ExtensionType type)
    {
        Creator = creator;
        ExtensionName = extensionName;
        Slug = slug;
        Type = type;
    }

    public string DownloadLink(string vpVersion)
    {
        if (string.IsNullOrEmpty(LatestVersion))
            return string.Empty;

        var tag = LatestVersion.Remove(LatestVersion.Length - 1);

        if (Type is ExtensionType.Extension)
            return $@"https://github.com/RatinFX/{GithubSlug}/releases/download/{tag}/{GithubSlug}{vpVersion}-{tag}.zip";

        return $@"https://github.com/RatinFX/{GithubSlug}/releases/download/{tag}/{GithubSlug}{vpVersion}.dll";
    }
}
