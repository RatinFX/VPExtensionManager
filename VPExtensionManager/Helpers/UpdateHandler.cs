using System.Diagnostics;
using System.IO;
using VPExtensionManager.Models;

namespace VPExtensionManager.Helpers;

internal class UpdateHandler
{
    public static void StartUpdate(string downloadPath)
    {
        var res = MessageBoxes.YesNo(
            Properties.Resources.MessageBoxYesNoUpdateVPEMContent,
            Properties.Resources.MessageBoxYesNoUpdateVPEMTitle
        );

        if (res != System.Windows.MessageBoxResult.Yes)
        {
            return;
        }

        var success = AppProperties.Get(AppProperties.LatestVersionURL, out string releaseLink);
        if (!success)
        {
            MessageBoxes.Error(content: Properties.Resources.ErrorLatestVersionURLNotFound);
            return;
        }

        var installPath = Environment.CurrentDirectory;
        var fileName = Path.Combine(installPath, "VPExtensionManager.Updater.exe");

        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = @$"""{releaseLink}"" ""{downloadPath}"" ""{installPath}""",
            Verb = "runas",
            UseShellExecute = true
        };

        Process.Start(psi);

        App.Current.Shutdown(0);
    }
}
