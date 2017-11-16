using System;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Buffer
    {
        private readonly IntPtr _field0;
        private readonly IntPtr _field1;

        public IntPtr BasePointer => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? this._field1 : this._field0;

        public Buffer(IntPtr basePtr, uint length)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                this._field0 = (IntPtr) length;
                this._field1 = basePtr;
            }
            else
            {
                this._field0 = basePtr;
                this._field1 = (IntPtr) length;
            }
        }
    }
}