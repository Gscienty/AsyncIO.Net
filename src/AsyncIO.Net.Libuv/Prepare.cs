using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public class Prepare : Handle
    {
        private Loop _loop;
        public Action Schedule { get; set; }

        public Prepare(Loop loop) : this(loop, null) { }

        public Prepare(Loop loop, Action schedule)
            : base(Thread.CurrentThread.ManagedThreadId, uv_handle_size(HandleType.UV_PREPARE), null)
        {
            this._loop = loop;
            this.Schedule = schedule;

            uv_prepare_init(loop.DangerousGetHandle(), this.handle);
        }

        public void Start()
        {
            if (this.Schedule == null)
            {
                throw new ArgumentNullException(nameof(this.Schedule));
            }

            uv_prepare_start(this.handle, handle => this.Schedule());
        }

        public void Stop() => uv_prepare_stop(this.handle);


        internal delegate void uv_prepare_cb(IntPtr handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_prepare_init(IntPtr loopHandle, IntPtr prepareHandle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_prepare_start(IntPtr handle, uv_prepare_cb callback);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_prepare_stop(IntPtr handle);
    }
}