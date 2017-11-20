using System;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public class NamedPipe : Stream
    {
        public string SocketName
        {
            get
            {
                IntPtr name = IntPtr.Zero;
                int length = 256;
                NativeMethods.uv_pipe_getsockname(this, out name, ref length);
                return Marshal.PtrToStringAnsi(name);
            }
        }

        public string PeerName
        {
            get
            {
                IntPtr name = IntPtr.Zero;
                int length = 256;
                NativeMethods.uv_pipe_getpeername(this, out name, ref length);
                return Marshal.PtrToStringAnsi(name);
            }
        }

        public int PendingCount => NativeMethods.uv_pipe_pending_count(this);

        public HandleType PendingType => (HandleType)NativeMethods.uv_pipe_pending_type(this);

        public NamedPipe(ILibuvLogger logger, EventLooper looper, bool ipc, Action<Action<IntPtr>, IntPtr> queueCloseHandle) : base(logger)
        {
            this.Initialize(looper, ipc, queueCloseHandle);
        }

        private void Initialize(EventLooper looper, bool ipc, Action<Action<IntPtr>, IntPtr> queueCloseHandle)
        {
            this.AllocateMemory(
                looper.ThreadId,
                Handle.NativeMethods.uv_handle_size(HandleType.NamedPipe),
                queueCloseHandle
            );

            NativeMethods.uv_pipe_init(looper, this, ipc ? 1 : 0);
        }

        public NamedPipe Bind(string name)
        {
            NativeMethods.uv_pipe_bind(this, name);
            return this;
        }

        public NamedPipe SetInstances(int count)
        {
            NativeMethods.uv_pipe_pending_instances(this, count);
            return this;
        }

        public NamedPipe Chmod(int flags)
        {
            NativeMethods.uv_pipe_chmod(this, flags);
            return this;
        }

        internal static new class NativeMethods
        {
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_pipe_init(EventLooper looper, NamedPipe handle, int ipc);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_pipe_bind(NamedPipe handle, string name);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_pipe_getsockname(NamedPipe handle, out IntPtr buffer, ref int length);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_pipe_getpeername(NamedPipe handle, out IntPtr buffer, ref int length);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern void uv_pipe_pending_instances(NamedPipe handle, int count);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_pipe_pending_count(NamedPipe handle);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_pipe_pending_type(NamedPipe handle);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_pipe_chmod(NamedPipe handle, int flags);
        }
    }
}