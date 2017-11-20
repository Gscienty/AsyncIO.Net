using System;
using System.Net;
using System.Runtime.InteropServices;
using AsyncIO.Net.Libuv.Structs;

namespace AsyncIO.Net.Libuv
{
    public class Udp : Handle
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void uv_udp_recv_cb(IntPtr handle, ref Structs.Buffer buffer, ref NativeSocketAddress address, uint flags);

        private Action<IPEndPoint, Structs.Buffer, uint> _receiver;
        private GCHandle _receiverKeeper;

        private Func<int, Structs.Buffer> _allocator;

        private readonly static uv_udp_recv_cb _recvCallback = (IntPtr handle, ref Structs.Buffer buffer, ref NativeSocketAddress address, uint flags) =>
        {
            Udp server = Handle.FromIntPtr<Udp>(handle);
            server._receiver(address.IPEndPoint, buffer, flags);
        };

        private readonly static uv_alloc_cb _allocCallback = (IntPtr handle, int size, out Structs.Buffer buffer) =>
        {
            Udp server = Handle.FromIntPtr<Udp>(handle);
            buffer = server._allocator(size);
        };

        public IPEndPoint SocketName
        {
            get
            {
                NativeSocketAddress socketAddress;
                int length = Marshal.SizeOf<NativeSocketAddress>();
                NativeMethods.uv_udp_getsockname(this, out socketAddress, ref length);
                return socketAddress.IPEndPoint;
            }
        }

        public Udp(
            ILibuvLogger logger,
            EventLooper looper,
            uint flags,
            Action<Action<IntPtr>, IntPtr> queueCloseHandle) : base(logger)
        {
            this.Initialize(looper, flags, queueCloseHandle);
        }

        public Udp(
            ILibuvLogger logger,
            EventLooper looper,
            Action<Action<IntPtr>, IntPtr> queueCloseHandle) : base(logger)
        {
            this.Initialize(looper, queueCloseHandle);
        }

        private void Initialize(EventLooper looper, Action<Action<IntPtr>, IntPtr> queueCloseHandle)
        {
            this.AllocateMemory(
                looper.ThreadId,
                Handle.NativeMethods.uv_handle_size(HandleType.UDP),
                queueCloseHandle
            );

            NativeMethods.uv_udp_init(looper, this);
        }

        private void Initialize(EventLooper looper, uint flags, Action<Action<IntPtr>, IntPtr> queueCloseHandle)
        {
            this.AllocateMemory(
                looper.ThreadId,
                Handle.NativeMethods.uv_handle_size(HandleType.UDP),
                queueCloseHandle
            );

            NativeMethods.uv_udp_init_ex(looper, this, flags);
        }

        public Udp Bind(string ip, int port)
        {
            NativeSocketAddress address = NativeSocketAddress.GetIPv4(ip, port);
            NativeMethods.uv_udp_bind(this, ref address, 0);
            return this;
        }

        public Udp ReceiveStart(
            Func<int, Structs.Buffer> allocator,
            Action<IPEndPoint, Structs.Buffer, uint> receiver)
        {
            if (this._receiverKeeper.IsAllocated)
            {
                throw new InvalidOperationException("TODO: ReceiveStop must be called before ReceiveStart may be called again");
            }

            try
            {
                this._allocator = allocator;
                this._receiver = receiver;
                this._receiverKeeper = GCHandle.Alloc(this, GCHandleType.Normal);

                NativeMethods.uv_udp_recv_start(this, Udp._allocCallback, Udp._recvCallback);

                return this;
            }
            catch
            {
                this._allocator = null;
                this._receiver = null;
                if (this._receiverKeeper.IsAllocated)
                {
                    this._receiverKeeper.Free();
                }

                throw;
            }
        }

        public Udp ReceiveStop()
        {
            this._allocator = null;
            this._receiver = null;
            if (this._receiverKeeper.IsAllocated)
            {
                this._receiverKeeper.Free();
            }

            NativeMethods.uv_udp_recv_stop(this);
            return this;
        }

        internal static new class NativeMethods
        {
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_udp_init(EventLooper looper, Udp handle);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_udp_init_ex(EventLooper looper, Udp handle, uint flags);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_udp_bind(Udp handle, ref NativeSocketAddress address, uint flags);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_udp_getsockname(Udp handle, out NativeSocketAddress address, ref int length);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            unsafe internal static extern int uv_udp_recv_start(Udp handle, uv_alloc_cb allocCallback, uv_udp_recv_cb recvCallback);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_udp_recv_stop(Udp handle);
        }
    }
}