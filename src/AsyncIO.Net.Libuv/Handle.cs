using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public enum HandleType : int 
    {
        UnknowHandle = 0,
        Async,
        Check,
        FileSystemEvent,
        FileSystemPoll,
        Handle,
        Idle,
        NamedPipe,
        Poll,
        Prepare,
        Process,
        Stream,
        TCP,
        Timer,
        TTY,
        UDP,
        Signal,
        File,
        HandleTypeMax
    }

    public abstract class Handle : SafeHandle
    {
        private int _threadId;
        private Action<Action<IntPtr>, IntPtr> _queueCloseHandle;

        public override bool IsInvalid => this.handle == IntPtr.Zero;
        public new bool IsClosed => this.IsInvalid;
        public bool IsClosing => this.IsInvalid ? false : uv_is_closing(this.handle) != 0;

        unsafe protected Handle(
            int threadId,
            int size,
            Action<Action<IntPtr>, IntPtr> queueCloseHandle,
            GCHandleType handleType = GCHandleType.Weak) : base(IntPtr.Zero, true)
        {
            this._threadId = threadId;
            this._queueCloseHandle = queueCloseHandle;

            this.SetHandle(Marshal.AllocCoTaskMem(size));
            *(IntPtr*)this.handle = GCHandle.ToIntPtr(GCHandle.Alloc(this, handleType));
        }

        protected override bool ReleaseHandle()
        {
            var memory = this.handle;

            if (memory != IntPtr.Zero)
            {
                this.SetHandle(IntPtr.Zero);

                if (Thread.CurrentThread.ManagedThreadId == this._threadId)
                {
                    uv_close(memory, handle => DestoryMemory());
                }
                else if (this._queueCloseHandle != null)
                {
                    this._queueCloseHandle(
                        truthMemory => uv_close(truthMemory, handle => DestoryMemory()),
                        memory
                    );
                }
                else
                {
                    return false;
                }
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
        internal static extern int uv_is_closing(IntPtr handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void uv_close(IntPtr handle, CloseCallback callback);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_handle_size(HandleType type);
    }
}