using DEW.Core;
using Xunit;

namespace DEW.Tests
{
    public class SyntaxRegistryTests
    {
        [Theory]
        [InlineData("$b", "Box Break")]
        [InlineData("$e", "End Conversation")]
        [InlineData("$k", "Kill")]
        [InlineData("#", "Separator")]
        [InlineData("@", "Farmer Name")]
        [InlineData("%", "Narration Box")]
        [InlineData("$h", "Portrait: Happy")]
        [InlineData("$1", "Portrait: Happy")]
        [InlineData("%pet", "Pet Name")]
        [InlineData("$c", "Chance Split")]
        [InlineData("^", "Gender Split")]

        public void Lookup_KnownPattern_ReturnsExpectedFriendlyName(string pattern, string expectedFriendlyName)
        {
            var entry = SyntaxRegistry.Lookup(pattern);

            Assert.NotNull(entry);
            Assert.Equal(expectedFriendlyName, entry!.FriendlyName);
        }

        [Theory]
        [InlineData("$q")]
        [InlineData("notarealpattern")]
        [InlineData("$c 0.9")]
        
        public void Lookup_UnknownPattern_ReturnsNull(string pattern)
        {
            var entry = SyntaxRegistry.Lookup(pattern);

            Assert.Null(entry);
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

        [Theory]
        [InlineData("$action AddMoney 500", "Action Handler")]
        [InlineData("$c 0.9", "Chance Split")]
        [InlineData("$c0.9", "Chance Split")]
        [InlineData("%revealtaste:Abigail:(O)66", "Reveal Item Preference")]
        public void Resolve_ArgCommand_FallsBackToCommandName(string rawText, string expectedFriendlyName)
        {
            var entry = SyntaxRegistry.Resolve(rawText);

            Assert.NotNull(entry);
            Assert.Equal(expectedFriendlyName, entry!.FriendlyName);
        }
    }
}