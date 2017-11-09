using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public enum HandleType : int 
    {
        UV_UNKNOWN_HANDLE = 0,
        UV_ASYNC,
        UV_CHECK,
        UV_FS_EVENT,
        UV_FS_POLL,
        UV_HANDLE,
        UV_IDLE,
        UV_NAMED_PIPE,
        UV_POLL,
        UV_PREPARE,
        UV_PROCESS,
        UV_STREAM,
        UV_TCP,
        UV_TIMER,
        UV_TTY,
        UV_UDP,
        UV_SIGNAL,
        UV_FILE,
        UV_HANDLE_TYPE_MAX
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