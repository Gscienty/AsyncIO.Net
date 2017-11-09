using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public class Check : Handle
    {
        private Loop _loop;
        public Action Schedule { get; set; }

        public Check(Loop loop) : this(loop, null) { }

        public Check(Loop loop, Action schedule)
            : base(Thread.CurrentThread.ManagedThreadId, uv_handle_size(HandleType.UV_CHECK), null)
        {
            this._loop = loop;
            this.Schedule = schedule;
        }

        public void Start()
        {
            if (this.Schedule == null)
            {
                throw new ArgumentNullException(nameof(this.Schedule));
            }

            uv_check_start(this.handle, handle => this.Schedule());
        }

        public void Stop() => uv_check_stop(this.handle);

        internal delegate void uv_check_cb(IntPtr handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_check_init(IntPtr loopHandle, IntPtr checkHandle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_check_start(IntPtr handle, uv_check_cb callback);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_check_stop(IntPtr handle);
    }
}