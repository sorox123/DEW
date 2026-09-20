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

        public void Lookup_KnownPattern_ReturnsExpectedFriendlyName(string pattern, string expectedFriendlyName)
        {
            var entry = SyntaxRegistry.Lookup(pattern);

            Assert.NotNull(entry);
            Assert.Equal(expectedFriendlyName, entry!.FriendlyName);
        }

        [Theory]
        [InlineData("$q")]
        [InlineData("$c")]
        [InlineData("notarealpattern")]

        public void Lookup_UnknownPattern_ReturnsNull(string pattern)
        {
            var entry = SyntaxRegistry.Lookup(pattern);

            Assert.Null(entry);
        }
    }
}