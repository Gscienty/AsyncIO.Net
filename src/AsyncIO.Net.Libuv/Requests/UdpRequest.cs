using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using AsyncIO.Net.Libuv.Structs;

namespace AsyncIO.Net.Libuv.Requests
{
    public class UdpRequest : Request
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void uv_udp_send_cb(IntPtr handle, int status);

        private readonly static uv_udp_send_cb _udpSendCallback = (handle, status) =>
        {
            UdpRequest request = Handle.FromIntPtr<UdpRequest>(handle);

            request._callback();
        };

        private IntPtr _buffersPointer;
        private List<BufferPin> _sendBuffers;
        private Action _callback;

        public UdpRequest(ILibuvLogger logger, EventLooper looper) : base(logger)
        {
            this.AllocateMemory(
                looper.ThreadId,
                Request.NativeMethods.uv_req_size(RequestType.UDP_SEND)
            );

            this._buffersPointer = IntPtr.Zero;
            this._sendBuffers = new List<BufferPin>();
        }

        unsafe public void Send(Udp udp, string ip, int port, ArraySegment<ArraySegment<byte>> buffers, Action sended)
        {
            this._callback = sended;

            this.FillngBuffers(buffers);
            NativeSocketAddress address = NativeSocketAddress.GetIPv4(ip, port);

            NativeMethods.uv_udp_send(
                this,
                udp,
                (Structs.Buffer*)this._buffersPointer,
                buffers.Count,
                ref address,
                UdpRequest._udpSendCallback
            );
        }
        
        unsafe private void FillngBuffers(ArraySegment<ArraySegment<byte>> buffers)
        {
            int buffersCount = buffers.Count;

            int buffersSize = Marshal.SizeOf<Structs.Buffer>() * buffersCount;
            this._buffersPointer = Marshal.AllocCoTaskMem(buffersSize);

            for (int index = 0; index < buffersCount; index++)
            {
                ArraySegment<byte> buffer = buffers.Array[buffers.Offset + index];
                BufferPin bufferPin = new BufferPin(buffer.Array, (IntPtr)buffer.Offset, (IntPtr)buffer.Count);
                this._sendBuffers.Add(bufferPin);

                ((Structs.Buffer*)this._buffersPointer)[index] = new Structs.Buffer(bufferPin.Start, (uint)bufferPin.Count);
            }
        }

        private void FreeBuffers()
        {
            Marshal.FreeCoTaskMem(this._buffersPointer);
            this._sendBuffers.ForEach(bufferPin =>
            {
                bufferPin.Dispose();
            });
            this._sendBuffers.Clear();
        }

        internal static new class NativeMethods
        {
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            unsafe internal static extern int uv_udp_send(
                UdpRequest request,
                Udp udp,
                Structs.Buffer* buffers,
                int nbufs,
                ref NativeSocketAddress address,
                uv_udp_send_cb callback);
        }
    }
}