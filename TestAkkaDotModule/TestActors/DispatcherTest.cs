using Akka.Actor;
using Akka.Event;
using AkkaNetCoreTest;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace TestAkkaDotModule.TestActors
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

    public class DispatcherTest : TestKitXunit
    {
        public DispatcherTest(ITestOutputHelper output) : base(output)
        {
            Setup();
        }

        public void Setup()
        {
        }

        [Theory(DisplayName = "Dispacher로 성능이 다른 액터에게 메시지전송")]
        [InlineData(20)]
        public void Test1(int waitTimeSec)
        {
            return; //20초정도 소요되는 유닛테스트로, 배포시 자동유닛 테스트함으로 작동봉인 조치..

            var delayActor_fast1 = Sys.ActorOf(Props.Create(() => new DelayActor("delayActor_fast1"))
                .WithDispatcher("custom-fork-join-dispatcher2")
            );

            var delayActor_fast2 = Sys.ActorOf(Props.Create(() => new DelayActor("delayActor_fast2"))
                .WithDispatcher("custom-fork-join-dispatcher2")
            );

            var delayActor_slow1 = Sys.ActorOf(Props.Create(() => new DelayActor("delayActor_slow1"))
                .WithDispatcher("custom-fork-join-dispatcher1")
            );

            var delayActor_slow2 = Sys.ActorOf(Props.Create(() => new DelayActor("delayActor_slow2"))
                .WithDispatcher("custom-fork-join-dispatcher1")
            );

            for (int i = 0; i < 10; i++)
            {
                delayActor_fast1.Tell("fast"+i);
                delayActor_fast2.Tell("fast" + i);
                delayActor_slow1.Tell("slow" + i);
                delayActor_slow2.Tell("slow" + i);
            }
            Task.Delay(waitTimeSec*1000).Wait();
        }
    }
}
