
namespace AuroraVisionLauncher.Contracts.ViewModels;

public interface IDialogViewModel
{
    //Func<Task> CloseDialog { get; }

    Task WaitForExit();
}

public interface IDialogViewModel<T>:IDialogViewModel
{
    //Func<Task> CloseDialog { get; }

    new Task<T> WaitForExit();
}

