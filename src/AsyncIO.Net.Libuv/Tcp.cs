using System;
using System.Net;
using System.Runtime.InteropServices;
using AsyncIO.Net.Libuv.Structs;

namespace AsyncIO.Net.Libuv
{
    public class Tcp : Stream
    {
        public Tcp(
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
            public static extern int uv_tcp_bind(Tcp handle, ref NativeSocketAddress addr, int flags);
        }
    }
}