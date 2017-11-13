using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public enum LoopRunMode : int { Default = 0, Once, NoWait }

    public class Loop : SafeHandle, IDisposable
    {

        public override bool IsInvalid => uv_loop_close(this.handle) != 0;
        unsafe public Loop() : base(IntPtr.Zero, true)
        {
            this.SetHandle(Marshal.AllocCoTaskMem(uv_loop_size()));
            *(IntPtr*)this.handle = GCHandle.ToIntPtr(GCHandle.Alloc(this, GCHandleType.Weak));

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
        
        unsafe protected void DestoryMemory() => this.DestoryMemory(*(IntPtr*)this.handle);

        unsafe protected void DestoryMemory(IntPtr gcHandlePtr)
        {
            if (gcHandlePtr != IntPtr.Zero)
            {
                var gcHandle = GCHandle.FromIntPtr(gcHandlePtr);
                gcHandle.Free();
            }

            Marshal.FreeCoTaskMem(this.handle);

            this.SetHandle(IntPtr.Zero);
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