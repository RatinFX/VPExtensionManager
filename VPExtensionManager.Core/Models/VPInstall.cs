using Newtonsoft.Json;

namespace VPExtensionManager.Core.Models;

public class VPInstall
{
    public VPVersion VPVersion { get; set; }
    [JsonIgnore]
    public string VPVersionName => $"Vegas Pro {VPVersion:D}.0";
    public string InstallPath { get; set; }
}
