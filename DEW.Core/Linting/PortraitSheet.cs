namespace DEW.Core;

public class PortraitSheet
{
    public static int FrameCount(int width, int height, int? frameSize = null)
    {
        int size = frameSize ?? width / 2; //if blank, use Portraiture rule (width /2)
        int cols = width / size;
        int rows = height /size;
        return cols * rows;
    }
}