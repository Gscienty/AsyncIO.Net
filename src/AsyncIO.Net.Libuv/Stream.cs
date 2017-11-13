using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using AsyncIO.Net.Libuv.Structs;

namespace AsyncIO.Net.Libuv
{
    public abstract class Stream : Handle
    {
        private Loop _loop;
        private BufferAllocator _allocator;

        public bool IsReadable => uv_is_readable(this.handle) == 1;
        public bool IsWritable => uv_is_writable(this.handle) == 1;
        public int Blocking { set => uv_stream_set_blocking(this.handle, value); }

        public Stream(Loop loop, BufferAllocator allocator)
            : base(Thread.CurrentThread.ManagedThreadId, uv_handle_size(HandleType.Stream), null)
        {
            this._loop = loop;
            this._allocator = allocator;
        }

        public void Listen(Action<Stream, int> receiver, int backlog) => uv_listen(
            this.handle,
            backlog,
            (handle, status) => receiver(this.FromIntPtr<Stream>(handle), status)
        );

        public void Accept(Stream client) => uv_accept(this.handle, client.DangerousGetHandle());

        public void SetBlocking(int blocking) => this.Blocking = blocking;

        public void Shutdown(Action<int> callback)
        {
            ShutdownRequestor shutdown = new ShutdownRequestor();

            uv_shutdown(
                shutdown.DangerousGetHandle(),
                this.handle,
                (handle, status) => callback(status)
            );
        }

        public void ReadStart(Action<uint, UvBuffer> reading) => uv_read_start(
            this.handle,
            this._allocator.Allocator,
            (IntPtr handle, uint nread, ref UvBuffer buffer) => reading(nread, buffer)
        );

        public void ReadStop() => uv_read_stop(this.handle);

        public void Write(ArraySegment<byte> data, Action<int> writed) =>
            this.Write(new ArraySegment<byte>[] { data }, writed);

        public void Write(IList<ArraySegment<byte>> datas, Action<int> writed)
        {
            int length = datas.Count;
            GCHandle[] dataGCHandles = new GCHandle[length];
            UvBuffer[] buffers = new UvBuffer[length];

            for (int i = 0; i < length; i++)
            {
                ArraySegment<byte> data = datas[i];
                int offset = data.Offset;
                int count = data.Count;
                GCHandle dataGCHandle = GCHandle.Alloc(data.Array, GCHandleType.Pinned);
                IntPtr ptr = (IntPtr)(dataGCHandle.AddrOfPinnedObject() + offset);
                buffers[i] = new UvBuffer(ptr, (uint)count);
                dataGCHandles[i] = dataGCHandle;
            }

            WriteRequestor writeRequestor = new WriteRequestor();
            uv_write(
                writeRequestor.DangerousGetHandle(),
                this.handle,
                buffers,
                (uint)length,
                (hendle, status) => writed(status)
            );
        }

        unsafe private THandle FromIntPtr<THandle>(IntPtr handle) where THandle : Handle
        {
            GCHandle gcHandle = GCHandle.FromIntPtr(*(IntPtr*)handle);

            return (THandle) gcHandle.Target;
        }

        internal delegate void uv_connection_cb(IntPtr handle, int status);
        internal delegate void uv_read_cb(IntPtr handle, uint nread, ref UvBuffer buffer);
        internal delegate void uv_shutdown_cb(IntPtr handle, int status);
        internal delegate void uv_write_cb(IntPtr writeRequestor, int status);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_shutdown(IntPtr shutdownHandle, IntPtr handle, uv_shutdown_cb callback);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_listen(IntPtr handle, int backlog, uv_connection_cb callback);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_accept(IntPtr serverHandle, IntPtr clientHandle);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_is_readable(IntPtr handle);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_is_writable(IntPtr handle);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_stream_set_blocking(IntPtr handle, int blocking);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_read_start(IntPtr handle, uv_alloc_cb alloc_cb, uv_read_cb read_cb);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_read_stop(IntPtr handle);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_write(IntPtr writeRequestor, IntPtr handle, UvBuffer[] buffers, uint bufferLength, uv_write_cb write_cb);
    }
}