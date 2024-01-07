using VPExtensionManager.Core.Models;

namespace VPExtensionManager.Core.Contracts.Services;

public interface IExtensionService
{
    Task<IEnumerable<VPExtension>> InitializeExtensions();
}
