namespace DEW.Core
{
    //give each syntax marker a friendly name and plain-English description
    public class RegistryEntry
    {
        public string Pattern { get; set; } = string.Empty;
        public string FriendlyName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public SyntaxKind Kind { get; set; }
        public string[] Arguments { get; set; } = System.Array.Empty<string>();
        public string? RequiresModId { get; set; }
    }
}