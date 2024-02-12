namespace VPExtensionManager.Interfaces.Services;

public interface IFolderService
{
    string LocalAppData { get; }
    string DefaultDownloadsFolder { get; }
    string DefaultConfigurationFolder { get; }
    string GetDownloadsFolder();
    void SaveDownloadsFolder(string path);
    string ResetDownloadsFolder();
}
