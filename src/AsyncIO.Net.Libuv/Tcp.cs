using System;
using System.Net;
using System.Runtime.InteropServices;
using AsyncIO.Net.Libuv.Structs;

namespace AsyncIO.Net.Libuv
{
    public class Tcp : Stream
    {
        private bool _noDelay;
        private int _keepAlive;
        private bool _simultaneousAccepts;

        public bool NoDelay
        {
            get => this._noDelay;
            set
            {
                this._noDelay = value;
                NativeMethods.uv_tcp_nodelay(this, this._noDelay ? 1 : 0);
            }
        }

        public int KeepAlive
        {
            get => this._keepAlive;
            set
            {
                this._keepAlive = value;
                if (this._keepAlive == 0)
                {
                    NativeMethods.uv_tcp_keepalive(this, 0, 0);
                }
                else
                {
                    NativeMethods.uv_tcp_keepalive(this, 1, this._keepAlive);
                }
            }
        }

        public bool SimultaneousAccepts
        {
            get => this._simultaneousAccepts;
            set
            {
                this._simultaneousAccepts = value;
                NativeMethods.uv_tcp_simultaneous_accepts(this, this._simultaneousAccepts ? 1 : 0);
            }
        }

        public IPEndPoint SocketName
        {
            get
            {
                NativeSocketAddress socketAddress;
                int length = Marshal.SizeOf<NativeSocketAddress>();
                NativeMethods.uv_tcp_getsockname(this, out socketAddress, ref length);
                return socketAddress.IPEndPoint;
            }
        }

        public IPEndPoint PeerName
        {
            get
            {
                NativeSocketAddress socketAddress;
                int length = Marshal.SizeOf<NativeSocketAddress>();
                NativeMethods.uv_tcp_getpeername(this, out socketAddress, ref length);
                return socketAddress.IPEndPoint;
            }
        }

        public Tcp(
            ILibuvLogger logger,
            EventLooper looper,
            Action<Action<IntPtr>, IntPtr> queueCloseHandle) : base(logger)
        {
            this.Initialize(looper, queueCloseHandle);
            this._noDelay = false;
            this._simultaneousAccepts = false;
            this._keepAlive = 0;
        }

        private void Initialize(EventLooper looper, Action<Action<IntPtr>, IntPtr> queueCloseHandle)
        {
            this.AllocateMemory(
                looper.ThreadId,
                Handle.NativeMethods.uv_handle_size(HandleType.TCP),
                queueCloseHandle
            );

            NativeMethods.uv_tcp_init(looper, this);
        }

        public Tcp Bind(string ip, int port, int flags)
        {
            NativeSocketAddress address = NativeSocketAddress.GetIPv4(ip, port);
            NativeMethods.uv_tcp_bind(this, ref address, flags);
            return this;
        }

        public Tcp Accept(EventLooper looper, Action<Action<IntPtr>, IntPtr> queueCloseHandle)
        {
            Tcp client = new Tcp(this._logger, looper, queueCloseHandle);
            this.Accept(client);

            return client;
        }

        internal static new class NativeMethods
        {
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_tcp_init(EventLooper loop, Tcp handle);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_tcp_bind(Tcp handle, ref NativeSocketAddress addr, int flags);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_tcp_nodelay(Tcp handle, int enable);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_tcp_keepalive(Tcp handle, int enable, int delay);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_tcp_simultaneous_accepts(Tcp handle, int enable);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_tcp_getsockname(Tcp handle, out NativeSocketAddress name, ref int nameLength);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_tcp_getpeername(Tcp handle, out NativeSocketAddress name, ref int nameLength);
        }
    }
}