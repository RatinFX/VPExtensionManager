namespace VPExtensionManager.Core.Models;

public class VPExtensionType(string name, string fileExtension, string downloadExtension)
{
    public string Name { get; set; } = name;
    public string FileExtension { get; set; } = fileExtension;
    public string DownloadFileExtension { get; set; } = downloadExtension;

    public static VPExtensionType Extension = new("Extension", RFXStrings.Dll, RFXStrings.Zip);
    public static VPExtensionType Script = new("Script", RFXStrings.Dll, RFXStrings.Dll);

    public bool Equals(VPExtensionType other) => Name == other.Name;
}