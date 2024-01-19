using System.Collections;
using System.IO;

using Microsoft.Extensions.Options;
using VPExtensionManager.Core.Interfaces;
using VPExtensionManager.Interfaces.Services;
using VPExtensionManager.Models;

namespace VPExtensionManager.Services;

public class PersistAndRestoreService : IPersistAndRestoreService
{
    private readonly IFileService _fileService;
    private readonly AppConfig _appConfig;
    private readonly string _localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public PersistAndRestoreService(IFileService fileService, IOptions<AppConfig> appConfig)
    {
        _fileService = fileService;
        _appConfig = appConfig.Value;
    }

    public void PersistData()
    {
        var folderPath = Path.Combine(_localAppData, _appConfig.MainFolder, _appConfig.ConfigurationsFolder);

        if (App.Current.Properties != null)
        {
            var fileName = _appConfig.AppPropertiesFileName;
            _fileService.Save(folderPath, fileName, App.Current.Properties);
        }

        if (ExtensionService.Extensions != null)
        {
            var fileName = _appConfig.ExtensionsFileName;
            _fileService.Save(folderPath, fileName, ExtensionService.Extensions);
        }
    }

    public void RestoreData()
    {
        var folderPath = Path.Combine(_localAppData, _appConfig.MainFolder, _appConfig.ConfigurationsFolder);
        var appPropertiesName = _appConfig.AppPropertiesFileName;
        var properties = _fileService.Read<IDictionary>(folderPath, appPropertiesName);
        if (properties != null)
        {
            foreach (DictionaryEntry property in properties)
            {
                App.Current.Properties.Add(property.Key, property.Value);
            }
        }

        var extensionsName = _appConfig.ExtensionsFileName;
        var extensions = _fileService.Read<List<VPExtension>>(folderPath, extensionsName);
        if (extensions != null)
        {
            foreach (VPExtension ext in extensions)
            {
                ExtensionService.Extensions.Add(ext);
            }
        }
    }
}
