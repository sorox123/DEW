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
        public void Tokenize_ItemPool_Closed_IsOneStructuralToken()
        {
            string raw ="[128 130 72]#$b#Hi";

            var tokens = DialogueTokenizer.Tokenize(raw);

            Assert.Equal(SyntaxKind.Structural, tokens[0].Kind); //checks to see if token[0].kind is structural
            Assert.Equal("[128 130 72]", tokens[0].RawText); //checks to see if token[0] = [128 130 72]
            Assert.Equal("#", tokens[1].RawText); //checks to see if token[1] = #
        }

        [Fact]
        public void Tokenize_ItemPool_Open_IsUnknownAndStopsAtHash()
        {
            string raw = "[128 130 72#$b#Hi";

            var tokens = DialogueTokenizer.Tokenize(raw);

            Assert.Equal(SyntaxKind.Unknown, tokens[0].Kind); //checks to see if this raw text matches Unknown tokenkind
            Assert.Equal("[128 130 72", tokens[0].RawText); //checks to see if this raw text is its own token
            Assert.Equal("#", tokens[1].RawText); //checks to make sure # is its own token
        }

        [Fact]
        public void Tokenize_RevealTaste_SingleStructuralToken()
        {
            string raw = "%revealtaste:Abigail:(O)66#$b#Hi.";

            var tokens = DialogueTokenizer.Tokenize(raw);

            Assert.Equal(SyntaxKind.Structural, tokens[0].Kind);
            Assert.Equal("%revealtaste:Abigail:(O)66", tokens[0].RawText);
            Assert.Equal("#", tokens[1].RawText);
        }

        [Fact]
        public void Tokenize_RevealTaste_Chained_SplitsIntoTwoTokens()
        {
            string raw = ("%revealtaste:Abigail:(O)66%revealtaste:Abigail:(O)72#Hi");

            var tokens = DialogueTokenizer.Tokenize(raw);

            Assert.Equal("%revealtaste:Abigail:(O)66", tokens[0].RawText); //checks to see if the %revealtaste:Abigail:66 is being tokenized properly
            Assert.Equal(SyntaxKind.Structural, tokens[1].Kind); //checks to see if the 2nd token matches the syntaxkind (both tokens should match this kind, this is the only test we need)
            Assert.Equal("%revealtaste:Abigail:(O)72", tokens[1].RawText); //checks to see if %revealtaste:Abigail:72 is being tokenized properly
            Assert.Equal("#", tokens[2].RawText); //checks to make sure # is its own token
        }

        [Fact]
        public void Tokenize_Portrait_MultiDigit_IsOneToken()
        {
            string raw = "Hi.$12";

            var tokens = DialogueTokenizer.Tokenize(raw);

            Assert.Equal("$12", tokens[1].RawText);
            Assert.Equal(SyntaxKind.Portrait, tokens[1].Kind);
        }

        [Fact]
        public void Tokenize_SayOnce_AtPieceStart_IsStructuralCommand()
        {
            string raw = "$1 abbyGift#Here's a gift!";

            var tokens = DialogueTokenizer.Tokenize(raw);

            Assert.Equal("$1 abbyGift", tokens[0].RawText); //checks to see if $1 abbyGift is its own token
            Assert.Equal(SyntaxKind.Structural, tokens[0].Kind); //checks to see if the SyntaxKind of the token (if it passes) is Structural
            Assert.Equal("#", tokens[1].RawText); //checks to see if # is its own token
        }

        [Fact]
        public void Tokenize_DollarOne_AfterText_IsPortrait()
        {
            string raw = "I'm so happy!$1";

            var tokens = DialogueTokenizer.Tokenize(raw);

            Assert.Equal("$1", tokens[1].RawText); //checks to see if $1 is its own token
            Assert.Equal(SyntaxKind.Portrait, tokens[1].Kind); //checks to see if SyntaxKind of the token (if it passes) is Portrait
        }

        [Fact]
        public void Tokenize_Portrait_MultiDigit_AtPieceStart_IsNotSayOnce()
        {
            string raw = "$12#Hi";

            var tokens = DialogueTokenizer.Tokenize(raw);

            Assert.Equal("$12", tokens[0].RawText);
            Assert.Equal(SyntaxKind.Portrait, tokens[0].Kind);
        }

        [Fact]
        public void Tokenize_GenderSplit_ProducesTextSplitterText()
        {
            //arrange
            var input = "You look nice.^You look pretty.";

            //Act
            var tokens = DialogueTokenizer.Tokenize(input);

            //Asserts
            Assert.Equal(3, tokens.Count);

            Assert.Equal(SyntaxKind.Text, tokens[0].Kind);
            Assert.Equal("You look nice.", tokens[0].RawText);

            Assert.Equal(SyntaxKind.Splitter, tokens[1].Kind);
            Assert.Equal("^", tokens[1].RawText);

            Assert.Equal(SyntaxKind.Text, tokens[2].Kind);
            Assert.Equal("You look pretty.", tokens[2].RawText);
        }

        [Fact]
        public void Tokenize_PortraitA_StillPortraitAfterActionAdded()
        {
            var tokens = DialogueTokenizer.Tokenize("Hmph.$a");

            Assert.Equal(2, tokens.Count);
            Assert.Equal(SyntaxKind.Portrait, tokens[1].Kind);
            Assert.Equal("$a", tokens[1].RawText);
        }

        [Fact]
        public void Tokenize_PortraitAfterText_HasCorrectPosition()
        {
            var tokens = DialogueTokenizer.Tokenize("Hi!$999");

            Assert.Equal(0, tokens[0].Position); //first token should start at "H" for "Hi!"
            Assert.Equal(3, tokens[1].Position); //second token should start at "$" for "$999"
        }
    }
}