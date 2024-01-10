namespace VPExtensionManager.Core.Models;

/// <summary>
/// TODO: Add warning if some reference files are missing via a ToolTip
/// </summary>
public class VPInstall
{
    public string Version { get; set; }
    public string InstallPath { get; set; }

    public VPInstall() { }
    public VPInstall(string version, string path)
    {
        Version = version;
        InstallPath = path;
    }
}
