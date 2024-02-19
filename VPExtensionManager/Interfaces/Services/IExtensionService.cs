using VPExtensionManager.Models;

namespace VPExtensionManager.Interfaces.Services;

public interface IExtensionService
{
    void SetDownloadsPath(string downloadsPath);
    void SetPossibleFolders(List<int> localVersions);
    IEnumerable<VPExtension> InitializeExtensions();

    bool RefreshLatestRelease(VPExtension extension);
    void RefreshInstallFolders(VPExtension extension);
    List<string> GetAvailableFolders(VPExtension extension);

    VPInstall Install(VPExtension extension, VPVersion vp, string installPath, bool forceDownload);
    bool Update(VPExtension extension, VPVersion vp, string installPath, bool forceDownload);
    bool Uninstall(VPExtension extension, VPInstall selectedInstall);
}
