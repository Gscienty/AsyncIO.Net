using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public abstract class Handle : SafeHandle
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void uv_close_cb(IntPtr handle);

        protected readonly ILibuvLogger _logger;
        protected readonly GCHandleType _handleType;
        private Action<Action<IntPtr>, IntPtr> _queueCloseHandle;
        private static readonly uv_close_cb _freeMemory = handle => Handle.FreeMemory(handle);

        public override bool IsInvalid => this.handle == IntPtr.Zero;
        public int ThreadId { get; protected set; }
        public IntPtr Handler => this.handle;

        protected Handle(ILibuvLogger logger, GCHandleType handleType = GCHandleType.Weak) : base(IntPtr.Zero, true)
        {
            this._handleType = handleType;
            this._logger = logger;
        }

        public void Reference() => NativeMethods.uv_ref(this);
        public void Unreference() => NativeMethods.uv_unref(this);

        unsafe protected void AllocateMemory(int threadId, int size, Action<Action<IntPtr>, IntPtr> queueCloseHandle)
        {
            this.AllocateMemory(threadId, size);

            this._queueCloseHandle = queueCloseHandle;
        }

        unsafe protected void AllocateMemory(int threadId, int size)
        {
            this.ThreadId = threadId;

            this.handle = Marshal.AllocCoTaskMem(size);
            *(IntPtr*)this.handle = GCHandle.ToIntPtr(GCHandle.Alloc(this, this._handleType));
        }

        unsafe protected static void FreeMemory(IntPtr memory)
        {
            IntPtr gcHandlePointer = *(IntPtr*)memory;
            Handle.FreeMemory(memory, gcHandlePointer);
        }

        unsafe protected static void FreeMemory(IntPtr memory, IntPtr gcHandlePointer)
        {
            if (gcHandlePointer != IntPtr.Zero)
            {
                GCHandle gcHandle = GCHandle.FromIntPtr(gcHandlePointer);
                gcHandle.Free();
            }
            Marshal.FreeCoTaskMem(memory);
        }

        unsafe public static T FromIntPtr<T>(IntPtr handle) where T : Handle
        {
            GCHandle gcHandle = GCHandle.FromIntPtr(*(IntPtr*)handle);
            return gcHandle.Target as T;
        }

        protected override bool ReleaseHandle()
        {
            var memory = this.handle;
            if (memory != IntPtr.Zero)
            {
                this.handle = IntPtr.Zero;

                if (Thread.CurrentThread.ManagedThreadId == this.ThreadId)
                {
                    NativeMethods.uv_close(memory, Handle._freeMemory);
                }
                else if (this._queueCloseHandle != null)
                {
                    this._queueCloseHandle(
                        otherThreadsMemory => NativeMethods.uv_close(otherThreadsMemory, Handle._freeMemory), 
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

        protected static class NativeMethods
        {

            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern void uv_ref(Handle handle);

            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern void uv_unref(Handle handle);

            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_fileno(Handle handle, ref IntPtr socket);

            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern void uv_close(IntPtr handle, Handle.uv_close_cb close_cb);

            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            public static extern int uv_handle_size(HandleType handleType);
        }
    }
}