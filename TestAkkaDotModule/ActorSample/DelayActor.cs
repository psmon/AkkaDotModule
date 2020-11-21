using Akka.Actor;
using Akka.Event;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestAkkaDotModule.ActorSample
{
    public class DelayActor : ReceiveActor
    {
        private readonly ILoggingAdapter logger = Context.GetLogger();

        private string MyName { get; set; }

        public DelayActor(string name)
        {
            MyName = name;
            ReceiveAsync<string>(async message =>
            {
                Thread thread = Thread.CurrentThread;
                string currentTime = DateTime.Now.Second.ToString();
                string inComeMessage = $"[Sec-{currentTime}] [TID-{thread.ManagedThreadId}] [{MyName}] : {message}";
                Console.WriteLine($"{inComeMessage}");
                //지연테스트를 위한 임의 블락처리
                Task.Delay(1500).Wait();
            });
        }
    }
}
