using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public class Async : Handle
    {
        private Loop _loop;
        public Action Schedule { get; set; }

        public Async(Loop loop) : this(loop, null) { }

        public Async(Loop loop, Action schedule)
            : base(Thread.CurrentThread.ManagedThreadId, uv_handle_size(HandleType.Check), null)
        {
            this._loop = loop;
            this.Schedule = schedule;

            uv_async_init(loop.DangerousGetHandle(), this.handle, handle => this.Schedule());
        }

        public void Send()
        {
            if (this.Schedule == null)
            {
                throw new ArgumentNullException(nameof(this.Schedule));
            }

            uv_async_send(this.handle);
        }

        public void Send(Action schedule)
        {
            this.Schedule = schedule;
            this.Send();
        }

        internal delegate void uv_async_cb(IntPtr handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_async_init(IntPtr loopHandle, IntPtr asyncHandle, uv_async_cb callback);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_async_send(IntPtr handle);
    }
}