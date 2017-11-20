using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using AsyncIO.Net.Libuv;
using AsyncIO.Net.Libuv.Structs;
using AsyncIO.Net.Libuv.Requests;

namespace AsyncIO.Net.TTY
{
    class Program
    {
        static void Main(string[] args)
        {
            EventLooper looper = new EventLooper(null);
            looper.Initialize();

            Libuv.TTY tty = new Libuv.TTY(
                null,
                looper,
                TTYFileDescription.StandardIn,
                true,
                null
            );

            Console.WriteLine(tty.WindowSize);
            BufferPin pin = null;
            tty.ReadStart(length =>
            {
                pin = new BufferPin((uint)length);

                return new Libuv.Structs.Buffer(pin.Start, (uint)length);
            },
            (buffer, length) =>
            {
                Console.Write(System.Text.Encoding.UTF8.GetString(pin.Buffer));
            });

            looper.Run();
        }
    }
}
