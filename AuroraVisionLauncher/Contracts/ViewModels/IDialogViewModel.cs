
namespace AuroraVisionLauncher.Contracts.ViewModels;

public interface IDialogViewModel
{
    Func<Task> CloseDialog { get; }

    Task WaitForExit();
}