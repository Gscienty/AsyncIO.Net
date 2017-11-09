using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public class Timer : Handle
    {
        private Loop _loop;
        private Action _callback;
        private ulong _interval;
        private ulong _delay;

        public ulong Interval
        {
            get => uv_timer_get_repeat(this.handle);
            set => uv_timer_set_repeat(this.handle, value);
        }

        public Timer(Loop loop)
            : base(Thread.CurrentThread.ManagedThreadId, uv_handle_size(HandleType.UV_TIMER), null)
        {
            this._loop = loop;
            uv_timer_init(this._loop.DangerousGetHandle(), this.handle);

            this._callback = null;
            this._interval = 0;
            this._delay = 0;
        }

        public void Schedule(Action callback) => this.Schedule(callback, 0, 0);

        public void Schedule(Action callback, ulong interval) => this.Schedule(callback, interval, 0);

        public void Schedule(Action callback, ulong interval, ulong delay)
        {
            this._callback = callback;
            this._interval = interval;
            this._delay = delay;
        }

        public void Start()
        {
            if (this._callback == null)
            {
                throw new ArgumentNullException(nameof(this._callback));
            }
            uv_timer_start(this.handle, handle => this._callback(), this._delay, this._interval);
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