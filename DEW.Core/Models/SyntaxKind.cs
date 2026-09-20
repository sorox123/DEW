namespace DEW.Core
{
    public enum SyntaxKind
    {
        Text,           //plain dialogue text
        Inline,         //embedded in running text (eg: %pet to mark pet name)
        LineLevel,      //controls box itself, portrait tags also go here (eg. $b to indicate new dialog box, $e to indicate next time spoken to.)
        Structural,     //owns child text/branches
        Unknown         //unrecognized syntax -- opaque pass-through
    }
}