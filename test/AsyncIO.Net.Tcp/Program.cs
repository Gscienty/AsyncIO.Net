using System;
using System.Threading;
using System.Runtime.InteropServices;
using AsyncIO.Net.Libuv;
using AsyncIO.Net.Libuv.Structs;
using AsyncIO.Net.Libuv.Requests;

namespace AsyncIO.Net.Tcp
{
    class Program
    {
        static void Main(string[] args)
        {
            EventLooper looper = new EventLooper(null);
            looper.Initialize();
            Libuv.Tcp server = new Libuv.Tcp(null, looper, null);

            server.Bind("127.0.0.1", 8000, 0);
            server.Listen(128, () =>
            {
                Libuv.Tcp client = server.Accept(looper, null);

                BufferPin pin = null;

                client.ReadStart(length =>
                {
                    pin = new BufferPin((uint)length);
                    return new Libuv.Structs.Buffer(pin.Start, (uint)length);
                },
                (buffer, length) =>
                {
                    Console.Write(System.Text.Encoding.UTF8.GetString(pin.Buffer, 0, length));
                    
                    client.ReadStop();
                    
                    WriteRequest writor = new WriteRequest(null, looper);

                    writor.Write(
                        client,
                        new ArraySegment<ArraySegment<byte>>(
                            new [] {
                                new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n")),
                                new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes("Content-Length:11\r\nContent-Type:text/html\r\n")),
                                new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes("\r\n")),
                                new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes("Hello World"))
                            }
                        ),
                        () => {
                            client.Close();
                        }
                    );
                });
            });

            looper.Run();
        }
    }
}
