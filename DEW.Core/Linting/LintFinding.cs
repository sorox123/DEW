namespace DEW.Core;

public class LintFinding
{
    public LintSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public string RawText { get; set; } = string.Empty;
    public int Position { get; set; }
}