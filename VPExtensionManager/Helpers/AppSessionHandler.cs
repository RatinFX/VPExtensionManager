using System.IO;
using System.IO.Pipes;
using System.Windows;

namespace VPExtensionManager.Helpers;

internal class AppSessionHandler
{
    public const string VPEM_PIPESERVER = @"VPEM-PIPE-F3052674-7283-4F25-97FD-FE47DBE337EA";
    public const string VPEM_MUTEX = @"VPEM-MUTEX-F3052674-7283-4F25-97FD-FE47DBE337EA";

    private static Mutex _mutex;
    private static bool createdNewMutex;

    private static NamedPipeServerStream _pipeServer;

    public static void HandleAppSession()
    {
        try
        {
            _mutex = new Mutex(false, VPEM_MUTEX, out createdNewMutex);

            if (createdNewMutex)
            {
                StartPipeServer();
            }
            else
            {
                // Another instance is already running
                NotifyRunningInstance();
                Application.Current.Shutdown(0);
            }
        }
        catch (Exception ex)
        {
            MessageBoxes.Error(ex, Properties.Resources.MessageBoxErrorSession);
            throw;
        }
    }

    private static void NotifyRunningInstance()
    {
        try
        {
            using var client = new NamedPipeClientStream(".", VPEM_PIPESERVER, PipeDirection.Out);
            using var writer = new StreamWriter(client);

            // Timeout in milliseconds
            client.Connect(500);

            writer.Write(Environment.GetCommandLineArgs().Skip(1).FirstOrDefault());
            writer.Flush();
        }
        catch (Exception ex)
        {
            MessageBoxes.Error(ex, Properties.Resources.MessageBoxErrorRunningInstanceNotification);
            throw;
        }
    }

    public static void StartPipeServer()
    {
        try
        {
            _pipeServer = new NamedPipeServerStream(VPEM_PIPESERVER, PipeDirection.InOut, 1, PipeTransmissionMode.Byte);
            _pipeServer.BeginWaitForConnection(WaitForConnectionCallback, null);
        }
        catch (Exception ex)
        {
            MessageBoxes.Error(ex, Properties.Resources.MessageBoxErrorPipeServer);
            throw;
        }
    }

    private static void WaitForConnectionCallback(IAsyncResult result)
    {
        try
        {
            _pipeServer.EndWaitForConnection(result);

            using var reader = new StreamReader(_pipeServer);
            var args = reader.ReadToEnd().Split(';');
            AppLinkHandler.HandleURL(args);

            _pipeServer.Close();
            StartPipeServer();
        }
        catch (Exception ex)
        {
            MessageBoxes.Error(ex, Properties.Resources.MessageBoxErrorWaitForConnection);
            throw;
        }
    }

    public static void StopPipeServer()
    {
        if (!createdNewMutex || _pipeServer is null)
            return;

        if (_pipeServer.IsConnected)
        {
            _pipeServer.WaitForPipeDrain();
            _pipeServer.Disconnect();
        }

        _pipeServer.Close();
        _pipeServer.Dispose();
    }
}
