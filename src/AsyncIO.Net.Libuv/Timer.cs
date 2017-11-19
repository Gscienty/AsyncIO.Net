using System;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public class Timer : Handle
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void uv_timer_cb(IntPtr handle);

        private readonly static uv_timer_cb _timerCallback = handle =>
        {
            Timer timer = Handle.FromIntPtr<Timer>(handle);

            timer._callback();
        };

        private Action _callback;

        public ulong Repeat
        {
            get => NativeMethods.uv_timer_get_repeat(this);
            set => NativeMethods.uv_timer_set_repeat(this, value);
        }

        public Timer(ILibuvLogger logger, EventLooper looper, Action<Action<IntPtr>, IntPtr> queueCloseHandle) : base(logger)
        {
            this.Initialize(looper, queueCloseHandle);
        }

        private void Initialize(EventLooper looper, Action<Action<IntPtr>, IntPtr> queueCloseHandle)
        {
            this.AllocateMemory(
                looper.ThreadId,
                Handle.NativeMethods.uv_handle_size(HandleType.Timer),
                queueCloseHandle
            );

            NativeMethods.uv_timer_init(looper, this);
        }

        public Timer Start(Action callback, ulong timeout, ulong repeat)
        {
            this._callback = callback;
            NativeMethods.uv_timer_start(this, Timer._timerCallback, timeout, repeat);
            return this;
        }

        public Timer Stop()
        {
            this._callback = null;
            NativeMethods.uv_timer_stop(this);
            return this;
        }

        public Timer Again()
        {
            NativeMethods.uv_timer_again(this);
            return this;
        }

        internal static new class NativeMethods
        {
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_timer_init(EventLooper looper, Timer timer);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_timer_start(Timer timer, uv_timer_cb timerCallback, ulong timeout, ulong repeat);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_timer_stop(Timer timer);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_timer_again(Timer timer);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern void uv_timer_set_repeat(Timer timer, ulong repeat);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern ulong uv_timer_get_repeat(Timer timer);
        }
    }
}