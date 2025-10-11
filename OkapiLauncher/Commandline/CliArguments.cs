using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ookii.CommandLine;
using Ookii.CommandLine.Validation;

namespace OkapiLauncher.Commandline;
[GeneratedParser]
[ParseOptions(CaseSensitive = false, ArgumentNameTransform = NameTransform.DashCase)]

public partial class CliArguments
{
    [CommandLineArgument(IsPositional = true)]
    [FileValidator]
    [Description("File path pointing to a vision project to load upon launching the application.")]
    public string? File { get; set; }
    [CommandLineArgument(IsShort = true)]
    [Alias("load", IsHidden = false)]
    [Requires(nameof(File),IncludeInUsageHelp =false)]
    [Description("If specified, automatically loads the file upon launch. Requires a project path to be specified.")]
    public bool AutoLoad { get; set; }
    public static CliArguments? CustomParse()
    {
        var args = Environment.GetCommandLineArgs()[1..];
        using var std = new StringWriter();
        using var err = new StringWriter();
        var cliParsed = CliArguments.Parse(args, new Ookii.CommandLine.ParseOptions()
        {
            UsageWriter = new Ookii.CommandLine.UsageWriter(new(std, 40, false, false), Ookii.CommandLine.TriState.False),
            AutoHelpArgument = true,
            AutoVersionArgument = false,
            Error = err,
            AutoPrefixAliases = true,
            ShowUsageOnError = Ookii.CommandLine.UsageHelpRequest.Full,
            Mode = ParsingMode.Default,
        });
        var errorStr = err.ToString();
        var stdStr = std.ToString();
        if (errorStr.Length > 0)
        {
            MessageBox.Show(errorStr, "Invalid arguments", MessageBoxButton.OK);
            return null;
        }
        if (errorStr.Length == 0 && stdStr.Length > 0)
        {
            MessageBox.Show(stdStr, "Okapi Launcher Usage", MessageBoxButton.OK);
            return null;
        }
        if (cliParsed is null)
        {
            MessageBox.Show($"Could not parse arguments: {string.Join(' ', args)}", "Parsing error", MessageBoxButton.OK);
            return null;
        }
        return cliParsed!;
    }
}
