using VPExtensionManager.Contracts.Services;
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
            return versions.Split(",").Select(int.Parse).ToList();
        }

        return [];
    }

    public void SetLocalVersions(List<int> versions)
    {
        AppProperties.Set(AppProperties.VPVersions, string.Join(",", versions));
        _extensionService.SetPossibleFolders(versions);
    }
}
