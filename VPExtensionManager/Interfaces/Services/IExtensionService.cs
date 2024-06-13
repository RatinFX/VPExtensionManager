using VPExtensionManager.Models;

namespace VPExtensionManager.Interfaces.Services;

public interface IExtensionService
{
    void SetDownloadsPath(string downloadsPath);
    void SetPossibleFolders(List<int> localVersions);
    IEnumerable<VPExtensionBase> InitializeExtensions();

    bool RefreshLatestRelease(VPExtensionBase extension);
    void RefreshInstallFolders(VPExtensionBase extension);
    List<string> GetAvailableFolders(VPExtensionBase extension);

    VPInstall Install(VPExtensionBase extension, VPVersion vp, string installPath, bool forceDownload);
    bool Update(VPExtensionBase extension, VPVersion vp, string installPath, bool forceDownload);
    bool Uninstall(VPExtensionBase extension, VPInstall selectedInstall);
}
