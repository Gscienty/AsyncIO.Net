using System;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public class Async : Handle
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void uv_async_cb(IntPtr handle);

        private readonly static uv_async_cb _asyncCallback = handle =>
        {
            Async async = Handle.FromIntPtr<Async>(handle);
            async._callback();
        };

        private Action _callback;

        public Async(ILibuvLogger logger, EventLooper looper, Action callback, Action<Action<IntPtr>, IntPtr> queueCloseHandle) : base(logger)
        {
            this.Initialize(looper, callback, queueCloseHandle);
        }

        private void Initialize(
            EventLooper looper,
            Action callback,
            Action<Action<IntPtr>, IntPtr> queueCloseHandle)
        {
            this.AllocateMemory(
                looper.ThreadId,
                Handle.NativeMethods.uv_handle_size(HandleType.Async),
                queueCloseHandle
            );

            this._callback = callback;

            NativeMethods.uv_async_init(looper, this, Async._asyncCallback);
        }

        public void Send()
        {
            NativeMethods.uv_async_send(this);
        }

        internal static new class NativeMethods
        {
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_async_init(EventLooper looper, Async handle, uv_async_cb callback);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_async_send(Async handle);
        }
    }
}