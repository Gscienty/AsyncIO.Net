using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public enum RequestorType : int
    {
        UnknowRequest = 0,
        Request,
        Connect,
        Write,
        Shutdown,
        UDPSend,
        FileSystem,
        Work,
        GetAddressInformation,
        GetNameInformation,
        RequestTypePrivate,
        RequestTypeMax,
    }
    public abstract class Requestor : Handle
    {
        protected Requestor(int size)
            : base(Thread.CurrentThread.ManagedThreadId, size, null) { }

        public void Cancel() => uv_cancel(this.handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_cancel(IntPtr handle);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_req_size(RequestorType type);
    }

    public class ShutdownRequestor : Requestor
    {
        public ShutdownRequestor()
            : base(uv_req_size(RequestorType.Shutdown)) { }
    }

    public class WriteRequestor: Requestor
    {
        public WriteRequestor()
            : base(uv_req_size(RequestorType.Write)) { }
    }
}