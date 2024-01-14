using Microsoft.Extensions.Options;
using System.IO;
using VPExtensionManager.Contracts.Services;
using VPExtensionManager.Models;

namespace VPExtensionManager.Services;

public class FolderService : IFolderService
{
    private readonly string _localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly string _defaultDownloadsFolder;
    private readonly AppConfig _appConfig;

    public FolderService(IOptions<AppConfig> appConfig)
    {
        _appConfig = appConfig.Value;
        _defaultDownloadsFolder = Path.Combine(_localAppData, _appConfig.MainFolder, _appConfig.DownloadsFolder);
    }

    public string GetDownloadsFolder()
    {
        if (!App.Current.Properties.Contains("DownloadsFolder"))
        {
            ResetDownloadsFolder();
            return _defaultDownloadsFolder;
        }

        return App.Current.Properties["DownloadsFolder"].ToString();
    }

    public void SaveDownloadsFolder(string path)
    {
        App.Current.Properties["DownloadsFolder"] = path;
    }

    public void ResetDownloadsFolder()
    {
        SaveDownloadsFolder(_defaultDownloadsFolder);
    }
}
