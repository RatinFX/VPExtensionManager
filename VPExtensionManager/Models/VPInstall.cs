﻿namespace VPExtensionManager.Models;

/// <summary>
/// TODO: Add warning if some of the Dependency files are missing via a ToolTip
/// </summary>
public class VPInstall
{
    public string Version { get; set; }
    public string InstallPath { get; set; }
    public VPVersion VPVersion { get; set; }

    public VPInstall() { }
    public VPInstall(string version, string path, VPVersion vp)
    {
        Version = version;
        InstallPath = path;
        VPVersion = vp;
    }
}
