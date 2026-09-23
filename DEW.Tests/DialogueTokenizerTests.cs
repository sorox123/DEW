using DEW.Core;
using System.Linq;
using Xunit;

namespace DEW.Tests
{
    public class DialogueTokenizerTests
    {
        [Theory] //test that uses the following inputs
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
            //sets variable
            var tokens = DialogueTokenizer.Tokenize(original);

            //splits variable into tokens
            var rebuilt = string.Concat(tokens.Select(t => t.RawText));
            
            //rebuilds tokens and checks to make sure they get built back to original
            Assert.Equal(original, rebuilt);
        }

        [Theory]
        [InlineData("$c 0.9#A#B", "$c 0.9")]
        [InlineData("$c0.9#A#B", "$c0.9")]
        [InlineData("$c 0.75#A#B", "$c 0.75")]
        public void Tokenize_ChanceSplit_CapturesProbabilityInRawText(string input, string expectedRaw)
        {
            //sets variable
            var tokens = DialogueTokenizer.Tokenize(input);

            //checks to make sure there are tokens
            Assert.NotEmpty(tokens);
            //checks to make sure first tokens are matched to kind Structural
            Assert.Equal(SyntaxKind.Structural, tokens[0].Kind);
            //checks to make sure first tokens match raw text
            Assert.Equal(expectedRaw, tokens[0].RawText);
        }

        [Theory]
        [InlineData("$action AddMoney 500", "$action AddMoney 500")]
        [InlineData("$action AddMoney 500#Here you go!", "$action AddMoney 500")]
        [InlineData("$t cc_Complete 7", "$t cc_Complete 7")]
        [InlineData("$v 1234 false false#Come with me.", "$v 1234 false false")]
        public void Tokenize_ArgCommand_CapturesArgsUntilHash(string input, string expectedRaw)
        {
            //sets up variable
            var tokens = DialogueTokenizer.Tokenize(input);

            Assert.NotEmpty(tokens); //passes if tokens are added
            Assert.Equal(SyntaxKind.Structural, tokens[0].Kind); //passes if first token matches kind Structural
            Assert.Equal(expectedRaw, tokens[0].RawText); //passes if the whole line of text is tokenized
        }

        [Fact] //test with no inputs
        public void Tokenize_LongerAfterArgCommand_IsUnknown() //what is being tested and what should happen
        {
            //set up input string
            string raw = "$tomorrow";

            //try and tokenize the string
            var tokens = DialogueTokenizer.Tokenize(raw);

            //check the results
            Assert.Single(tokens); //passes if list is one item
            Assert.Equal(SyntaxKind.Unknown, tokens[0].Kind); //token should be unknown
            Assert.Equal("$tomorrow", tokens[0].RawText); //checks to make sure word stayed together
        }

        [Fact]
        public void Tokenize_RevealTaste_SingleStructuralToken()
        {
            string raw = "%revealtaste:Abigail:(O)66#$b#Hi";

            var tokens = DialogueTokenizer.Tokenize(raw);

            Assert.Equal(SyntaxKind.Structural, tokens[0].Kind);
            Assert.Equal("%revealtaste:Abigail:(O)66", tokens[0].RawText);
            Assert.Equal("#", tokens[1].RawText);
        }
    }
}