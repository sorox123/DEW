namespace DEW.Core;

public class PortraitSheet
{
    public static int FrameCount(int width, int height, int frameSize)
    {
        int cols = width / frameSize;
        int rows = height /frameSize;
        return cols * rows;
    }
}