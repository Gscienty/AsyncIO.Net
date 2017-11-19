using System;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public class Idle : Handle
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void uv_idle_cb(IntPtr handle);

        private static readonly uv_idle_cb _idleCallback = handle =>
        {
            Idle idle = Handle.FromIntPtr<Idle>(handle);

            idle._callback();
        };

        private Action _callback;

        public Idle(ILibuvLogger logger, EventLooper looper, Action<Action<IntPtr>, IntPtr> queueCloseHandle) : base(logger)
        {
            this.Initialize(looper, queueCloseHandle);
        }

        private void Initialize(EventLooper looper, Action<Action<IntPtr>, IntPtr> queueCloseHandle)
        {
            this.AllocateMemory(
                looper.ThreadId,
                Handle.NativeMethods.uv_handle_size(HandleType.Idle),
                queueCloseHandle
            );

            NativeMethods.uv_idle_init(looper, this);
        }

        public void Start(Action idle)
        {
            this._callback = idle;

            NativeMethods.uv_idle_start(this, Idle._idleCallback);
        }

        public void Stop()
        {
            this._callback = null;

            NativeMethods.uv_idle_stop(this);
        }

        internal static new class NativeMethods
        {
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_idle_init(EventLooper looper, Idle handle);

            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_idle_start(Idle handle, uv_idle_cb callback);

            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_idle_stop(Idle handle);
        }
    }
}