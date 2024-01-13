using VPExtensionManager.Contracts.Services;
using VPExtensionManager.Core.Contracts.Services;

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
        if (!App.Current.Properties.Contains("VPVersions"))
            return [];

        var localVersions = App.Current.Properties["VPVersions"]
                .ToString()
                .Split(",")
                .Select(x => int.Parse(x.Trim()))
                .ToList();

        return localVersions;
    }

    public void SetLocalVersions(List<int> versions)
    {
        App.Current.Properties["VPVersions"] = string.Join(",", versions);

        _extensionService.SetPossibleFolders(versions);
    }
}
