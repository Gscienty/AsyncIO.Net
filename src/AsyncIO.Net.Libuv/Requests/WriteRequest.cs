using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AsyncIO.Net.Libuv;
using AsyncIO.Net.Libuv.Structs;

namespace AsyncIO.Net.Libuv.Requests
{
    public class WriteRequest : Request
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void uv_write_cb(IntPtr req, int status);

        private static readonly uv_write_cb _writeCallback = (requestPointer, status) => 
        {
            WriteRequest request = Handle.FromIntPtr<WriteRequest>(requestPointer);
            request.FreeBuffers();
            request._callback();
        };

        private IntPtr _buffersPointer;
        private List<BufferPin> _writeBuffers;
        private Action _callback;
        public WriteRequest(ILibuvLogger logger, EventLooper looper) : base(logger)
        {
            this.AllocateMemory(
                looper.ThreadId,
                Request.NativeMethods.uv_req_size(RequestType.WRITE)
            );
            this._buffersPointer = IntPtr.Zero;
            this._writeBuffers = new List<BufferPin>();
        }

        unsafe public void Write2(Stream receiver, Stream sender, ArraySegment<ArraySegment<byte>> buffers, Action writed)
        {
            this._callback = writed;
            this.FillngBuffers(buffers);
            
            NativeMethods.uv_write2(
                this,
                receiver,
                (Structs.Buffer*)this._buffersPointer,
                buffers.Count,
                sender,
                WriteRequest._writeCallback
            );
        }

        unsafe public void Write(Stream receiver, ArraySegment<ArraySegment<byte>> buffers, Action writed)
        {
            this._callback = writed;

            this.FillngBuffers(buffers);

            NativeMethods.uv_write(
                this,
                receiver,
                (Structs.Buffer*)this._buffersPointer,
                buffers.Count,
                WriteRequest._writeCallback
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
                this._writeBuffers.Add(bufferPin);

                ((Structs.Buffer*)this._buffersPointer)[index] = new Structs.Buffer(bufferPin.Start, (uint)bufferPin.Count);
            }
        }

        private void FreeBuffers()
        {
            Marshal.FreeCoTaskMem(this._buffersPointer);
            this._writeBuffers.ForEach(bufferPin =>
            {
                bufferPin.Dispose();
            });
            this._writeBuffers.Clear();
        }

        internal static new class NativeMethods
        {
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            unsafe public static extern int uv_write(Request req, Stream handle, Structs.Buffer* bufs, int nbufs, uv_write_cb cb);
            [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
            unsafe public static extern int uv_write2(Request req, Stream handle, Structs.Buffer* bufs, int nbufs, Stream sendHandle, uv_write_cb cb);
        }
    }
}