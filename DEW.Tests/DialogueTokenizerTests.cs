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
    }
}