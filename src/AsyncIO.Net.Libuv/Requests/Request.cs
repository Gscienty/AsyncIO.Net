using System;
using System.Runtime.InteropServices;
using AsyncIO.Net.Libuv;

namespace AsyncIO.Net.Libuv.Requests
{
    public abstract class Request : Handle
    {
        protected Request(ILibuvLogger logger) : base(logger) { }

        protected override bool ReleaseHandle()
        {
            Handle.FreeMemory(this.handle);
            this.SetHandle(IntPtr.Zero);
            return true;   
        }

        internal static new class NativeMethods
        {
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_req_size(RequestType type);
        }
    }
}