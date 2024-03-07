namespace VPExtensionManager.Models;

public class VPExtensionType(string name, string downloadExtension)
{
    public string Name { get; set; } = name;
    public string DownloadFileExtension { get; set; } = downloadExtension;

    public static readonly VPExtensionType Extension = new("Extension", RFXStrings.Zip);
    public static readonly VPExtensionType Script = new("Script", RFXStrings.Dll);

    public bool Equals(VPExtensionType other) => Name == other.Name;
}