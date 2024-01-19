namespace VPExtensionManager.Interfaces.Activation;

public interface IActivationHandler
{
    bool CanHandle();

    Task HandleAsync();
}
