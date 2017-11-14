using System;
using System.Net;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv.Structs
{
    [StructLayout(LayoutKind.Sequential, Size = 32)]
    internal struct NativeSocketAddress
    {
        unsafe public IPEndPoint IPEndPoint
        {
            get
            {
                int port = ((int)(this._field0 & 0x0000000000FF0000) >> 8) | ((int)(this._field0 & 0x00000000FF000000) >> 24);
                int family = (int)(this._field0 & 0x00000000000000FF);
                if (family == 2)
                {
                    return new IPEndPoint((this._field0 >> 32) & 0x00000000FFFFFFFF, port);
                }
                else if (this.IsIPv4MappedToIPv6())
                {
                    return new IPEndPoint((this._field2 >> 32) & 0x00000000FFFFFFFF, port);                    
                }
                else
                {
                    byte[] bytes = new byte[16];
                    fixed (byte* b = bytes)
                    {
                        *((long*)b) = this._field1;
                        *((long*)(b + 8)) = this._field2;
                    }

                    return new IPEndPoint(new IPAddress(bytes, this._field3 & 0x00000000FFFFFFFF), port);
                }
            }
        }

        private readonly long _field0;
        private readonly long _field1;
        private readonly long _field2;
        private readonly long _field3;

        public static NativeSocketAddress GetIPv4(string ip, int port)
        {
            NativeSocketAddress address;
            Stream.uv_ip4_addr(ip, port, out address);

            return address;
        }

        public static NativeSocketAddress GetIPv6(string ip, int port)
        {
            NativeSocketAddress address;
            Stream.uv_ip6_addr(ip, port, out address);

            return address;
        }

        private bool IsIPv4MappedToIPv6()
        {
            if (this._field0 != 0)
            {
                return false;
            }

            return (this._field2 & 0x00000000FFFFFFFF) == 0x00000000FFFF0000;
        }
    }
}