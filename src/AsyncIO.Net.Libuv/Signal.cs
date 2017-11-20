using System;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public class Signal : Handle
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void uv_signal_cb(IntPtr handle, int signnum);

        private readonly static uv_signal_cb _signalCallback = (handle, signnum) =>
        {
            Signal signal = Handle.FromIntPtr<Signal>(handle);
            signal._callback(signnum);
        };

        private GCHandle _signalKeeper;
        private Action<int> _callback;

        public Signal(ILibuvLogger logger, EventLooper looper, Action<Action<IntPtr>, IntPtr> queueCloseHandle) : base(logger)
        {
            this.Initialize(looper, queueCloseHandle);
        }

        private void Initialize(EventLooper looper, Action<Action<IntPtr>, IntPtr> queueCloseHandle)
        {
            this.AllocateMemory(
                looper.ThreadId,
                Handle.NativeMethods.uv_handle_size(HandleType.Signal),
                queueCloseHandle
            );

            NativeMethods.uv_signal_init(looper, this);
        }

        public Signal Start(int signnum, Action<int> callback)
        {
            if (this._signalKeeper.IsAllocated)
            {
                throw new InvalidOperationException("TODO: Start may not be called more than once");
            }

            try
            {
                this._callback = callback;
                this._signalKeeper = GCHandle.Alloc(this, GCHandleType.Normal);

                NativeMethods.uv_signal_start(this, Signal._signalCallback, signnum);

                return this;
            }
            catch
            {
                this._callback = null;

                if (this._signalKeeper.IsAllocated)
                {
                    this._signalKeeper.Free();
                }

                throw;
            }
        }
        public Signal StartOneShot(int signnum, Action<int> callback)
        {
            if (this._signalKeeper.IsAllocated)
            {
                throw new InvalidOperationException("TODO: Start may not be called more than once");
            }

            try
            {
                this._callback = callback;
                this._signalKeeper = GCHandle.Alloc(this, GCHandleType.Normal);

                NativeMethods.uv_signal_start_oneshot(this, Signal._signalCallback, signnum);

                return this;
            }
            catch
            {
                this._callback = null;

                if (this._signalKeeper.IsAllocated)
                {
                    this._signalKeeper.Free();
                }

                throw;
            }
        }

        public Signal Stop()
        {
            this._callback = null;
            if (this._signalKeeper.IsAllocated)
            {
                this._signalKeeper.Free();
            }

            NativeMethods.uv_signal_stop(this);

            return this;
        }

        internal static new class NativeMethods
        {
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_signal_init(EventLooper looper, Signal signal);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_signal_start(Signal handle, uv_signal_cb callback, int signnum);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_signal_start_oneshot(Signal handle, uv_signal_cb callback, int signnum);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_signal_stop(Signal handle);
        }
    }
}