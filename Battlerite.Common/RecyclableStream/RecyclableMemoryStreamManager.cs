using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Threading;

public sealed class RecyclableMemoryStreamManager
{
    public RecyclableMemoryStreamManager() : this(131072, 1048576, 134217728)
    {
    }

    public RecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize)
    {
        if (blockSize <= 0)
        {
            throw new ArgumentOutOfRangeException("blockSize", blockSize, "blockSize must be a positive number");
        }
        if (largeBufferMultiple <= 0)
        {
            throw new ArgumentOutOfRangeException("largeBufferMultiple", "largeBufferMultiple must be a positive number");
        }
        if (maximumBufferSize < blockSize)
        {
            throw new ArgumentOutOfRangeException("maximumBufferSize", "maximumBufferSize must be at least blockSize");
        }
        this.blockSize = blockSize;
        this.largeBufferMultiple = largeBufferMultiple;
        this.maximumBufferSize = maximumBufferSize;
        if (!this.IsLargeBufferMultiple(maximumBufferSize))
        {
            throw new ArgumentException("maximumBufferSize is not a multiple of largeBufferMultiple", "maximumBufferSize");
        }
        this.smallPool = new ConcurrentStack<byte[]>();
        int num = maximumBufferSize / largeBufferMultiple;
        this.largeBufferInUseSize = new long[num + 1];
        this.largeBufferFreeSize = new long[num];
        this.largePools = new ConcurrentStack<byte[]>[num];
        for (int i = 0; i < this.largePools.Length; i++)
        {
            this.largePools[i] = new ConcurrentStack<byte[]>();
        }
        RecyclableMemoryStreamManager.Events.Writer.MemoryStreamManagerInitialized(blockSize, largeBufferMultiple, maximumBufferSize);
    }

    public int BlockSize
    {
        get
        {
            return this.blockSize;
        }
    }

    public int LargeBufferMultiple
    {
        get
        {
            return this.largeBufferMultiple;
        }
    }

    public int MaximumBufferSize
    {
        get
        {
            return this.maximumBufferSize;
        }
    }

    public long SmallPoolFreeSize
    {
        get
        {
            return this.smallPoolFreeSize;
        }
    }

    public long SmallPoolInUseSize
    {
        get
        {
            return this.smallPoolInUseSize;
        }
    }

    public long LargePoolFreeSize
    {
        get
        {
            return this.largeBufferFreeSize.Sum();
        }
    }

    public long LargePoolInUseSize
    {
        get
        {
            return this.largeBufferInUseSize.Sum();
        }
    }

    public long SmallBlocksFree
    {
        get
        {
            return (long)this.smallPool.Count;
        }
    }

    public long LargeBuffersFree
    {
        get
        {
            long num = 0L;
            foreach (ConcurrentStack<byte[]> concurrentStack in this.largePools)
            {
                num += (long)concurrentStack.Count;
            }
            return num;
        }
    }

    public long MaximumFreeSmallPoolBytes { get; set; }

    public long MaximumFreeLargePoolBytes { get; set; }

    public long MaximumStreamCapacity { get; set; }

    public bool GenerateCallStacks { get; set; }

    public bool AggressiveBufferReturn { get; set; }

    internal byte[] GetBlock()
    {
        byte[] result;
        if (!this.smallPool.TryPop(out result))
        {
            result = new byte[this.BlockSize];
            RecyclableMemoryStreamManager.Events.Writer.MemoryStreamNewBlockCreated(this.smallPoolInUseSize);
            this.ReportBlockCreated();
        }
        else
        {
            Interlocked.Add(ref this.smallPoolFreeSize, (long)(-(long)this.BlockSize));
        }
        Interlocked.Add(ref this.smallPoolInUseSize, (long)this.BlockSize);
        return result;
    }

    internal byte[] GetLargeBuffer(int requiredSize, string tag)
    {
        requiredSize = this.RoundToLargeBufferMultiple(requiredSize);
        int num = requiredSize / this.largeBufferMultiple - 1;
        byte[] array;
        if (num < this.largePools.Length)
        {
            if (!this.largePools[num].TryPop(out array))
            {
                array = new byte[requiredSize];
                RecyclableMemoryStreamManager.Events.Writer.MemoryStreamNewLargeBufferCreated(requiredSize, this.LargePoolInUseSize);
                this.ReportLargeBufferCreated();
            }
            else
            {
                Interlocked.Add(ref this.largeBufferFreeSize[num], (long)(-(long)array.Length));
            }
        }
        else
        {
            num = this.largeBufferInUseSize.Length - 1;
            array = new byte[requiredSize];
            string allocationStack = null;
            if (this.GenerateCallStacks)
            {
                allocationStack = Environment.StackTrace;
            }
            RecyclableMemoryStreamManager.Events.Writer.MemoryStreamNonPooledLargeBufferCreated(requiredSize, tag, allocationStack);
            this.ReportLargeBufferCreated();
        }
        Interlocked.Add(ref this.largeBufferInUseSize[num], (long)array.Length);
        return array;
    }

    private int RoundToLargeBufferMultiple(int requiredSize)
    {
        return (requiredSize + this.LargeBufferMultiple - 1) / this.LargeBufferMultiple * this.LargeBufferMultiple;
    }

    private bool IsLargeBufferMultiple(int value)
    {
        return value != 0 && value % this.LargeBufferMultiple == 0;
    }

    internal void ReturnLargeBuffer(byte[] buffer, string tag)
    {
        if (buffer == null)
        {
            throw new ArgumentNullException("buffer");
        }
        if (!this.IsLargeBufferMultiple(buffer.Length))
        {
            throw new ArgumentException("buffer did not originate from this memory manager. The size is not a multiple of " + this.LargeBufferMultiple);
        }
        int num = buffer.Length / this.largeBufferMultiple - 1;
        if (num < this.largePools.Length)
        {
            if ((long)((this.largePools[num].Count + 1) * buffer.Length) <= this.MaximumFreeLargePoolBytes || this.MaximumFreeLargePoolBytes == 0L)
            {
                this.largePools[num].Push(buffer);
                Interlocked.Add(ref this.largeBufferFreeSize[num], (long)buffer.Length);
            }
            else
            {
                RecyclableMemoryStreamManager.Events.Writer.MemoryStreamDiscardBuffer(RecyclableMemoryStreamManager.Events.MemoryStreamBufferType.Large, tag, RecyclableMemoryStreamManager.Events.MemoryStreamDiscardReason.EnoughFree);
                this.ReportLargeBufferDiscarded(RecyclableMemoryStreamManager.Events.MemoryStreamDiscardReason.EnoughFree);
            }
        }
        else
        {
            num = this.largeBufferInUseSize.Length - 1;
            RecyclableMemoryStreamManager.Events.Writer.MemoryStreamDiscardBuffer(RecyclableMemoryStreamManager.Events.MemoryStreamBufferType.Large, tag, RecyclableMemoryStreamManager.Events.MemoryStreamDiscardReason.TooLarge);
            this.ReportLargeBufferDiscarded(RecyclableMemoryStreamManager.Events.MemoryStreamDiscardReason.TooLarge);
        }
        Interlocked.Add(ref this.largeBufferInUseSize[num], (long)(-(long)buffer.Length));
        this.ReportUsageReport(this.smallPoolInUseSize, this.smallPoolFreeSize, this.LargePoolInUseSize, this.LargePoolFreeSize);
    }

    internal void ReturnBlocks(ICollection<byte[]> blocks, string tag)
    {
        if (blocks == null)
        {
            throw new ArgumentNullException("blocks");
        }
        int num = blocks.Count * this.BlockSize;
        Interlocked.Add(ref this.smallPoolInUseSize, (long)(-(long)num));
        foreach (byte[] array in blocks)
        {
            if (array == null || array.Length != this.BlockSize)
            {
                throw new ArgumentException("blocks contains buffers that are not BlockSize in length");
            }
        }
        foreach (byte[] item in blocks)
        {
            if (this.MaximumFreeSmallPoolBytes != 0L && this.SmallPoolFreeSize >= this.MaximumFreeSmallPoolBytes)
            {
                RecyclableMemoryStreamManager.Events.Writer.MemoryStreamDiscardBuffer(RecyclableMemoryStreamManager.Events.MemoryStreamBufferType.Small, tag, RecyclableMemoryStreamManager.Events.MemoryStreamDiscardReason.EnoughFree);
                this.ReportBlockDiscarded();
                break;
            }
            Interlocked.Add(ref this.smallPoolFreeSize, (long)this.BlockSize);
            this.smallPool.Push(item);
        }
        this.ReportUsageReport(this.smallPoolInUseSize, this.smallPoolFreeSize, this.LargePoolInUseSize, this.LargePoolFreeSize);
    }

    internal void ReportBlockCreated()
    {
        RecyclableMemoryStreamManager.EventHandler blockCreated = this.BlockCreated;
        if (blockCreated == null)
        {
            return;
        }
        blockCreated();
    }

    internal void ReportBlockDiscarded()
    {
        RecyclableMemoryStreamManager.EventHandler blockDiscarded = this.BlockDiscarded;
        if (blockDiscarded == null)
        {
            return;
        }
        blockDiscarded();
    }

    internal void ReportLargeBufferCreated()
    {
        RecyclableMemoryStreamManager.EventHandler largeBufferCreated = this.LargeBufferCreated;
        if (largeBufferCreated == null)
        {
            return;
        }
        largeBufferCreated();
    }

    internal void ReportLargeBufferDiscarded(RecyclableMemoryStreamManager.Events.MemoryStreamDiscardReason reason)
    {
        RecyclableMemoryStreamManager.LargeBufferDiscardedEventHandler largeBufferDiscarded = this.LargeBufferDiscarded;
        if (largeBufferDiscarded == null)
        {
            return;
        }
        largeBufferDiscarded(reason);
    }

    internal void ReportStreamCreated()
    {
        RecyclableMemoryStreamManager.EventHandler streamCreated = this.StreamCreated;
        if (streamCreated == null)
        {
            return;
        }
        streamCreated();
    }

    internal void ReportStreamDisposed()
    {
        RecyclableMemoryStreamManager.EventHandler streamDisposed = this.StreamDisposed;
        if (streamDisposed == null)
        {
            return;
        }
        streamDisposed();
    }

    internal void ReportStreamFinalized()
    {
        RecyclableMemoryStreamManager.EventHandler streamFinalized = this.StreamFinalized;
        if (streamFinalized == null)
        {
            return;
        }
        streamFinalized();
    }

    internal void ReportStreamLength(long bytes)
    {
        RecyclableMemoryStreamManager.StreamLengthReportHandler streamLength = this.StreamLength;
        if (streamLength == null)
        {
            return;
        }
        streamLength(bytes);
    }

    internal void ReportStreamToArray()
    {
        RecyclableMemoryStreamManager.EventHandler streamConvertedToArray = this.StreamConvertedToArray;
        if (streamConvertedToArray == null)
        {
            return;
        }
        streamConvertedToArray();
    }

    internal void ReportUsageReport(long smallPoolInUseBytes, long smallPoolFreeBytes, long largePoolInUseBytes, long largePoolFreeBytes)
    {
        RecyclableMemoryStreamManager.UsageReportEventHandler usageReport = this.UsageReport;
        if (usageReport == null)
        {
            return;
        }
        usageReport(smallPoolInUseBytes, smallPoolFreeBytes, largePoolInUseBytes, largePoolFreeBytes);
    }

    public MemoryStream GetStream()
    {
        return new RecyclableMemoryStream(this);
    }

    public MemoryStream GetStream(string tag)
    {
        return new RecyclableMemoryStream(this, tag);
    }

    public MemoryStream GetStream(string tag, int requiredSize)
    {
        return new RecyclableMemoryStream(this, tag, requiredSize);
    }

    public MemoryStream GetStream(string tag, int requiredSize, bool asContiguousBuffer)
    {
        if (!asContiguousBuffer || requiredSize <= this.BlockSize)
        {
            return this.GetStream(tag, requiredSize);
        }
        return new RecyclableMemoryStream(this, tag, requiredSize, this.GetLargeBuffer(requiredSize, tag));
    }

    public MemoryStream GetStream(string tag, byte[] buffer, int offset, int count)
    {
        RecyclableMemoryStream recyclableMemoryStream = new RecyclableMemoryStream(this, tag, count);
        recyclableMemoryStream.Write(buffer, offset, count);
        recyclableMemoryStream.Position = 0L;
        return recyclableMemoryStream;
    }

    public event RecyclableMemoryStreamManager.EventHandler BlockCreated;

    public event RecyclableMemoryStreamManager.EventHandler BlockDiscarded;

    public event RecyclableMemoryStreamManager.EventHandler LargeBufferCreated;

    public event RecyclableMemoryStreamManager.EventHandler StreamCreated;

    public event RecyclableMemoryStreamManager.EventHandler StreamDisposed;

    public event RecyclableMemoryStreamManager.EventHandler StreamFinalized;

    public event RecyclableMemoryStreamManager.StreamLengthReportHandler StreamLength;

    public event RecyclableMemoryStreamManager.EventHandler StreamConvertedToArray;

    public event RecyclableMemoryStreamManager.LargeBufferDiscardedEventHandler LargeBufferDiscarded;

    public event RecyclableMemoryStreamManager.UsageReportEventHandler UsageReport;

    public const int DefaultBlockSize = 131072;

    public const int DefaultLargeBufferMultiple = 1048576;

    public const int DefaultMaximumBufferSize = 134217728;

    private readonly int blockSize;

    private readonly long[] largeBufferFreeSize;

    private readonly long[] largeBufferInUseSize;

    private readonly int largeBufferMultiple;

    private readonly ConcurrentStack<byte[]>[] largePools;

    private readonly int maximumBufferSize;

    private readonly ConcurrentStack<byte[]> smallPool;

    private long smallPoolFreeSize;

    private long smallPoolInUseSize;

    [EventSource(Name = "Microsoft-IO-RecyclableMemoryStream", Guid = "{B80CD4E4-890E-468D-9CBA-90EB7C82DFC7}")]
    public sealed class Events : EventSource
    {
        [Event(1, Level = EventLevel.Verbose)]
        public void MemoryStreamCreated(Guid guid, string tag, int requestedSize)
        {
            if (base.IsEnabled(EventLevel.Verbose, EventKeywords.None))
            {
                base.WriteEvent(1, new object[]
                {
                        guid,
                        tag ?? string.Empty,
                        requestedSize
                });
            }
        }

        [Event(2, Level = EventLevel.Verbose)]
        public void MemoryStreamDisposed(Guid guid, string tag)
        {
            if (base.IsEnabled(EventLevel.Verbose, EventKeywords.None))
            {
                base.WriteEvent(2, new object[]
                {
                        guid,
                        tag ?? string.Empty
                });
            }
        }

        [Event(3, Level = EventLevel.Critical)]
        public void MemoryStreamDoubleDispose(Guid guid, string tag, string allocationStack, string disposeStack1, string disposeStack2)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(3, new object[]
                {
                        guid,
                        tag ?? string.Empty,
                        allocationStack ?? string.Empty,
                        disposeStack1 ?? string.Empty,
                        disposeStack2 ?? string.Empty
                });
            }
        }

        [Event(4, Level = EventLevel.Error)]
        public void MemoryStreamFinalized(Guid guid, string tag, string allocationStack)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(4, new object[]
                {
                        guid,
                        tag ?? string.Empty,
                        allocationStack ?? string.Empty
                });
            }
        }

        [Event(5, Level = EventLevel.Verbose)]
        public void MemoryStreamToArray(Guid guid, string tag, string stack, int size)
        {
            if (base.IsEnabled(EventLevel.Verbose, EventKeywords.None))
            {
                base.WriteEvent(5, new object[]
                {
                        guid,
                        tag ?? string.Empty,
                        stack ?? string.Empty,
                        size
                });
            }
        }

        [Event(6, Level = EventLevel.Informational)]
        public void MemoryStreamManagerInitialized(int blockSize, int largeBufferMultiple, int maximumBufferSize)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(6, blockSize, largeBufferMultiple, maximumBufferSize);
            }
        }

        [Event(7, Level = EventLevel.Verbose)]
        public void MemoryStreamNewBlockCreated(long smallPoolInUseBytes)
        {
            if (base.IsEnabled(EventLevel.Verbose, EventKeywords.None))
            {
                base.WriteEvent(7, smallPoolInUseBytes);
            }
        }

        [Event(8, Level = EventLevel.Verbose)]
        public void MemoryStreamNewLargeBufferCreated(int requiredSize, long largePoolInUseBytes)
        {
            if (base.IsEnabled(EventLevel.Verbose, EventKeywords.None))
            {
                base.WriteEvent(8, (long)requiredSize, largePoolInUseBytes);
            }
        }

        [Event(9, Level = EventLevel.Verbose)]
        public void MemoryStreamNonPooledLargeBufferCreated(int requiredSize, string tag, string allocationStack)
        {
            if (base.IsEnabled(EventLevel.Verbose, EventKeywords.None))
            {
                base.WriteEvent(9, new object[]
                {
                        requiredSize,
                        tag ?? string.Empty,
                        allocationStack ?? string.Empty
                });
            }
        }

        [Event(10, Level = EventLevel.Warning)]
        public void MemoryStreamDiscardBuffer(RecyclableMemoryStreamManager.Events.MemoryStreamBufferType bufferType, string tag, RecyclableMemoryStreamManager.Events.MemoryStreamDiscardReason reason)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(10, new object[]
                {
                        bufferType,
                        tag ?? string.Empty,
                        reason
                });
            }
        }

        [Event(11, Level = EventLevel.Error)]
        public void MemoryStreamOverCapacity(int requestedCapacity, long maxCapacity, string tag, string allocationStack)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(11, new object[]
                {
                        requestedCapacity,
                        maxCapacity,
                        tag ?? string.Empty,
                        allocationStack ?? string.Empty
                });
            }
        }

        public static RecyclableMemoryStreamManager.Events Writer = new RecyclableMemoryStreamManager.Events();

        public enum MemoryStreamBufferType
        {
            Small,
            Large
        }

        public enum MemoryStreamDiscardReason
        {
            TooLarge,
            EnoughFree
        }
    }

    public delegate void EventHandler();

    public delegate void LargeBufferDiscardedEventHandler(RecyclableMemoryStreamManager.Events.MemoryStreamDiscardReason reason);

    public delegate void StreamLengthReportHandler(long bytes);

    public delegate void UsageReportEventHandler(long smallPoolInUseBytes, long smallPoolFreeBytes, long largePoolInUseBytes, long largePoolFreeBytes);
}
