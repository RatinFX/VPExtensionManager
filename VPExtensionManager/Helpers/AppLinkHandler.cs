using Microsoft.Extensions.Options;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;
using VPExtensionManager.Models;

namespace VPExtensionManager.Helpers;

internal class AppLinkHandler
{
    public delegate void NavigateToExtension(string extension);
    public static event NavigateToExtension NavigateToExtensionHandler;

    private static bool AskedRegistryQuestion()
    {
        if (AppProperties.Get(AppProperties.AskedRegistryQuestion, out string askedRegistryQuestion))
        {
            return bool.TryParse(askedRegistryQuestion, out bool res) && res;
        }

        return false;
    }

    public static void HandleRegistryEntry()
    {
        try
        {
            var processPath = Environment.ProcessPath;
            if (processPath == null)
                return;

            using var key = Registry.ClassesRoot.OpenSubKey(RFXStrings.VPEM);

            if (key != null && AskedRegistryQuestion())
                return;

            var res = MessageBoxes.YesNo(
                Properties.Resources.MessageBoxURLHandlerContent,
                Properties.Resources.MessageBoxURLHandlerTitle
            );

            if (res == MessageBoxResult.Yes)
            {
                AddURLHandlerRegistry();
            }

            AppProperties.Set(AppProperties.AskedRegistryQuestion, true.ToString());
        }
        catch (Exception ex)
        {
            MessageBoxes.Error(ex, Properties.Resources.MessageBoxErrorCreatingRegistryKey);
            throw;
        }
    }

    public static void AddURLHandlerRegistry()
    {
        var processPath = Environment.ProcessPath;
        var doubleBackslashPath = processPath.Replace("\\", "\\\\");

        var regContent =
$@"Windows Registry Editor Version 5.00

[HKEY_CLASSES_ROOT\vpem]
@=""VPExtensionManager url handler by RatinFX""
""URL Protocol""=""""

[HKEY_CLASSES_ROOT\vpem\DefaultIcon]
@=""{doubleBackslashPath},1""

[HKEY_CLASSES_ROOT\vpem\shell]

[HKEY_CLASSES_ROOT\vpem\shell\open]

[HKEY_CLASSES_ROOT\vpem\shell\open\command]
@=""\""{doubleBackslashPath}\"" \""%1\""""";

        var filePath = Path.Combine(Path.GetDirectoryName(processPath), "RegisterURLHandler.reg");
        File.WriteAllText(filePath, regContent);

        var psi = new ProcessStartInfo
        {
            FileName = "regedit.exe",
            Arguments = $@"/s ""{filePath}""",
            Verb = "runas",
            UseShellExecute = true
        };

        Process.Start(psi);
    }

    public static void HandleURL(string[] args)
    {
        try
        {
            if (args.Length == 0)
                return;

            var link = args[0].Trim().ToLower()
                .Replace(RFXStrings.VPEM_PROTOCOL, "");

            if (string.IsNullOrEmpty(link))
                return;

            if (link.EndsWith("/"))
                link = link.Replace("/", "");

            Application.Current.Dispatcher.Invoke(() =>
            {
                NavigateToExtensionHandler?.Invoke(link);
            });
        }
        catch (Exception ex)
        {
            MessageBoxes.Error(ex, Properties.Resources.MessageBoxErrorURLHandler);
            throw;
        }
    }
}
