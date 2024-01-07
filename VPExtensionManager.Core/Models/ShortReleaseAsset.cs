﻿using Octokit;

namespace VPExtensionManager.Core.Models;

public class ShortReleaseAsset
{
    public string Name { get; set; }
    public string BrowserDownloadUrl { get; set; }
    public VPVersion VP { get; set; }

    public ShortReleaseAsset() { }
    public ShortReleaseAsset(ReleaseAsset release, VPVersion vp)
    {
        BrowserDownloadUrl = release.BrowserDownloadUrl;
        Name = release.Name;
        VP = vp;
    }

    public static VPVersion GetVersion(string name, string tag)
        => name.Replace(tag, "").Contains(VPVersion.Sony.ToString()) ? VPVersion.Sony : VPVersion.Magix;
}