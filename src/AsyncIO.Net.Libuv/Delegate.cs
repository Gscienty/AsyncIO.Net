using System;
using System.Runtime.InteropServices;
using AsyncIO.Net.Libuv.Structs;

namespace AsyncIO.Net.Libuv
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void AllocateCallback(IntPtr handle, uint size, out UvBuffer buffer);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void CloseCallback(IntPtr handle);
}