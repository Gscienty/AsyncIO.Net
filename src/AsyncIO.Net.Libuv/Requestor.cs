using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using AsyncIO.Net.Libuv.Structs;

namespace AsyncIO.Net.Libuv
{
    public enum RequestorType : int
    {
        UnknowRequest = 0,
        Request,
        Connect,
        Write,
        Shutdown,
        UDPSend,
        FileSystem,
        Work,
        GetAddressInformation,
        GetNameInformation,
        RequestTypePrivate,
        RequestTypeMax,
    }
    public abstract class Requestor : Handle
    {
        protected Requestor(int size)
            : base(Thread.CurrentThread.ManagedThreadId, size, null) { }

        public void Cancel() => uv_cancel(this.handle);

        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_cancel(IntPtr handle);
        [DllImport("libuv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int uv_req_size(RequestorType type);
    }

    public class ShutdownRequestor : Requestor
    {
        public ShutdownRequestor()
            : base(uv_req_size(RequestorType.Shutdown)) { }
    }

    public class WriteRequestor: Requestor
    {
        private int _buffersCount;
        private IntPtr _buffers;
        private List<GCHandle> _pins;
        public WriteRequestor(int bufferCount) : this(bufferCount, uv_req_size(RequestorType.Write)) { }
        private WriteRequestor(int bufferCount, int requestSize)
            : base(requestSize + Marshal.SizeOf<UvBuffer>() * bufferCount)
        {
            this._buffersCount = bufferCount;
            this._buffers = this.handle + requestSize;

            this._pins = new List<GCHandle>(this._buffersCount + 1);
        }

        public void Write(Stream handle, ArraySegment<ArraySegment<byte>> buffers, Action<int> writed) =>
            this.Write(handle, buffers, null, writed);

        unsafe public void Write(
            Stream handle,
            ArraySegment<ArraySegment<byte>> buffers,
            Stream sendHandle,
            Action<int> writed)
        {
            this._pins.Add(GCHandle.Alloc(this, GCHandleType.Normal));

            UvBuffer* buffersPtr = (UvBuffer*)this._buffers;
            int buffersCount = buffers.Count;

            if (buffersCount > this._buffersCount)
            {
                UvBuffer[] buffersArray = new UvBuffer[buffersCount];
                GCHandle gcHandle = GCHandle.Alloc(buffersArray, GCHandleType.Pinned);
                this._pins.Add(gcHandle);
                buffersPtr = (UvBuffer*)gcHandle.AddrOfPinnedObject();
            }

            for (int index = 0; index < buffersCount; index++)
            {
                ArraySegment<byte> buffer = buffers.Array[buffers.Offset + index];

                GCHandle gcHandle = GCHandle.Alloc(buffer.Array, GCHandleType.Pinned);
                this._pins.Add(gcHandle);

                buffersPtr[index] = new UvBuffer(gcHandle.AddrOfPinnedObject() + buffer.Offset, (uint)buffer.Count);
            }

            if (sendHandle == null)
            {
                Stream.uv_write(
                    this.handle,
                    handle.DangerousGetHandle(),
                    buffersPtr,
                    (uint)buffersCount,
                    (ptr, state) =>
                    {
                        this.UnpinGCHandles();
                        writed(state);
                    }
                );
            }
            else
            {
                Stream.uv_write2(
                    this.handle,
                    handle.DangerousGetHandle(),
                    buffersPtr,
                    (uint)buffersCount,
                    sendHandle.DangerousGetHandle(),
                    (ptr, state) =>
                    {
                        this.UnpinGCHandles();
                        writed(state);
                    }
                );
            }
        }

        private void UnpinGCHandles()
        {
            List<GCHandle> pins = this._pins;
            int pinsCount = pins.Count;

            for (int index = 0; index < pinsCount; index++)
            {
                pins[index].Free();
            }

            pins.Clear();
        }
    }
}