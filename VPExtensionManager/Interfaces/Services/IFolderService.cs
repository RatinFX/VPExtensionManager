namespace VPExtensionManager.Interfaces.Services;

public interface IFolderService
{
    string GetDownloadsFolder();
    void SaveDownloadsFolder(string path);
    void ResetDownloadsFolder();
}
