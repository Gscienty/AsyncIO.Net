using System;
using System.Net;
using System.Runtime.InteropServices;
using AsyncIO.Net.Libuv.Structs;

namespace AsyncIO.Net.Libuv
{
    public class Tcp : Stream
    {
        private Loop _loop;
        private bool _noDelay;
        private bool _simultaneousAccepts;
        private bool IsKeepAlive { get; set; }
        private bool SimultaneousAccepts {
            get => this._simultaneousAccepts;
            set
            {
                this._simultaneousAccepts = value;
                uv_tcp_simultaneous_accepts(this.handle, this._simultaneousAccepts ? 1 : 0);
            }
        }
        public bool NoDelay
        {
            get => this._noDelay;
            set
            {
                this._noDelay = value;
                uv_tcp_nodelay(this.handle, this._noDelay ? 1 : 0);
            }
        }

        public IPEndPoint LocalIPEndPoint
        {
            get
            {
                NativeSocketAddress address;
                int length = Marshal.SizeOf<NativeSocketAddress>();
                uv_tcp_getsockname(this.handle, out address, ref length);

                return address.IPEndPoint;
            }
        }

        public IPEndPoint PeerIPEndPoint
        {
            get
            {
                NativeSocketAddress address;
                int length = Marshal.SizeOf<NativeSocketAddress>();
                uv_tcp_getpeername(this.handle, out address, ref length);

                return address.IPEndPoint;
            }
        }

        public Tcp(Loop loop) : base(loop)
        {
            this._loop = loop;
            uv_tcp_init(this._loop.DangerousGetHandle(), this.handle);
            this._noDelay = false;
            this.IsKeepAlive = false;
            this.SimultaneousAccepts = false;
        }

        public Tcp KeepAlive(uint delay)
        {
            uv_tcp_keepalive(this.handle, this.IsKeepAlive ? 1 : 0, delay);
            return this;
        }

        public Tcp Bind(string ip, int port, uint flags)
        {
            NativeSocketAddress address = NativeSocketAddress.GetIPv4(ip, port);
            uv_tcp_bind(this.handle, ref address, flags);

            return this;
        }

        public Tcp Open(IntPtr fileDescriptor)
        {
            uv_tcp_open(this.handle, fileDescriptor);

            return this;
        }

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_tcp_init(IntPtr loopHandle, IntPtr tcpHandle);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_tcp_init(IntPtr loopHandle, IntPtr tcpHandle, int flags);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_tcp_nodelay(IntPtr handle, int enable);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_tcp_keepalive(IntPtr handle, int enable, uint delay);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_tcp_simultaneous_accepts(IntPtr handle, int enable);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_tcp_bind(IntPtr handle, ref NativeSocketAddress address, uint flags);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_tcp_getsockname(IntPtr handle, out NativeSocketAddress name, ref int length);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_tcp_getpeername(IntPtr handle, out NativeSocketAddress name, ref int length);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_tcp_open(IntPtr handle, IntPtr socket);
    }
}