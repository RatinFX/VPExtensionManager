﻿using System.Diagnostics;
using System.Reflection;
using VPExtensionManager.Interfaces.Services;

namespace VPExtensionManager.Services;

public class ApplicationInfoService : IApplicationInfoService
{
    public ApplicationInfoService() { }

    public Version GetVersion()
    {
        // Set the app version in VPExtensionManager > Properties > Package > PackageVersion
        string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var version = FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;
        return new Version(version);
    }

    public string GetVersionShort()
    {
        var version = GetVersion();
        return $"{version.Major}.{version.Minor}.{version.Build}";
    }
}
