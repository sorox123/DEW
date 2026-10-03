namespace DEW.Core;

public static class PortraitLinter
{
    public static List<LintFinding> Check(List<Token> tokens, int width, int height, int? frameSize = null)
    {
        var findings = new List<LintFinding>();
        int count = PortraitSheet.FrameCount(width, height, frameSize);

        foreach (var t in tokens)
        {
            if (t.Kind != SyntaxKind.Portrait) continue; //akip if syntax kind is not portrait

            if (!int .TryParse(t.RawText.Substring(1), out int index)) continue; //skip if portait is not a number

            if (index >= count) //if index is greater or equal to the count of frames, add a warning finding. Remember, index 0 is frame 1, so frame 10 should be index 9
            {
                findings.Add(new LintFinding
                {
                    Severity = LintSeverity.Warning, //severity type
                    Message = $"Frame {index} doesn't exist. The game will show frame 0 instead.", //message shown to user
                    RawText = t.RawText, //raw text that caused the finding
                    Position = t.Position //position of the token that caused the finding
                });
            }
        }
        return findings;
    }
}