using System;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    [Flags]
    public enum FileSystemEventType : uint
    {
        Rename = 1,
        Change = 2
    }

    [Flags]
    public enum FileSystemEventFlags : uint
    {
        WatchEntry = 1,
        Stat = 2,
        Recursive = 4
    }

    public class FileSystemEvent : Handle
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void uv_fs_event_cb(IntPtr handle, string fileName, FileSystemEventType events, int status);

        private static readonly uv_fs_event_cb _fileSystemEventCallback = (handle, fileName, events, status) =>
        {
            FileSystemEvent fsEvent = Handle.FromIntPtr<FileSystemEvent>(handle);

            fsEvent._callback(fileName, events);
        };

        private Action<string, FileSystemEventType> _callback;
        private GCHandle _startKeeper;

        public string Path
        {
            get
            {
                IntPtr path = IntPtr.Zero;
                int length = 256;
                NativeMethods.uv_fs_event_getpath(this, out path, ref length);

                return Marshal.PtrToStringAnsi(path);
            }
        }

        public FileSystemEvent(ILibuvLogger logger, EventLooper looper, Action<Action<IntPtr>, IntPtr> queueCloseHandle) : base(logger)
        {
            this.Initialize(looper, queueCloseHandle);
        }

        private void Initialize(EventLooper looper, Action<Action<IntPtr>, IntPtr> queueCloseHandle)
        {
            this.AllocateMemory(
                looper.ThreadId,
                Handle.NativeMethods.uv_handle_size(HandleType.FileSystemEvent),
                queueCloseHandle
            );

            NativeMethods.uv_fs_event_init(looper, this);
        }

        public FileSystemEvent Start(
            string path,
            FileSystemEventFlags flags,
            Action<string, FileSystemEventType> callback)
        {
            if (this._startKeeper.IsAllocated)
            {
                throw new InvalidOperationException("TODO: Start may not be called more than once");
            }
            try
            {
                this._callback = callback;
                this._startKeeper = GCHandle.Alloc(this, GCHandleType.Normal);

                NativeMethods.uv_fs_event_start(
                    this,
                    FileSystemEvent._fileSystemEventCallback,
                    path,
                    flags
                );

                return this;
            }
            catch
            {
                this._callback = null;

                if (this._startKeeper.IsAllocated)
                {
                    this._startKeeper.Free();
                }
                throw;
            }
        }

        public FileSystemEvent Stop()
        {
            this._callback = null;
            if (this._startKeeper.IsAllocated)
            {
                this._startKeeper.Free();
            }

            NativeMethods.uv_fs_event_stop(this);
            return this;
        }

        internal static new class NativeMethods
        {
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_fs_event_init(EventLooper looper, FileSystemEvent fsEvent);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_fs_event_start(FileSystemEvent handle, uv_fs_event_cb callback, string path, FileSystemEventFlags flags);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_fs_event_stop(FileSystemEvent handle);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int uv_fs_event_getpath(FileSystemEvent handle, out IntPtr buffer, ref int size);
        }
    }
}