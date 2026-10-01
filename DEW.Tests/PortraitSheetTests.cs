using DEW.Core;
using Xunit;

namespace DEW.Tests
{
    public class PortraitSheetTests
    {
        [Theory]
        [InlineData(128, 320, 64, 10)] //vanilla Abigail
        [InlineData(256, 640, 128, 10)] //HD at 2×
        [InlineData(512, 4096, 256, 32)] //RRRR-Style
        public void FrameCount_KnownSheetSizes_ReturnsExpectedCount(int width, int height, int frameSize, int expectedCount)
        {
            int portraitCount = PortraitSheet.FrameCount(width, height, frameSize);
            
            Assert.Equal(expectedCount, portraitCount);
        }
    }
}