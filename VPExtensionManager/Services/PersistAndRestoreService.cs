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
    private readonly IFolderService _folderService;

    public PersistAndRestoreService(IFileService fileService, IOptions<AppConfig> appConfig, IFolderService folderService)
    {
        _fileService = fileService;
        _appConfig = appConfig.Value;
        _folderService = folderService;
    }

    public void PersistData()
    {
        if (App.Current.Properties != null)
        {
            var fileName = _appConfig.AppPropertiesFileName;
            _fileService.Save(_folderService.DefaultConfigurationFolder, fileName, App.Current.Properties);
        }

        if (ExtensionService.Extensions != null)
        {
            var fileName = _appConfig.ExtensionsFileName;
            _fileService.Save(_folderService.DefaultConfigurationFolder, fileName, ExtensionService.Extensions);
        }
    }

    public void RestoreData()
    {
        var appPropertiesName = _appConfig.AppPropertiesFileName;
        var properties = _fileService.Read<IDictionary>(_folderService.DefaultConfigurationFolder, appPropertiesName);
        if (properties != null)
        {
            foreach (DictionaryEntry property in properties)
            {
                App.Current.Properties.Add(property.Key, property.Value);
            }
        }

        var extensionsName = _appConfig.ExtensionsFileName;
        var extensions = _fileService.Read<List<VPExtensionBase>>(_folderService.DefaultConfigurationFolder, extensionsName);
        if (extensions != null)
        {
            foreach (VPExtensionBase ext in extensions)
            {
                ExtensionService.Extensions.Add(ext);
            }
        }
    }
}
