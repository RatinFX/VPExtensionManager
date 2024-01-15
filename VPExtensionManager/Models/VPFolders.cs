using System.IO;

namespace VPExtensionManager.Models;

/// <summary>
/// These paths are from the official VEGASScriptFAQ.html
/// https://www.vegascreativesoftware.info/us/forum/vegas-pro-scripting-faqs-resources--104563/
/// 
/// Note that these Paths may only fully support MAGIX Vegas (14 and above)
/// as I could not find an older (Sony) version of the FAQ
/// </summary>
public class VPFolders
{
    private static string GetVPFolderPath(Environment.SpecialFolder specialFolder, string folder, int vp = 0)
    {
        var basePath = Path.Combine(
            Environment.GetFolderPath(specialFolder),
            RFXStrings.VegasPro
        );

        var vpFolder = vp == 0 ? folder : Path.Combine($"{vp}.0", folder);

        return Path.Combine(basePath, vpFolder);
    }

    // Script
    // > 1.10: How do I add a script to the Scripting menu?
    // 
    // C:\Users\<username>\Documents\Vegas Script Menu\ 
    // C:\Users\<username>\AppData\Local\Vegas Pro\17.0\Script Menu\
    // C:\Users\<username>\AppData\Roaming\Vegas Pro\17.0\Script Menu\
    // C:\ProgramData\Vegas Pro\17.0\Script Menu\
    // C:\Users\<username>\AppData\Local\Vegas Pro\Script Menu\
    // C:\Users\<username>\AppData\Roaming\Vegas Pro\Script Menu\
    // C:\ProgramData\Vegas Pro\Script Menu\

    public static string ScriptMenu = "Script Menu";
    public static string VegasScriptMenu = $"Vegas {ScriptMenu}";

    public static string ScriptDocuments
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), VegasScriptMenu);

    public static string ScriptLocalAppData(int vp = 0)
        => GetVPFolderPath(Environment.SpecialFolder.LocalApplicationData, ScriptMenu, vp);

    public static string ScriptRoamingAppData(int vp = 0)
        => GetVPFolderPath(Environment.SpecialFolder.ApplicationData, ScriptMenu, vp);

    public static string ScriptProgramData(int vp = 0)
        => GetVPFolderPath(Environment.SpecialFolder.CommonApplicationData, ScriptMenu, vp);

    public static List<string> ScriptLocalAppData(params int[] vps) => vps.Select(ScriptLocalAppData).ToList();
    public static List<string> ScriptRoamingAppData(params int[] vps) => vps.Select(ScriptRoamingAppData).ToList();
    public static List<string> ScriptProgramData(params int[] vps) => vps.Select(ScriptProgramData).ToList();

    public static List<string> ScriptFolders(params int[] vps) => [
        ScriptDocuments,
        ScriptLocalAppData(),
        .. ScriptLocalAppData(vps),
        ScriptRoamingAppData(),
        .. ScriptRoamingAppData(vps),
        ScriptProgramData(),
        .. ScriptProgramData(vps),
    ];

    // Extension
    // > 4.2: How do I install an application extension?
    // 
    // C:\Users\<username>\Documents\Vegas Application Extensions\  
    // C:\Users\<username>\AppData\Local\VEGAS Pro\17.0\Application Extensions\ 
    // C:\Users\<username>\AppData\Roaming\VEGAS Pro\17.0\Application Extensions\ 
    // C:\ProgramData\Vegas Pro\17.0\Application Extensions\  
    // C:\Users\<username>\AppData\Local\Vegas Pro\Application Extensions\
    // C:\Users\<username>\AppData\Roaming\Vegas Pro\Application Extensions\
    // C:\ProgramData\Vegas Pro\Application Extensions\

    public static string ApplicationExtensions = "Application Extensions";
    public static string VegasApplicationExtensions = $"Vegas {ApplicationExtensions}";

    public static string ExtensionDocuments
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), VegasApplicationExtensions);

    public static string ExtensionLocalAppData(int vp = 0)
        => GetVPFolderPath(Environment.SpecialFolder.LocalApplicationData, ApplicationExtensions, vp);

    public static string ExtensionRoamingAppData(int vp = 0)
        => GetVPFolderPath(Environment.SpecialFolder.ApplicationData, ApplicationExtensions, vp);

    public static string ExtensionProgramData(int vp = 0)
        => GetVPFolderPath(Environment.SpecialFolder.CommonApplicationData, ApplicationExtensions, vp);

    public static List<string> ExtensionLocalAppData(params int[] vps) => vps.Select(ExtensionLocalAppData).ToList();
    public static List<string> ExtensionRoamingAppData(params int[] vps) => vps.Select(ExtensionRoamingAppData).ToList();
    public static List<string> ExtensionProgramData(params int[] vps) => vps.Select(ExtensionProgramData).ToList();

    public static List<string> ExtensionFolders(int[] vps) => [
        ExtensionDocuments,
        ExtensionLocalAppData(),
        .. ExtensionLocalAppData(vps),
        ExtensionRoamingAppData(),
        .. ExtensionRoamingAppData(vps),
        ExtensionProgramData(),
        .. ExtensionProgramData(vps),
    ];
}