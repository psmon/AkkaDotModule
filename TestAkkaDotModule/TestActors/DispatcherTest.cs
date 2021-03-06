﻿using Akka.Actor;
using AkkaNetCoreTest;
using System.Threading.Tasks;
using TestAkkaDotModule.ActorSample;
using Xunit;
using Xunit.Abstractions;

// http://wiki.webnori.com/display/webfr/Dispatcher+Vs+Thread
namespace TestAkkaDotModule.TestActors
{
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
