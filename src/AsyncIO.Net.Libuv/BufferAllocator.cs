using System;
using AsyncIO.Net.Libuv;
using AsyncIO.Net.Libuv.Structs;

namespace AsyncIO.Net.Libuv
{
    public abstract class BufferAllocator : IDisposable
    {
        internal Handle.uv_alloc_cb Allocator { get; private set; }

        public BufferAllocator() { this.Allocator = Allocate; }

        ~BufferAllocator() => this.Dispose(false);
        public void Dispose() => this.Dispose(true);

        private void Allocate(IntPtr handle, uint size, ref UvBuffer buffer)
        {
            Tuple<uint, IntPtr> result = this.Allocate(size);
            buffer = new UvBuffer(result.Item2, result.Item1);
        }

        public abstract Tuple<uint, IntPtr> Allocate(uint size);
        public abstract void Dispose(bool disposing);
        public abstract ArraySegment<byte> Retrieve(int size);
    }
}