using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using AsyncIO.Net.Libuv.Structs;

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

    public abstract class Handle : SafeHandle, IDisposable
    {
        private int _threadId;
        private Action<Action<IntPtr>, IntPtr> _queueCloseHandle;

        public override bool IsInvalid => this.handle == IntPtr.Zero;
        public new bool IsClosed => this.IsInvalid;
        public bool IsClosing => this.IsInvalid ? false : uv_is_closing(this.handle) != 0;
        public virtual bool IsActive => uv_is_active(this.handle) != 0;
        public bool HasReference => uv_has_ref(this.handle) != 0;
        public int SendBufferSize
        {
            set => uv_send_buffer_size(this.handle, ref value);
            get
            {
                int val = 0;
                return uv_send_buffer_size(this.handle, ref val);
            }
        }

        public int ReceiveBufferSize
        {
            set => uv_recv_buffer_size(this.handle, ref value);
            get
            {
                int val = 0;
                return uv_recv_buffer_size(this.handle, ref val);
            }
        }

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

        public void Reference() => uv_ref(this.handle);
        public void UnReference() => uv_unref(this.handle);


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
    
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void uv_alloc_cb(IntPtr handle, uint suggestedSize, ref UvBuffer buffer);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void uv_close_cb(IntPtr handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_is_closing(IntPtr handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void uv_close(IntPtr handle, uv_close_cb callback);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_handle_size(HandleType type);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_is_active(IntPtr handle);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void uv_ref(IntPtr handle);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void uv_unref(IntPtr handle);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_has_ref(IntPtr handle);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_send_buffer_size(IntPtr handle, ref int value);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_recv_buffer_size(IntPtr handle, ref int value);
    }
}