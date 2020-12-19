using Akka.Actor;
using Akka.Routing;
using Akka.TestKit;
using AkkaDotModule.ActorSample;
using AkkaNetCoreTest;
using System;
using Xunit;
using Xunit.Abstractions;

// http://wiki.webnori.com/display/AKKA/Routers
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
        [Theory(DisplayName = "fire를 n번 전송하면, n개의 액터가 각각 균등처리 ")]
        [InlineData(10,3)]
        public void TestRoundRobbin(int poolLength,int expectedTestSec) 
        {
            var helloActor = Sys.ActorOf(Props.Create(() => 
                new HelloActor("Pool5")).WithRouter(new RoundRobinPool(poolLength)));

            Within(TimeSpan.FromSeconds(expectedTestSec), () =>
            {                
                for(int i=0; i< poolLength; i++)
                {
                    helloActor.Tell("fire:"+(i+1), this.TestActor);
                }
                
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

        /// <summary>
        /// 사용목적 : 액터를 라운드 로빈으로 구성하고 분산처리할때
        /// </summary>        
        [Theory(DisplayName = "hello를 n번 전송하면, n개의 액터가 각각 균등처리하면서, world 응답체크.. ")]
        [InlineData(10, 3)]
        public void TestRoundRobbinCheckMsg(int poolLength, int expectedTestSec)
        {
            var helloActor = Sys.ActorOf(Props.Create(() =>
                new HelloActor("Pool5")).WithRouter(new RoundRobinPool(poolLength)));

            //Given
            string expectedStr = "world";

            Within(TimeSpan.FromSeconds(expectedTestSec), () =>
            {
                //When
                for (int i = 0; i < poolLength; i++)
                {                    
                    helloActor.Tell("hello:" + (i + 1), probe);
                    //Then
                    probe.ExpectMsg( expectedStr);
                }
            });            
        }
    }
}
