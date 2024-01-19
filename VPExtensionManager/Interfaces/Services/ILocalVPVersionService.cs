namespace VPExtensionManager.Interfaces.Services;

public interface ILocalVPVersionService
{
    List<int> GetLocalVersions();
    void SetLocalVersions(List<int> versions);
}
