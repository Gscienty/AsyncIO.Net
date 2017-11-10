using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public abstract class Stream : Handle
    {
        private Loop _loop;

        public Stream(Loop loop)
            : base(Thread.CurrentThread.ManagedThreadId, uv_handle_size(HandleType.Stream), null)
        {
            this._loop = loop;
        }

        public void Listen(Action<Stream, int> receiver, int backlog) => uv_listen(
            this.handle,
            backlog,
            (handle, status) => receiver(this.FromIntPtr<Stream>(handle), status)
        );

        public void Accept(Stream client) => uv_accept(this.handle, client.DangerousGetHandle());

        unsafe private THandle FromIntPtr<THandle>(IntPtr handle) where THandle : Handle
        {
            GCHandle gcHandle = GCHandle.FromIntPtr(*(IntPtr*)handle);

            return (THandle) gcHandle.Target;
        }

        internal delegate void uv_connection_cb(IntPtr handle, int status);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_listen(IntPtr handle, int backlog, uv_connection_cb callback);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_accept(IntPtr serverHandle, IntPtr clientHandle);
    }
}