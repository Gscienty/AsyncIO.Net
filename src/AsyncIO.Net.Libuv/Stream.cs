using System;
using System.Runtime.InteropServices;
using AsyncIO.Net.Libuv.Structs;

namespace AsyncIO.Net.Libuv
{
    public abstract class Stream : Handle
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void uv_connection_cb(IntPtr server, int status);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void uv_read_cb(IntPtr server, int nread, ref Structs.Buffer buf);

        private readonly static uv_connection_cb _connectionCallback = (serverPtr, status) =>
        {
            Stream server = Handle.FromIntPtr<Stream>(serverPtr);
            server._listener();
        };
        private readonly static uv_alloc_cb _allocCallback = (IntPtr serverPtr, int size, out Structs.Buffer buffer) => 
        {
            Stream server = Handle.FromIntPtr<Stream>(serverPtr);
            buffer = server._allocator(size);
        };
        private readonly static uv_read_cb _readCallback = (IntPtr serverPtr, int size, ref Structs.Buffer buffer) =>
        {
            Stream server = Handle.FromIntPtr<Stream>(serverPtr);
            server._reader(buffer, size);
        };

        private Action _listener;
        private GCHandle _listenKeeper;
        
        private Func<int, Structs.Buffer> _allocator;
        private GCHandle _allocKeeper;

        private Action<Structs.Buffer, int> _reader;
        private GCHandle _readKeeper;

        public bool Readable => NativeMethods.uv_is_readable(this) != 0;

        public bool Writable => NativeMethods.uv_is_writable(this) != 0;

        protected Stream(ILibuvLogger logger) : base(logger) { }

        public Stream Listen(int backlog, Action listener)
        {
            if (this._listenKeeper.IsAllocated)
            {
                throw new InvalidOperationException("TODO: Listen may not be called more than once");
            }

            try
            {
                this._listenKeeper = GCHandle.Alloc(this, GCHandleType.Normal);
                this._listener = listener;
                NativeMethods.uv_listen(this, backlog, Stream._connectionCallback);
            }
            catch
            {
                this._listener = null;
                if (this._listenKeeper.IsAllocated)
                {
                    this._listenKeeper.Free();
                }
                throw;
            }
            
            return this;
        }

        protected Stream Accept(Stream client)
        {
            NativeMethods.uv_accept(this, client);
            return this;
        }

        public Stream ReadStart(Func<int, Structs.Buffer> allocator, Action<Structs.Buffer, int> reader)
        {
            if (this._readKeeper.IsAllocated)
            {
                throw new InvalidOperationException("TODO: ReadStop must be called before ReadStart may be called again");
            }

            try
            {
                this._allocator = allocator;
                this._reader = reader;
                this._readKeeper = GCHandle.Alloc(this, GCHandleType.Normal);

                NativeMethods.uv_read_start(this, Stream._allocCallback, Stream._readCallback);
            }
            catch
            {
                this._allocator = null;
                this._reader = null;
                if (this._readKeeper.IsAllocated)
                {
                    this._readKeeper.Free();
                }

                throw;
            }

            return this;
        }

        public Stream ReadStop()
        {
            this._allocator = null;
            this._reader = null;
            if (this._readKeeper.IsAllocated)
            {
                this._readKeeper.Free();
            }

            NativeMethods.uv_read_stop(this);

            return this;
        }

        public Stream SetBlocking(int blocking)
        {
            NativeMethods.uv_stream_set_blocking(this, blocking);
            return this;
        }

        protected override bool ReleaseHandle()
        {
            if (this._listenKeeper.IsAllocated)
            {
                this._listenKeeper.Free();
            }

            if (this._allocKeeper.IsAllocated)
            {
                this._allocKeeper.Free();
            }

            return base.ReleaseHandle();
        }

        internal static new class NativeMethods
        {
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            public static extern int uv_listen(Stream handle, int backlog, uv_connection_cb cb);

            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            public static extern int uv_accept(Stream server, Stream client);

            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            public extern static int uv_read_start(Stream handle, uv_alloc_cb alloc_cb, uv_read_cb read_cb);

            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            public static extern int uv_read_stop(Stream handle);

            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            public static extern int uv_ip4_addr(string ip, int port, out NativeSocketAddress addr);

            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            public static extern int uv_ip6_addr(string ip, int port, out NativeSocketAddress addr);

            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            public static extern int uv_is_readable(Stream handle);

            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            public static extern int uv_is_writable(Stream handle);

            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            public static extern int uv_stream_set_blocking(Stream handle, int blocking);
        }
    }
}