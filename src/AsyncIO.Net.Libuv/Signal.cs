using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace AsyncIO.Net.Libuv
{
    public class Signal : Handle
    {
        private Loop _loop;
        protected Signal(Loop loop)
            : base(Thread.CurrentThread.ManagedThreadId, uv_handle_size(HandleType.Signal), null)
        {
            this._loop = loop;
        }
    }
}