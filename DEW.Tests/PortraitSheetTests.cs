using DEW.Core;
using Xunit;

namespace DEW.Tests
{
    public class PortraitSheetTests
    {
        [Theory]
        [InlineData(128, 320, 64, 10)] //vanilla style
        [InlineData(256, 640, 128, 10)] //HD at 2×
        [InlineData(512, 4096, 256, 32)] //RRRR-Style
        public void FrameCount_KnownSheetSizes_ReturnsExpectedCount(int width, int height, int frameSize, int expectedCount)
        {
            int portraitCount = PortraitSheet.FrameCount(width, height, frameSize);
            
            Assert.Equal(expectedCount, portraitCount);
        }

        [Theory]
        [InlineData(128, 320, 10)] //vanilla style
        [InlineData(512, 4096, 32)] //RRRR-style
        public void FrameCount_NoFrameSize_UsesHalfWidth(int width, int height, int expectedCount)
        {
            int count = PortraitSheet.FrameCount(width, height);

            Assert.Equal(expectedCount, count);
        }
    }
}