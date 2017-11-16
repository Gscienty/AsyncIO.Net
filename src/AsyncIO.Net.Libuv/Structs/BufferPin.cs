using System;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv.Structs
{
    public class BufferPin : IDisposable
    {
        public byte[] Buffer { get; private set; }
        public GCHandle GCHandle { get; private set; }
        public IntPtr Pointer { get; private set; }
        public IntPtr Offset { get; private set; }
        public IntPtr Count { get; private set; }
        public IntPtr Start => this[this.Offset];
        public IntPtr End => this[this.Offset.ToInt64() + this.Count.ToInt64()];
        
        public BufferPin(uint size) : this(new byte[size]) { }
        public BufferPin(byte[] buffer) : this(buffer, (IntPtr)0, (IntPtr)buffer.Length) { }
        public BufferPin(byte[] buffer, IntPtr offset, IntPtr count)
        {
            this.Buffer = buffer;
            this.Offset = offset;
            this.Count = count;

            this.Alloc();
        }

        private void Alloc()
        {
            this.GCHandle = GCHandle.Alloc(this.Buffer, GCHandleType.Pinned);
            this.Pointer = this.GCHandle.AddrOfPinnedObject();
        }

        private IntPtr this[IntPtr index] => (IntPtr)(this.Pointer.ToInt64() + index.ToInt64());
        private IntPtr this[long index] => this[(IntPtr)index];
        private IntPtr this[int index] => this[(IntPtr)index];

        private void Dispose(bool disposing)
        {
            if (this.GCHandle.IsAllocated)
            {
                this.GCHandle.Free();
            }

            this.Pointer = IntPtr.Zero;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BufferPin()
        {
            this.Dispose(false);
        }
    }
}