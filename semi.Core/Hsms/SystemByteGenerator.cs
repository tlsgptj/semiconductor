namespace semi.Core.Hsms;

public class SystemByteGenerator
{
    private uint _current = 0;

    public uint Next()
    {
        return Interlocked.Increment(ref _current);
    }
}