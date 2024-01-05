using VPExtensionManager.Core.Models;

namespace VPExtensionManager.Core.Contracts.Services;

public interface IExtensionService
{
    Task<IEnumerable<VPExtension>> GetListDetailsDataAsync();
    void GetLatest(VPExtension extension, Action<string> error = null);
}
