namespace VPExtensionManager.Models;

public class RFXStrings
{
    // General
    public const string RatinFX = "RatinFX";
    public const string VegasPro = "VEGAS Pro";
    public const string VPExtensionManager = "VPExtensionManager";

    public const string VPEM = "vpem";
    public const string VPEM_PROTOCOL = $"{VPEM}://";

    public const string Zip = ".zip";
    public const string Dll = ".dll";

    // Extensions
    public const string VPConsole = "VPConsole";
    public const string VegasProFlow = "VegasProFlow";
    public const string ShortenExtendMedia = "ShortenExtendMedia";
    public const string CustomFades = "CustomFades";

    // Dependencies
    public const string VegasProData = "VegasProData.dll";
    public const string RatinFXVP = "RatinFX.VP.dll";
    public const string NewtonsoftJson = "Newtonsoft.Json.dll";
    public const string Octokit = "Octokit.dll";
    public const string WindowsInput = "WindowsInput.dll";

    // Icons
    public const string VPFlowIcon = "VPFlow.png";
    public const string VPConsoleIcon = "VPConsole.png";

    public static readonly List<string> VegasProFlowRefs = [VegasProData, RatinFXVP, Octokit, NewtonsoftJson, VPFlowIcon];
    public static readonly List<string> VPConsoleRefs = [VegasProData, RatinFXVP, Octokit, NewtonsoftJson, WindowsInput, VPConsoleIcon];
}
