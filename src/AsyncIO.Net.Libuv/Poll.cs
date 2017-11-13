using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    [Flags]
    public enum PollEvent : int
    {
        Readable = 1,
        Writeable = 2,
        Disconnect = 4,
        Prioritized = 8
    }
    public abstract class Poll : Handle
    {
        private Loop _loop;

        public Action<int, int> Schedule;

        public Poll(Loop loop) : this(loop, null) { }

        public Poll(Loop loop, Action<int, int> schedule)
            : base(Thread.CurrentThread.ManagedThreadId, uv_handle_size(HandleType.Poll), null)
        {
            this._loop = loop;
            this.Schedule = schedule;
            this.Initialize();
        }

        public void Start(PollEvent events) => uv_poll_start(
            this.handle,
            events,
            (handle, status, resultEvents) => this.Schedule(status, resultEvents)
        );

        public void Stop() => uv_poll_stop(this.handle);

        protected abstract void Initialize();

        internal delegate void uv_poll_cb(IntPtr handle, int status, int events);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_poll_init(IntPtr loopHandle, IntPtr pollHandle, int fd);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_poll_init_socket(IntPtr loopHandle, IntPtr pollHandle, IntPtr socket);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_poll_start(IntPtr handle, PollEvent events, uv_poll_cb callback);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_poll_stop(IntPtr handle);
    }
}