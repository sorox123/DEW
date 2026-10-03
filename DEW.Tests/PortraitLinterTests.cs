namespace DEW.Core;

public class PortraitLinterTests
{
    [Fact]
    public void Check_PortraitBeyondSheet_ReturnsWarning()
    {
        var tokens = DialogueTokenizer.Tokenize("Hi!$999");

        var findings = PortraitLinter.Check(tokens, 128, 320); //checks to see if portrait is beyond the sheet size of 128x320

        var finding = Assert.Single(findings); //checks to see if there is only one finding in the list of findings
        Assert.Equal(LintSeverity.Warning, finding.Severity); //checks to see if the found severity matches expected Linter severity
        Assert.Equal("$999", finding.RawText); //checks to see if the finding matches the expected raw text of the warning token
        Assert.Equal(3, finding.Position); //checks to see if the finding position matches the expected position of the warning token
    }
}