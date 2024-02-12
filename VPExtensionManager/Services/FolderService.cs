using Microsoft.Extensions.Options;
using System.IO;
using VPExtensionManager.Interfaces.Services;
using VPExtensionManager.Models;

namespace VPExtensionManager.Services;

public class FolderService : IFolderService
{
    private readonly AppConfig _appConfig;
    public string LocalAppData { get; }
    public string DefaultDownloadsFolder { get; }
    public string DefaultConfigurationFolder { get; }

    public FolderService(IOptions<AppConfig> appConfig)
    {
        _appConfig = appConfig.Value;
        LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        DefaultDownloadsFolder = Path.Combine(LocalAppData, _appConfig.MainFolder, _appConfig.DownloadsFolder);
        DefaultConfigurationFolder = Path.Combine(LocalAppData, _appConfig.MainFolder, _appConfig.ConfigurationsFolder);
    }

    public string GetDownloadsFolder()
    {
        if (AppProperties.Get(AppProperties.DownloadsFolder, out string downloadsFolder))
        {
            return downloadsFolder;
        }

        ResetDownloadsFolder();
        return DefaultDownloadsFolder;
    }

    public void SaveDownloadsFolder(string path)
    {
        AppProperties.Set(AppProperties.DownloadsFolder, path);
    }

    public string ResetDownloadsFolder()
    {
        SaveDownloadsFolder(DefaultDownloadsFolder);
        return DefaultDownloadsFolder;
    }
}
