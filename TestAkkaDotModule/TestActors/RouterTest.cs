using Akka.Actor;
using Akka.Routing;
using Akka.TestKit;
using AkkaDotModule.ActorSample;
using AkkaNetCoreTest;
using System;
using Xunit;
using Xunit.Abstractions;

namespace TestAkkaDotModule.TestActors
{
    public class RouterTest : TestKitXunit
    {
        protected TestProbe probe;

        public RouterTest(ITestOutputHelper output) : base(output)
        {
            Setup();
        }

        public void Setup()
        {         
            probe = this.CreateTestProbe();
        }

        /// <summary>
        /// 사용목적 : 액터를 라운드 로빈으로 구성하고 분산처리할때
        /// </summary>
        /// <param name="timeSec"></param>
        /// <param name="elemntPerSec"></param>
        [Theory(DisplayName = "fire를 5번 전송하면, 5개의 액터가 각각 균등처리 ")]
        [InlineData(3)]
        public void TestRoundRobbin(int expectedTestSec) 
        {
            var helloActor = Sys.ActorOf(Props.Create(() => 
                new HelloActor("Pool5")).WithRouter(new RoundRobinPool(5)));

            Within(TimeSpan.FromSeconds(expectedTestSec), () =>
            {                
                helloActor.Tell("fire1", this.TestActor);
                helloActor.Tell("fire2", this.TestActor);
                helloActor.Tell("fire3", this.TestActor);
                helloActor.Tell("fire4", this.TestActor);
                helloActor.Tell("fire5", this.TestActor);
                
                // 응답메시지가 없음을 검사(대기)
                probe.ExpectNoMsg(TimeSpan.FromSeconds(1));
            });
            /* 수행결과 - 유닛테스트 결과에서 확인가능하며 멀티스레드를 이용하여 분산처리가됨 
            [INFO][2020-12-03 오후 3:30:29][Thread 0022][akka://test/user/$a/$b] [Pool5] : fire2
            [INFO][2020-12-03 오후 3:30:29][Thread 0024][akka://test/user/$a/$c] [Pool5] : fire3
            [INFO][2020-12-03 오후 3:30:29][Thread 0011][akka://test/user/$a/$d] [Pool5] : fire4
            [INFO][2020-12-03 오후 3:30:29][Thread 0010][akka://test/user/$a/$a] [Pool5] : fire1
            [INFO][2020-12-03 오후 3:30:29][Thread 0023][akka://test/user/$a/$e] [Pool5] : fire5
            */
        }
    }
}
