using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace VPExtensionManager.Updater;

internal class Program
{
    private const string vpem_updater_mutex = @"VPEM-UPDATER-MUTEX-F3052674-7283-4F25-97FD-FE47DBE337EA";

    private static Mutex _mutex;
    private static bool createdNewMutex;

    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("[ VPEM updater ]");

            _mutex = new Mutex(false, vpem_updater_mutex, out createdNewMutex);

            if (!createdNewMutex)
            {
                ExitApp("This should only run once.");
            }

            Console.WriteLine($"-- Sleeping for 1s to make sure VPEM is closed");
            Thread.Sleep(1000);
            Console.WriteLine($"-- Good morning :)");

            Console.WriteLine($"-- args: {string.Join(", ", args)}");

            if (args.Length < 3)
            {
                ExitApp("Did not have enough arguments: releaseLink downloadPath installPath");
            }

            // GitHub Release link
            var releaseLink = args[0];
            Console.WriteLine($"\n+ Release link: {releaseLink}");

            if (releaseLink.Length < 10)
            {
                ExitApp("Release link was wrong.");
            }

            // Download path
            var downloadPath = args[1];
            Console.WriteLine($"\n+ Download path: {downloadPath}");

            if (!Directory.Exists(downloadPath))
            {
                ExitApp("Download path did not exist.");
            }

            // Install path
            var installPath = args[2];
            Console.WriteLine($"\n+ Install path: {installPath}");

            if (!Directory.Exists(installPath))
            {
                ExitApp("Install path did not exist.");
            }

            // Download
            var filePath = Path.Combine(downloadPath, "VPExtensionManager.zip");
            Console.WriteLine($"\n+ Final .zip file path: {filePath}");

            Console.WriteLine("\n> Downloading files...");
            using (var client = new HttpClient())
            {
                var response = client.GetByteArrayAsync(releaseLink).Result;

                Console.WriteLine("\n> Writing Downloaded files...");
                File.WriteAllBytes(filePath, response);
            }

            // Extract
            Console.WriteLine("\n> Extracting files...");
            ZipFile.ExtractToDirectory(filePath, installPath, true);

            // Start VPEM
            Console.WriteLine("\n> Start VPEM...");
            Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(installPath, "VPExtensionManager.exe"),
                UseShellExecute = true,
            });

            // Close
            Console.WriteLine("\n[ Successfully updated to the latest version ]");
            //Console.ReadKey();
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n(!) {ex.Message}");
            throw;
        }
    }

    private static void ExitApp(string error)
    {
        Console.WriteLine($"\n(!) {error}");
        Console.WriteLine($"\nPress any key to exit.");
        Console.ReadKey();
        Environment.Exit(0);
    }
}