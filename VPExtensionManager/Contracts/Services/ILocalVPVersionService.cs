namespace VPExtensionManager.Contracts.Services;

public interface ILocalVPVersionService
{
    List<int> GetLocalVersions();
    void SetLocalVersions(List<int> versions);
}
