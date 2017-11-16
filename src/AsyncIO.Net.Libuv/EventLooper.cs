using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace AsyncIO.Net.Libuv
{
    public class EventLooper : Handle
    {
        public long Now => NativeMethods.uv_now(this);

        public EventLooper(ILibuvLogger logger) : base(logger) { }

        public void Initialize()
        {
            this.AllocateMemory(Thread.CurrentThread.ManagedThreadId, NativeMethods.uv_loop_size());

            NativeMethods.uv_loop_init(this);
        }

        public void Run(int mode = 0) => NativeMethods.uv_run(this, 0);
        public void Stop() => NativeMethods.uv_stop(this);

        unsafe protected override bool ReleaseHandle()
        {
            IntPtr memory = this.handle;

            if (memory != IntPtr.Zero)
            {
                IntPtr gcHandlePointer = *(IntPtr*)memory;
                NativeMethods.uv_loop_close(this);
                this.handle = IntPtr.Zero;

                Handle.FreeMemory(memory, gcHandlePointer);
            }

            return true;
        }

        internal static new class NativeMethods
        {
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_loop_size();
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_loop_init(EventLooper handle);

            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_loop_close(EventLooper handle);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_run(EventLooper handle, int mode);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern void uv_stop(EventLooper handle);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern long uv_now(EventLooper loop);
        }
    }
}