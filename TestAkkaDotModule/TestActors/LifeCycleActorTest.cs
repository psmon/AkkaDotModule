using Akka.Actor;
using Akka.TestKit;
using AkkaDotModule.ActorSample;
using AkkaNetCoreTest;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace TestAkkaDotModule.TestActors
{
    public class HelloActor2 : ReceiveActor
    {        
        private string MyName { get; set; }

        private IActorRef target = null;

        public HelloActor2(string name)
        {
            MyName = name;

            ReceiveAsync<string>(async message =>
            {
                string inComeMessage = $"[{MyName}] : {message}";

            });

        }
    }

    public class TestManager : UntypedActor
    {
        IActorRef probe;

        private IActorRef worker = Context.Watch(Context.ActorOf(
            Props.Create(() => new HelloActor2("hello")), "worker" ));

        public TestManager(IActorRef _probe)
        {
            probe = _probe;
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case "hello":
                    worker.Tell("hello");
                    break;
                case "shutdown":                    
                    worker.Tell(PoisonPill.Instance, Self);
                    Context.Become(ShuttingDown);   //종료모드로 메시지처리 상태변경
                    break;                
            }
        }

        private void ShuttingDown(object message)
        {
            switch (message)
            {
                case "hello": //어떤 잡을 시키려고 할시,셧다운중임을 알립니다.
                    Sender.Tell("service unavailable, shutting down", Self);
                    break;
                case Terminated t:
                    probe.Tell("SafeClose");
                    Context.Stop(Self);
                    break;
            }
        }
    }

    public class LifeCycleActorTest : TestKitXunit
    {
        protected TestProbe probe;

        protected IActorRef manager;

        public LifeCycleActorTest(ITestOutputHelper output) : base(output)
        {
            Setup();
        }

        public void Setup()
        {
            //테스트 관찰자..
            probe = this.CreateTestProbe();

            //GraceFulDown을 위한 Manager액터 생성
            manager = Sys.ActorOf(Props.Create(() => new TestManager(probe)));
            
        }

        [Theory(DisplayName = "GracefulStopTest")]
        [InlineData(5)]
        public async Task GtaceFulStopAreOK(int waitTimeSec)
        {
            //Step:
            // 1.GracefulStop 을통한 종료 시그널 발생 
            // 2.자식 액터종료(PoisonPill, 지금까지 받은메시지까지만 처리하고)
            // 3.GracefulStop , Terminated 될때까지 대기
            // 검증 : 안전한 종료메시지가 왔는지 검사

            await manager.GracefulStop(TimeSpan.FromMilliseconds(3), "shutdown");

            probe.ExpectMsg("SafeClose", TimeSpan.FromSeconds(waitTimeSec));
        }
    }
}
