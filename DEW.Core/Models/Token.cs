namespace DEW.Core
{
    public class Token
    {
        public SyntaxKind Kind { get; set; }
        public string RawText { get; set; } = string.Empty;
        public int Position { get; set; }
    }
}