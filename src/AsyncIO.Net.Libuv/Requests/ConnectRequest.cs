using System;
using System.Net;
using System.Runtime.InteropServices;
using AsyncIO.Net.Libuv.Structs;

namespace AsyncIO.Net.Libuv.Requests
{
    public class ConnectRequest : Request
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void uv_connect_cb(IntPtr req, int status);

        private static readonly uv_connect_cb connectCallback = (requestPointer, status) => 
        {
            ConnectRequest request = Handle.FromIntPtr<ConnectRequest>(requestPointer);

            request._callback();
        };

        private Action _callback;

        public ConnectRequest(ILibuvLogger logger, EventLooper looper) : base(logger)
        {
            int requestSize = Request.NativeMethods.uv_req_size(RequestType.CONNECT);
            this.AllocateMemory(looper.ThreadId, requestSize);
        }

        public void TcpConnect(Tcp client, string ip, int port, Action callback)
        {
            this._callback = callback;
            NativeSocketAddress address = NativeSocketAddress.GetIPv4(ip, port);
            NativeMethods.uv_tcp_connect(this, client, ref address, ConnectRequest.connectCallback);
        }

        internal static new class NativeMethods
        {
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_tcp_connect(ConnectRequest connectRequest, Tcp client, ref NativeSocketAddress address, uv_connect_cb connectCallback);
        }
    }
}