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


        
        

        [Theory]
        [InlineData("$action AddMoney 500", "Action Handler")]
        [InlineData("$c 0.9", "Chance Split")]
        [InlineData("$c0.9", "Chance Split")]
        [InlineData("%revealtaste:Abigail:(O)66", "Reveal Item Preference")]
        [InlineData("%revealtaste:Alex:201%revealtaste:Alex:212", "Reveal Item Preference")]
        [InlineData("[128 130 72]", "Item Pool")]
        public void Resolve_ArgCommand_FallsBackToCommandName(string rawText, string expectedFriendlyName)
        {
            var entry = SyntaxRegistry.Resolve(rawText);

            Assert.NotNull(entry);
            Assert.Equal(expectedFriendlyName, entry!.FriendlyName);
        }

        //split these two out since this test is specifically for $1 behavior
        [Theory]
        [InlineData("$1 abbyGift", "Say Once")]
        [InlineData("$1", "Portrait: Happy")]
        public void Resolve_DollarOne_ArgDecidesSayOnceOrPortrait(string rawText, string expectedFriendlyName)
        {
            var entry = SyntaxRegistry.Resolve(rawText);

            Assert.NotNull(entry);
            Assert.Equal(expectedFriendlyName, entry!.FriendlyName);
        }

        [Theory]
        [InlineData("$12", "Portrait: Frame 12")]
        [InlineData("$7", "Portrait: Frame 7")]
        public void Resolve_NumberedPortrait_FillsFrameNumber(string rawText, string expectedFriendlyName)
        {
            var entry = SyntaxRegistry.Resolve(rawText);

            Assert.NotNull(entry);
            Assert.Equal(expectedFriendlyName, entry!.FriendlyName);
        }

        [Fact]
        public void Resolve_LoneDollar_ReturnsNull()
        {
            var entry = SyntaxRegistry.Resolve("$"); //set entry to the result of calling Resolve on "$"

            Assert.Null(entry); //tests to see if entry is null or not
        }
    }
}