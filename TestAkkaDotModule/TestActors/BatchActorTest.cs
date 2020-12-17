using Akka.Actor;
using Akka.TestKit;
using AkkaDotModule.ActorUtils;
using AkkaDotModule.Models;
using AkkaNetCoreTest;
using System;
using Xunit;
using Xunit.Abstractions;

// http://wiki.webnori.com/display/AKKA/Finite+State+Machines
namespace TestAkkaDotModule.TestActors
{
    public class BatchActorTest : TestKitXunit
    {
        protected TestProbe probe;

        public BatchActorTest(ITestOutputHelper output) : base(output)
        {
            Setup();
        }

        public void Setup()
        {
            //배치가 컬렉션단위로 잘 수행하는지 관찰자 셋팅
            probe = this.CreateTestProbe();
        }

        // 테스트목적 : 이벤트가 발생할때마다 DB저장이 아닌, 특정시간 수집된 구간의 데이터 벌크인서트처리목적(벌크인서트는 건바이건보다 빠르다)
        // 벌크를 만드는 주기를 3초(collectSec)로 지정..
        [Theory(DisplayName = "배치처리는_특정시간구간_일괄처리한다")]
        [InlineData(3)]
        public void Test1(int collectSec)
        {
            var batchActor = Sys.ActorOf(Props.Create(() => new BatchActor(collectSec)));

            //배치저리 담당자 지정 : 배치처리를 검사하는 관찰자를 등록함
            IActorRef batchWriterActor = Sys.ActorOf(Props.Create(() => new TestBatchWriterActor(probe)));
            batchActor.Tell(new SetTarget(batchWriterActor));

            //이벤트는 실시간적으로 발생한다.
            batchActor.Tell(new Queue("오브젝트1"));
            batchActor.Tell(new Queue("오브젝트2"));
            batchActor.Tell(new Queue("오브젝트3"));

            //배치 처리할것이 없는것 확인
            probe.ExpectNoMsg();

            //배치 항목을 검사 : collectSec+1초를 기다려줌            
            var batchList = probe.ExpectMsg<Batch>(TimeSpan.FromSeconds(collectSec + 1)).Obj;

            var firstItem = batchList[0] as string;
            Assert.Equal("오브젝트1", firstItem);
            Assert.Equal(3, batchList.Count);

            //이벤트는 실시간적으로 발생한다.
            batchActor.Tell(new Queue("오브젝트4"));
            batchActor.Tell(new Queue("오브젝트5"));
            batchActor.Tell(new Queue("오브젝트6"));
            batchActor.Tell(new Queue("오브젝트7"));

            //강제 벌크요청
            batchActor.Tell(new Flush());

            //배치 항목을 검사
            batchList = probe.ExpectMsg<Batch>().Obj;
            firstItem = batchList[0] as string;
            Assert.Equal("오브젝트4", firstItem);
            Assert.Equal(4, batchList.Count);
        }
    }

    // 특정 기간동안 집계된 컬렉션은 이 액터에게 온다.
    public class TestBatchWriterActor : ReceiveActor
    {
        protected IActorRef probe;

        public TestBatchWriterActor(IActorRef _probe)
        {
            probe = _probe;
            ReceiveAsync<object>(async message =>
            {
                //각각 저장하고 싶은 DB에따라 배치처리를 수행한다.
                if (message is Batch batchMessage)
                {
                    probe.Tell(batchMessage);
                    Console.WriteLine($"====== TODO 배치수행 :{batchMessage.Obj.Count}");
                }
            });
        }
    }
}
