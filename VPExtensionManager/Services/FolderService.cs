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
        if (AppProperties.Get(AppProperties.DownloadsFolder, out string downloadsFolder))
        {
            return downloadsFolder;
        }

        ResetDownloadsFolder();
        return _defaultDownloadsFolder;
    }

    public void SaveDownloadsFolder(string path)
    {
        AppProperties.Set(AppProperties.DownloadsFolder, path);
    }

    public void ResetDownloadsFolder()
    {
        SaveDownloadsFolder(_defaultDownloadsFolder);
    }
}
