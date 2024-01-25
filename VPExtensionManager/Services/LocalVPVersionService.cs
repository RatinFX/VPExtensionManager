using VPExtensionManager.Interfaces.Services;
using VPExtensionManager.Models;

namespace VPExtensionManager.Services;

public class LocalVPVersionService : ILocalVPVersionService
{
    private readonly IExtensionService _extensionService;

    public LocalVPVersionService(IExtensionService extensionService)
    {
        _extensionService = extensionService;
    }

    public List<int> GetLocalVersions()
    {
        if (AppProperties.Get(AppProperties.VPVersions, out string versions))
        {
            return string.IsNullOrEmpty(versions) ? []
                : versions
                .Split(",")
                .Select(x => int.TryParse(x, out int result) ? result : 0)
                .Where(x => x != 0)
                .ToList();
        }

        return [];
    }

    public void SetLocalVersions(List<int> versions)
    {
        AppProperties.Set(AppProperties.VPVersions, string.Join(",", versions));
        _extensionService.SetPossibleFolders(versions);
    }
}
