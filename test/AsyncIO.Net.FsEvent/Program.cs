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

            FileSystemEvent fsEvent = new FileSystemEvent(
                null,
                looper,
                null
            );

            fsEvent.Start(
                @"F:\",
                FileSystemEventFlags.WatchEntry,
                (name, eventType) =>
                {
                    Console.WriteLine(name);
                    Console.WriteLine(eventType);
                }
            );
            looper.Run();
        }
    }
}
