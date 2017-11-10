using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public class Idle : Handle
    {
        private Loop _loop;
        public Action Schedule { get; set; }

        public Idle(Loop loop) : this(loop, null) { }

        public Idle(Loop loop, Action schedule)
            : base(Thread.CurrentThread.ManagedThreadId, uv_handle_size(HandleType.Idle), null)
        {
            this._loop = loop;
            this.Schedule = schedule;
            uv_idle_init(loop.DangerousGetHandle(), this.handle);
        }

        public void Start()
        {
            if (this.Schedule == null)
            {
                throw new ArgumentNullException(nameof(this.Schedule));
            }

            uv_idle_start(this.handle, handle => this.Schedule());
        }

        public void Stop() => uv_idle_stop(this.handle);

        internal delegate void uv_idle_cb(IntPtr handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_idle_init(IntPtr loopHandle, IntPtr checkHandle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_idle_start(IntPtr handle, uv_idle_cb callback);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_idle_stop(IntPtr handle);
    }
}