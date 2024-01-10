using VPExtensionManager.Core.Models;

namespace VPExtensionManager.Core.Contracts.Services;

public interface IExtensionService
{
    IEnumerable<VPExtension> InitializeExtensions();
    void RefreshLatestRelease(VPExtension extension);
    void RefreshInstallFolders(VPExtension extension);
}
