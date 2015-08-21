using WebsiteRipper.Helpers;
using Xunit;

namespace WebsiteRipper.Tests.Helpers
{
    public class HelperTests
    {

        [Theory]
        [InlineData("a", "a")]
        [InlineData(" a ", "a")]
        [InlineData("   a   ", "a")]
        [InlineData("a b", "a|b")]
        [InlineData("a   b", "a|b")]
        [InlineData("a b   c", "a|b|c")]
        [InlineData(" ab cd   ef   ", "ab|cd|ef")]
        public void Split_SpaceSeparatedTokens_ReturnsExpectedTokens(string list, string tokens)
        {
            var expected = tokens.Split('|');
            var actual = Helper.SplitSpaceSeparatedTokens(list);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("a   b", "a   b")]
        [InlineData(" a   b ", "a   b")]
        [InlineData("   a   b   ", "a   b")]
        [InlineData(",a   b,", "a   b")]
        [InlineData(" , a   b , ", "a   b")]
        [InlineData("   ,   a   b   ,   ", "a   b")]
        [InlineData("a   b,c   d", "a   b|c   d")]
        [InlineData("a   b,,,c   d", "a   b|c   d")]
        [InlineData("a   b   ,   c   d", "a   b|c   d")]
        [InlineData("a   b,,   ,c   d", "a   b|c   d")]
        [InlineData("a   b,c   d,,,e   f", "a   b|c   d|e   f")]
        [InlineData("a   b,c   d   ,   e   f", "a   b|c   d|e   f")]
        [InlineData("a   b,c   d,,   ,e   f", "a   b|c   d|e   f")]
        [InlineData(" ab ,cd,, ,   ,   e   f   ", "ab|cd|e   f")]
        public void Split_CommaSeparatedTokens_ReturnsExpectedTokens(string list, string tokens)
        {
            var expected = tokens.Split('|');
            var actual = Helper.SplitCommaSeparatedTokens(list);
            Assert.Equal(expected, actual);
        }
    }
}
