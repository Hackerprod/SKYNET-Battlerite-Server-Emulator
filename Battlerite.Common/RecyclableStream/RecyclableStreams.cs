using System;
using System.IO;

public class RecyclableStreams
{
    public static MemoryStream Create(byte[] bytes)
    {
        return RecyclableStreams.Manager.GetStream("", bytes, 0, bytes.Length);
    }

    public static MemoryStream Create(byte[] bytes, int offset, int length)
    {
        return RecyclableStreams.Manager.GetStream("", bytes, offset, length);
    }

    public static MemoryStream Create()
    {
        return RecyclableStreams.Manager.GetStream();
    }

    public static MemoryStream Create(int size)
    {
        return RecyclableStreams.Manager.GetStream("", size);
    }

    private const int BlockSize = 4096;

    private const int LargeBufferMultiple = 1048576;

    private const int MaxBufferSize = 536870912;

    public static RecyclableMemoryStreamManager Manager = new RecyclableMemoryStreamManager(4096, 1048576, 536870912);
}
