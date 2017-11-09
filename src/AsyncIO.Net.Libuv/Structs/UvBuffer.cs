using System;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv.Structs
{
    public struct UvBuffer
    {
        private readonly IntPtr _field0;
        private readonly IntPtr _field1;

        public UvBuffer(IntPtr basePtr, uint length)
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