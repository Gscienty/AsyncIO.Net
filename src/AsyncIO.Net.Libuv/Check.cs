using System;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public class Check : Handle
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void uv_check_cb(IntPtr handle);

        private static readonly uv_check_cb _checkCallback = handle =>
        {
            Check check = Handle.FromIntPtr<Check>(handle);

            check._callback();
        };

        private Action _callback;

        public Check(ILibuvLogger logger, EventLooper looper, Action<Action<IntPtr>, IntPtr> queueCloseHandle) : base(logger)
        {
            this.Initialize(looper, queueCloseHandle);
        }

        private void Initialize(EventLooper looper, Action<Action<IntPtr>, IntPtr> queueCloseHandle)
        {
            this.AllocateMemory(
                looper.ThreadId,
                Handle.NativeMethods.uv_handle_size(HandleType.CHECK),
                queueCloseHandle
            );

            NativeMethods.uv_check_init(looper, this);
        }

        public void Start(Action check)
        {
            this._callback = check;

            NativeMethods.uv_check_start(this, Check._checkCallback);
        }

        public void Stop()
        {
            this._callback = null;

            NativeMethods.uv_check_stop(this);
        }

        internal static new class NativeMethods
        {
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_check_init(EventLooper looper, Check handle);

            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_check_start(Check handle, uv_check_cb callback);

            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_check_stop(Check handle);
        }
    }
}