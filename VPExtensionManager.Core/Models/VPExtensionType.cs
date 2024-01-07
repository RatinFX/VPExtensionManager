namespace VPExtensionManager.Core.Models;

public class VPExtensionType(string name, string extension)
{
    public string Name { get; set; } = name;
    public string Extension { get; set; } = extension;

    public static VPExtensionType Zip = new("Extension", ".zip");
    public static VPExtensionType Dll = new("Script", ".dll");
}