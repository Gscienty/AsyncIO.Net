using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncIO.Net.Libuv;

namespace AsyncIO.Net.Prepare
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Loop loop = new Loop();
            Libuv.Prepare prepare = new Libuv.Prepare(loop);

            prepare.Schedule = () => {
                Console.WriteLine("Hello");
            };

            prepare.Start();
            loop.Run();
        }
    }
}