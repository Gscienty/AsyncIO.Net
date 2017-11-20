using System;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public enum TTYFileDescription : int
    {
        StandardIn = 0,
        StandardOut = 1,
        StandardError = 2
    }

    public enum TTYMode : int { Normal = 0, Row, IO }

    public class TTY : Stream
    {
        public (int width, int height) WindowSize
        {
            get
            {
                int width = 0;
                int height = 0;
                NativeMethods.uv_tty_get_winsize(this, ref width, ref height);
                return (width, height);
            }
        }

        public TTY(
            ILibuvLogger logger,
            EventLooper looper,
            TTYFileDescription fileDescription,
            bool readable,
            Action<Action<IntPtr>, IntPtr> queueCloseHandle) : base(logger)
        {
            this.Initialize(looper, fileDescription, readable, queueCloseHandle);
        }

        private void Initialize(
            EventLooper looper,
            TTYFileDescription fileDescription,
            bool readable,
            Action<Action<IntPtr>, IntPtr> queueCloseHandle)
        {
            this.AllocateMemory(
                looper.ThreadId,
                Handle.NativeMethods.uv_handle_size(HandleType.TTY),
                queueCloseHandle
            );

            NativeMethods.uv_tty_init(looper, this, (IntPtr)fileDescription, readable ? 1 : 0);
        }

        public TTY SetMode(TTYMode mode)
        {
            NativeMethods.uv_tty_set_mode(this, mode);
            return this;
        }

        public TTY ResetMode()
        {
            NativeMethods.uv_tty_reset_mode();
            return this;
        }

        internal static new class NativeMethods
        {
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_tty_init(EventLooper looper, TTY tty, IntPtr fd, int readable);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_tty_set_mode(TTY handle, TTYMode mode);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_tty_reset_mode();
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_tty_get_winsize(TTY handle, ref int width, ref int height);
        }
    }
}