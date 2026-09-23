using DEW.Core;
using System.Linq;
using Xunit;

namespace DEW.Tests
{
    public class DialogueTokenizerTests
    {
        [Theory]
        [InlineData("Hi @, how are you?")]
        [InlineData("Nice day!$h")]
        [InlineData("First box.$bSecond box.$e")]
        [InlineData("I love my %pet so much!")]
        [InlineData("%A plain narration line.")]
        [InlineData("Ahoy there ${lad^lass}$!")]
        [InlineData("$c 0.9#Nice day.#Ugh, weather.")]
        [InlineData("$q 101/102 fish_followup#Want to come?")]
        public void Tokenize_RoundTrips_ToOriginalString(string original)
        {
            var tokens = DialogueTokenizer.Tokenize(original);

            var rebuilt = string.Concat(tokens.Select(t => t.RawText));

            Assert.Equal(original, rebuilt);
        }

        [Theory]
        [InlineData("$c 0.9#A#B", "$c 0.9")]
        [InlineData("$c0.9#A#B", "$c0.9")]
        [InlineData("$c 0.75#A#B", "$c 0.75")]
        public void Tokenize_ChanceSplit_CapturesProbabilityInRawText(string input, string expectedRaw)
        {
            var tokens = DialogueTokenizer.Tokenize(input);

            Assert.NotEmpty(tokens);
            Assert.Equal(SyntaxKind.Structural, tokens[0].Kind);
            Assert.Equal(expectedRaw, tokens[0].RawText);
        }

        [Theory]
        [InlineData("$action AddMoney 500", "$action AddMoney 500")]
        [InlineData("$action AddMoney 500#Here you go!", "$action AddMoney 500")]
        [InlineData("$t cc_Complete 7", "$t cc_Complete 7")]
        [InlineData("$v 1234 false false#Come with me.", "$v 1234 false false")]
        public void Tokenize_ArgCommand_CapturesArgsUntilHash(string input, string expectedRaw)
        {
            var tokens = DialogueTokenizer.Tokenize(input);

            Assert.NotEmpty(tokens);
            Assert.Equal(SyntaxKind.Structural, tokens[0].Kind);
            Assert.Equal(expectedRaw, tokens[0].RawText);
        }
    }
}