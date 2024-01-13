using VPExtensionManager.Core.Models;

namespace VPExtensionManager.Core.Contracts.Services;

public interface IExtensionService
{
    void SetConfigPath(string configPath);
    void SetPossibleFolders(List<int> localVersions);
    IEnumerable<VPExtension> InitializeExtensions();
    void RefreshLatestRelease(VPExtension extension);
    void RefreshInstallFolders(VPExtension extension);
    List<string> GetAvailableFolders(VPExtension extension);
    VPInstall Install(VPExtension extension, VPVersion vp, string installPath, bool forceDownload);
    void Uninstall(VPExtension selected, VPInstall selectedInstall);
}
