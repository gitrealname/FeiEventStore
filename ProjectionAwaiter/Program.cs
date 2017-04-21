using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FeiEventStore.Core;
using FeiEventStore.EventQueue;

namespace ProjectionAwaiter
{
    class Program
    {
        static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        private static readonly EventQueueAwaiter _awaiter = new EventQueueAwaiter();

        private static async Task MainAsync()
        {


            for(var i = 0; i < 9; i++)
            {
                var id = i;
                var thread = new Thread(() => BackgroundWorker(id).GetAwaiter().GetResult());
                thread.IsBackground = true; // Keep this thread from blocking process shutdown
                thread.Start();
            }

            while(true)
            {
                var key = Console.ReadKey();
                var code = (int)key.KeyChar;
                if(code == (int)ConsoleKey.Escape)
                {
                    Console.WriteLine("exiting...");
                    break;
                }
                var ver = code - 48;
                _awaiter.Post(new TypeId("test"), ver);
            }

            await Task.Delay(1000);
        }

        private static async Task BackgroundWorker(int id)
        {
            var typeId = new TypeId("test");
            for(var i = 1; i < 10; i++)
            {
                if(await _awaiter.AwaitAsync(typeId, i, 10000))
                {
                    Console.WriteLine($"{id}: test reached version: {i}");
                } else
                {
                    Console.WriteLine($"{id}: test timed out");
                    i--;
                }
            }
        }
    }
}
