using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

public sealed class RecyclableMemoryStream : MemoryStream
{
    internal Guid Id
    {
        get
        {
            this.CheckDisposed();
            return this.id;
        }
    }

    internal string Tag
    {
        get
        {
            this.CheckDisposed();
            return this.tag;
        }
    }

    internal RecyclableMemoryStreamManager MemoryManager
    {
        get
        {
            this.CheckDisposed();
            return this.memoryManager;
        }
    }

    internal string AllocationStack { get; }

    internal string DisposeStack { get; private set; }

    public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager) : this(memoryManager, null, 0, null)
    {
    }

    public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag) : this(memoryManager, tag, 0, null)
    {
    }

    public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag, int requestedSize) : this(memoryManager, tag, requestedSize, null)
    {
    }

    internal RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag, int requestedSize, byte[] initialLargeBuffer) : base(RecyclableMemoryStream.emptyArray)
    {
        this.memoryManager = memoryManager;
        this.id = Guid.NewGuid();
        this.tag = tag;
        if (requestedSize < memoryManager.BlockSize)
        {
            requestedSize = memoryManager.BlockSize;
        }
        if (initialLargeBuffer == null)
        {
            this.EnsureCapacity(requestedSize);
        }
        else
        {
            this.largeBuffer = initialLargeBuffer;
        }
        if (this.memoryManager.GenerateCallStacks)
        {
            this.AllocationStack = Environment.StackTrace;
        }
        RecyclableMemoryStreamManager.Events.Writer.MemoryStreamCreated(this.id, this.tag, requestedSize);
        this.memoryManager.ReportStreamCreated();
    }

    ~RecyclableMemoryStream()
    {
        this.Dispose(false);
    }

    protected override void Dispose(bool disposing)
    {
        if (Interlocked.CompareExchange(ref this.disposedState, 1L, 0L) != 0L)
        {
            string disposeStack = null;
            if (this.memoryManager.GenerateCallStacks)
            {
                disposeStack = Environment.StackTrace;
            }
            RecyclableMemoryStreamManager.Events.Writer.MemoryStreamDoubleDispose(this.id, this.tag, this.AllocationStack, this.DisposeStack, disposeStack);
            return;
        }
        RecyclableMemoryStreamManager.Events.Writer.MemoryStreamDisposed(this.id, this.tag);
        if (this.memoryManager.GenerateCallStacks)
        {
            this.DisposeStack = Environment.StackTrace;
        }
        if (disposing)
        {
            this.memoryManager.ReportStreamDisposed();
            GC.SuppressFinalize(this);
        }
        else
        {
            RecyclableMemoryStreamManager.Events.Writer.MemoryStreamFinalized(this.id, this.tag, this.AllocationStack);
            if (AppDomain.CurrentDomain.IsFinalizingForUnload())
            {
                base.Dispose(disposing);
                return;
            }
            this.memoryManager.ReportStreamFinalized();
        }
        this.memoryManager.ReportStreamLength((long)this.length);
        if (this.largeBuffer != null)
        {
            this.memoryManager.ReturnLargeBuffer(this.largeBuffer, this.tag);
        }
        if (this.dirtyBuffers != null)
        {
            foreach (byte[] buffer in this.dirtyBuffers)
            {
                this.memoryManager.ReturnLargeBuffer(buffer, this.tag);
            }
        }
        this.memoryManager.ReturnBlocks(this.blocks, this.tag);
        this.blocks.Clear();
        base.Dispose(disposing);
    }

    public override void Close()
    {
        this.Dispose(true);
    }

    public override int Capacity
    {
        get
        {
            this.CheckDisposed();
            if (this.largeBuffer != null)
            {
                return this.largeBuffer.Length;
            }
            long val = (long)this.blocks.Count * (long)this.memoryManager.BlockSize;
            return (int)Math.Min(2147483647L, val);
        }
        set
        {
            this.CheckDisposed();
            this.EnsureCapacity(value);
        }
    }

    public override long Length
    {
        get
        {
            this.CheckDisposed();
            return (long)this.length;
        }
    }

    public override long Position
    {
        get
        {
            this.CheckDisposed();
            return (long)this.position;
        }
        set
        {
            this.CheckDisposed();
            if (value < 0L)
            {
                throw new ArgumentOutOfRangeException("value", "value must be non-negative");
            }
            if (value > 2147483647L)
            {
                throw new ArgumentOutOfRangeException("value", "value cannot be more than " + 2147483647L);
            }
            this.position = (int)value;
        }
    }

    public override bool CanRead
    {
        get
        {
            return !this.Disposed;
        }
    }

    public override bool CanSeek
    {
        get
        {
            return !this.Disposed;
        }
    }

    public override bool CanTimeout
    {
        get
        {
            return false;
        }
    }

    public override bool CanWrite
    {
        get
        {
            return !this.Disposed;
        }
    }

    public override byte[] GetBuffer()
    {
        this.CheckDisposed();
        if (this.largeBuffer != null)
        {
            return this.largeBuffer;
        }
        if (this.blocks.Count == 1)
        {
            return this.blocks[0];
        }
        byte[] buffer = this.memoryManager.GetLargeBuffer(this.Capacity, this.tag);
        this.InternalRead(buffer, 0, this.length, 0);
        this.largeBuffer = buffer;
        if (this.blocks.Count > 0 && this.memoryManager.AggressiveBufferReturn)
        {
            this.memoryManager.ReturnBlocks(this.blocks, this.tag);
            this.blocks.Clear();
        }
        return this.largeBuffer;
    }

    [Obsolete("This method has degraded performance vs. GetBuffer and should be avoided.")]
    public override byte[] ToArray()
    {
        this.CheckDisposed();
        byte[] array = new byte[this.Length];
        this.InternalRead(array, 0, this.length, 0);
        string stack = this.memoryManager.GenerateCallStacks ? Environment.StackTrace : null;
        RecyclableMemoryStreamManager.Events.Writer.MemoryStreamToArray(this.id, this.tag, stack, 0);
        this.memoryManager.ReportStreamToArray();
        return array;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return this.SafeRead(buffer, offset, count, ref this.position);
    }

    public int SafeRead(byte[] buffer, int offset, int count, ref int streamPosition)
    {
        this.CheckDisposed();
        if (buffer == null)
        {
            throw new ArgumentNullException("buffer");
        }
        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException("offset", "offset cannot be negative");
        }
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException("count", "count cannot be negative");
        }
        if (offset + count > buffer.Length)
        {
            throw new ArgumentException("buffer length must be at least offset + count");
        }
        int num = this.InternalRead(buffer, offset, count, streamPosition);
        streamPosition += num;
        return num;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        this.CheckDisposed();
        if (buffer == null)
        {
            throw new ArgumentNullException("buffer");
        }
        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException("offset", offset, "Offset must be in the range of 0 - buffer.Length-1");
        }
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException("count", count, "count must be non-negative");
        }
        if (count + offset > buffer.Length)
        {
            throw new ArgumentException("count must be greater than buffer.Length - offset");
        }
        int blockSize = this.memoryManager.BlockSize;
        long num = (long)this.position + (long)count;
        if (num > 2147483647L)
        {
            throw new IOException("Maximum capacity exceeded");
        }
        if ((num + (long)blockSize - 1L) / (long)blockSize * (long)blockSize > 2147483647L)
        {
            throw new IOException("Maximum capacity exceeded");
        }
        this.EnsureCapacity((int)num);
        if (this.largeBuffer == null)
        {
            int i = count;
            int num2 = 0;
            RecyclableMemoryStream.BlockAndOffset blockAndRelativeOffset = this.GetBlockAndRelativeOffset(this.position);
            while (i > 0)
            {
                byte[] dst = this.blocks[blockAndRelativeOffset.Block];
                int num3 = Math.Min(blockSize - blockAndRelativeOffset.Offset, i);
                Buffer.BlockCopy(buffer, offset + num2, dst, blockAndRelativeOffset.Offset, num3);
                i -= num3;
                num2 += num3;
                blockAndRelativeOffset.Block++;
                blockAndRelativeOffset.Offset = 0;
            }
        }
        else
        {
            Buffer.BlockCopy(buffer, offset, this.largeBuffer, this.position, count);
        }
        this.position = (int)num;
        this.length = Math.Max(this.position, this.length);
    }

    public override string ToString()
    {
        return string.Format("Id = {0}, Tag = {1}, Length = {2:N0} bytes", this.Id, this.Tag, this.Length);
    }

    public override void WriteByte(byte value)
    {
        this.CheckDisposed();
        this.byteBuffer[0] = value;
        this.Write(this.byteBuffer, 0, 1);
    }

    public override int ReadByte()
    {
        return this.SafeReadByte(ref this.position);
    }

    public int SafeReadByte(ref int streamPosition)
    {
        this.CheckDisposed();
        if (streamPosition == this.length)
        {
            return -1;
        }
        byte result;
        if (this.largeBuffer == null)
        {
            RecyclableMemoryStream.BlockAndOffset blockAndRelativeOffset = this.GetBlockAndRelativeOffset(streamPosition);
            result = this.blocks[blockAndRelativeOffset.Block][blockAndRelativeOffset.Offset];
        }
        else
        {
            result = this.largeBuffer[streamPosition];
        }
        streamPosition++;
        return (int)result;
    }

    public override void SetLength(long value)
    {
        this.CheckDisposed();
        if (value < 0L || value > 2147483647L)
        {
            throw new ArgumentOutOfRangeException("value", "value must be non-negative and at most " + 2147483647L);
        }
        this.EnsureCapacity((int)value);
        this.length = (int)value;
        if ((long)this.position > value)
        {
            this.position = (int)value;
        }
    }

    public override long Seek(long offset, SeekOrigin loc)
    {
        this.CheckDisposed();
        if (offset > 2147483647L)
        {
            throw new ArgumentOutOfRangeException("offset", "offset cannot be larger than " + 2147483647L);
        }
        int num;
        switch (loc)
        {
            case SeekOrigin.Begin:
                num = (int)offset;
                break;
            case SeekOrigin.Current:
                num = (int)offset + this.position;
                break;
            case SeekOrigin.End:
                num = (int)offset + this.length;
                break;
            default:
                throw new ArgumentException("Invalid seek origin", "loc");
        }
        if (num < 0)
        {
            throw new IOException("Seek before beginning");
        }
        this.position = num;
        return (long)this.position;
    }

    public override void WriteTo(Stream stream)
    {
        this.CheckDisposed();
        if (stream == null)
        {
            throw new ArgumentNullException("stream");
        }
        if (this.largeBuffer == null)
        {
            int num = 0;
            int i = this.length;
            while (i > 0)
            {
                int num2 = Math.Min(this.blocks[num].Length, i);
                stream.Write(this.blocks[num], 0, num2);
                i -= num2;
                num++;
            }
            return;
        }
        stream.Write(this.largeBuffer, 0, this.length);
    }

    private bool Disposed
    {
        get
        {
            return Interlocked.Read(ref this.disposedState) != 0L;
        }
    }

    private void CheckDisposed()
    {
        if (this.Disposed)
        {
            throw new ObjectDisposedException(string.Format("The stream with Id {0} and Tag {1} is disposed.", this.id, this.tag));
        }
    }

    private int InternalRead(byte[] buffer, int offset, int count, int fromPosition)
    {
        if (this.length - fromPosition <= 0)
        {
            return 0;
        }
        int num2;
        if (this.largeBuffer == null)
        {
            RecyclableMemoryStream.BlockAndOffset blockAndRelativeOffset = this.GetBlockAndRelativeOffset(fromPosition);
            int num = 0;
            int i = Math.Min(count, this.length - fromPosition);
            while (i > 0)
            {
                num2 = Math.Min(this.blocks[blockAndRelativeOffset.Block].Length - blockAndRelativeOffset.Offset, i);
                Buffer.BlockCopy(this.blocks[blockAndRelativeOffset.Block], blockAndRelativeOffset.Offset, buffer, num + offset, num2);
                num += num2;
                i -= num2;
                blockAndRelativeOffset.Block++;
                blockAndRelativeOffset.Offset = 0;
            }
            return num;
        }
        num2 = Math.Min(count, this.length - fromPosition);
        Buffer.BlockCopy(this.largeBuffer, fromPosition, buffer, offset, num2);
        return num2;
    }

    private RecyclableMemoryStream.BlockAndOffset GetBlockAndRelativeOffset(int offset)
    {
        int blockSize = this.memoryManager.BlockSize;
        return new RecyclableMemoryStream.BlockAndOffset(offset / blockSize, offset % blockSize);
    }

    private void EnsureCapacity(int newCapacity)
    {
        if ((long)newCapacity > this.memoryManager.MaximumStreamCapacity && this.memoryManager.MaximumStreamCapacity > 0L)
        {
            RecyclableMemoryStreamManager.Events.Writer.MemoryStreamOverCapacity(newCapacity, this.memoryManager.MaximumStreamCapacity, this.tag, this.AllocationStack);
            throw new InvalidOperationException(string.Concat(new object[]
            {
                    "Requested capacity is too large: ",
                    newCapacity,
                    ". Limit is ",
                    this.memoryManager.MaximumStreamCapacity
            }));
        }
        if (this.largeBuffer != null)
        {
            if (newCapacity > this.largeBuffer.Length)
            {
                byte[] buffer = this.memoryManager.GetLargeBuffer(newCapacity, this.tag);
                this.InternalRead(buffer, 0, this.length, 0);
                this.ReleaseLargeBuffer();
                this.largeBuffer = buffer;
                return;
            }
        }
        else
        {
            while (this.Capacity < newCapacity)
            {
                this.blocks.Add(this.memoryManager.GetBlock());
            }
        }
    }

    private void ReleaseLargeBuffer()
    {
        if (this.memoryManager.AggressiveBufferReturn)
        {
            this.memoryManager.ReturnLargeBuffer(this.largeBuffer, this.tag);
        }
        else
        {
            if (this.dirtyBuffers == null)
            {
                this.dirtyBuffers = new List<byte[]>(1);
            }
            this.dirtyBuffers.Add(this.largeBuffer);
        }
        this.largeBuffer = null;
    }

    private const long MaxStreamLength = 2147483647L;

    private static readonly byte[] emptyArray = new byte[0];

    private readonly List<byte[]> blocks = new List<byte[]>(1);

    private readonly byte[] byteBuffer = new byte[1];

    private readonly Guid id;

    private readonly RecyclableMemoryStreamManager memoryManager;

    private readonly string tag;

    private List<byte[]> dirtyBuffers;

    private long disposedState;

    private byte[] largeBuffer;

    private int length;

    private int position;

    private struct BlockAndOffset
    {
        public BlockAndOffset(int block, int offset)
        {
            this.Block = block;
            this.Offset = offset;
        }

        public int Block;

        public int Offset;
    }
}
