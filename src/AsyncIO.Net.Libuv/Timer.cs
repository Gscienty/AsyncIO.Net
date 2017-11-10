using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public class Timer : Handle
    {
        private Loop _loop;
        private ulong _interval;

        public ulong Interval
        {
            get => uv_timer_get_repeat(this.handle);
            set
            {
                uv_timer_set_repeat(this.handle, value);
                this._interval = value;
            }
        }

        public Action Schedule { get; set; }
        public ulong Delay { get; set; }

        public Timer(Loop loop) : this(loop, null) {}

        public Timer(Loop loop, Action schedule)
            : base(Thread.CurrentThread.ManagedThreadId, uv_handle_size(HandleType.Timer), null)
        {
            this._loop = loop;
            uv_timer_init(this._loop.DangerousGetHandle(), this.handle);

            this.Schedule = schedule;
            this._interval = 0;
            this.Delay = 0;
        }

        public void Set(Action schedule) => this.Set(schedule, 0, 0);

        public void Set(Action schedule, ulong interval) => this.Set(schedule, interval, 0);

        public void Set(Action schedule, ulong interval, ulong delay)
        {
            this.Schedule = schedule;
            this._interval = interval;
            this.Delay = delay;
        }

        public void Start()
        {
            if (this.Schedule == null)
            {
                throw new ArgumentNullException(nameof(this.Schedule));
            }
            uv_timer_start(this.handle, handle => this.Schedule(), this.Delay, this._interval);
        }

        public void Stop() => uv_timer_stop(this.handle);

        public void Again() => uv_timer_again(this.handle);


        internal delegate void uv_timer_cb(IntPtr handle);


        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_timer_init(IntPtr loopHandle, IntPtr timerHandle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_timer_start(IntPtr handle, uv_timer_cb cb, ulong timeout, ulong repeat);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_timer_stop(IntPtr handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_timer_again(IntPtr handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void uv_timer_set_repeat(IntPtr handle, ulong repeat);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern ulong uv_timer_get_repeat(IntPtr handle);
    }
}