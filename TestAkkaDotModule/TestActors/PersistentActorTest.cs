using Akka.Actor;
using Akka.TestKit;
using AkkaNetCoreTest;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

// http://wiki.webnori.com/display/webfr/EventSourcing
namespace TestAkkaDotModule.ActorSample
{
    public class PersistentActorTest : TestKitXunit
    {
        protected TestProbe probe;

        protected IActorRef persistentActor;

        public PersistentActorTest(ITestOutputHelper output) : base(output)
        {
            Setup();
        }

        public void Setup()
        {
            //여기서 관찰자는 장바구니에 담긴 상품수를 검사할수 있습니다.
            probe = this.CreateTestProbe();

            persistentActor = Sys.ActorOf(Props.Create(() => new MyPersistentActor(probe)), "persistentActor");
        }

        //이벤트 소싱 테스트
        [Theory(DisplayName = "이벤트소싱-이벤트는 상태화되고 재생되고 복구되어야한다")]
        [InlineData(5)]
        public void Test1(int cutoffSec)
        {
            // usage
            int expectedCount = 2;

            //선택 장애 장바구니 이벤트
            Cmd cmd1 = new Cmd("장바구니를 물건을 담음+1");
            Cmd cmd2 = new Cmd("장바구니에 물건을 뺌-0");
            Cmd cmd3 = new Cmd("장바구니에 물건을 담음+1");
            Cmd cmd4 = new Cmd("장바구니에 물건을 담음+2");

            Within(TimeSpan.FromSeconds(cutoffSec), () =>
            {
                persistentActor.Tell(cmd1);
                persistentActor.Tell(cmd2);
                persistentActor.Tell(cmd3);
                persistentActor.Tell(cmd4);
                persistentActor.Tell("print"); //현재까지 액터가 가진 이벤트리스트를 재생합니다.
                Assert.Equal(expectedCount, probe.ExpectMsg<int>());

                //액터를 강제로 죽입니다.
                persistentActor.Tell(Kill.Instance);
                Task.Delay(500).Wait();

                //시스템 셧다운후,재시작 시나리오
                //액터를 다시생성하여, 액터가 가진 이벤트가 복구되는지 확인합니다.
                persistentActor = Sys.ActorOf(Props.Create(() => new MyPersistentActor(probe)), "persistentActor");
                persistentActor.Tell("print");
                Assert.Equal(expectedCount, probe.ExpectMsg<int>());

            });
        }
    }
}
