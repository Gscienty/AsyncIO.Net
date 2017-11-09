using System;
using AsyncIO.Net.Libuv;

namespace AsyncIO.Net.Timer
{
    class Program
    {
        static void Main(string[] args)
        {
            Loop loop = new Loop();

            Libuv.Timer timer = new Libuv.Timer(loop);

            timer.Schedule(() => 
            {
                Console.WriteLine(timer.Interval);
                timer.Interval += 100;
                if (timer.Interval == 2000)
                {
                    timer.Stop();
                }
            }, 1000);

            timer.Start();

            loop.Run();
        }
    }
}
