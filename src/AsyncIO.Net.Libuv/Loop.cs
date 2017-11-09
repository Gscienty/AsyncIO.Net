using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public enum LoopRunMode : int { Default = 0, Once, NoWait }

    public class Loop : Handle
    {
        public Loop() : base(Thread.CurrentThread.ManagedThreadId, uv_loop_size(), null)
        {
            uv_loop_init(this.handle);
        }

        public void Run(LoopRunMode runMode = LoopRunMode.Default) => uv_run(this.handle, (int)runMode);

        protected override bool ReleaseHandle()
        {
            if (this.IsInvalid == false)
            {
                uv_loop_close(this.handle);
                this.DestoryMemory();
            }

            return true;
        }

        
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_loop_size();
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_loop_init(IntPtr handle);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_loop_close(IntPtr handle);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_run(IntPtr handle, int mode);
    }
}