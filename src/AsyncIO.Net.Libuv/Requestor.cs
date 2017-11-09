using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public abstract class Requestor : Handle
    {
        protected Requestor(int size) : base(Thread.CurrentThread.ManagedThreadId, size, null) { }

        public void Cancel() => uv_cancel(this.handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_cancel(IntPtr handle);
    }
}