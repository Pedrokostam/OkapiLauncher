using Ookii.CommandLine;
using Ookii.CommandLine.Validation;

namespace OkapiLauncher.Commandline;

public class FileValidatorAttribute : ArgumentValidationWithHelpAttribute
{
    public override string GetErrorMessage(CommandLineArgument argument, object? value)
    {
        return $"Path \"{value ?? "<empty>"}\" does not lead to a valid file or folder.";
    }
    public override bool IsValidPostConversion(CommandLineArgument argument, object? value)
    {
        return value is string s && (System.IO.File.Exists(s) || System.IO.Directory.Exists(s));
    }
    protected override string GetUsageHelpCore(CommandLineArgument argument)
    {
        return "Must lead to an existing file or folder";
    }
}