namespace OkapiLauncher.Core.Models.Apps;

public enum CommandLineInterface
{
    None,
    /// <summary>
    /// Just the program path, with no keyword
    /// ...Studio.exe &lt;program&gt;
    /// </summary>
    Studio,
    /// <summary>
    /// Program path after keyword.
    /// Optionally:
    /// <list type="bullet">
    ///     <item>
    ///         <term>log-level</term>
    ///         <description>int</description>
    ///     </item>
    ///     <item>
    ///         <term>console</term>
    ///         <description>enum</description>
    ///     </item>
    ///     <item>
    ///         <term>auto-close</term>
    ///         <description>switch</description>
    ///     </item>
    ///     <item>
    ///         <term>log-pipe</term>
    ///         <description>string</description>
    ///     </item>
    /// </list>
    /// <code>
    /// ...Executor.exe --program &lt;program&gt; [--log-level {trace|debug|pass|info|warning|warn|error|fatal|off}] [--console] [--auto-close] [--log-pipe &lt;log-pipe&gt;] 
    /// </code>
    /// </summary>
    Executor,
}
