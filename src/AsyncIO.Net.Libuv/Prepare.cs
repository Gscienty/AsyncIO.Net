using System;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public class Prepare : Handle
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void uv_prepare_cb(IntPtr handle);

        private static readonly uv_prepare_cb _prepareCallback = handle =>
        {
            Prepare prepare = Handle.FromIntPtr<Prepare>(handle);

            prepare._callback();
        };

        private Action _callback;

        public Prepare(ILibuvLogger logger, EventLooper looper, Action<Action<IntPtr>, IntPtr> queueCloseHandle) : base(logger)
        {
            this.Initialize(looper, queueCloseHandle);
        }

        private void Initialize(EventLooper looper, Action<Action<IntPtr>, IntPtr> queueCloseHandle)
        {
            this.AllocateMemory(
                looper.ThreadId,
                Handle.NativeMethods.uv_handle_size(HandleType.PREPARE),
                queueCloseHandle
            );

            NativeMethods.uv_prepare_init(looper, this);
        }

        public void Start(Action prepare)
        {
            this._callback = prepare;

            NativeMethods.uv_prepare_start(this, Prepare._prepareCallback);
        }

        public void Stop()
        {
            this._callback = null;

            NativeMethods.uv_prepare_stop(this);
        }

        internal static new class NativeMethods
        {
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_prepare_init(EventLooper looper, Prepare handle);

            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_prepare_start(Prepare handle, uv_prepare_cb callback);

            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_prepare_stop(Prepare handle);
        }
    }
}