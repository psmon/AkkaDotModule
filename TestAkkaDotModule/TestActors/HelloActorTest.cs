using System;
using Akka.Actor;
using Akka.TestKit;
using AkkaDotModule.ActorSample;
using AkkaNetCoreTest;
using Xunit;
using Xunit.Abstractions;

namespace TestAkkaDotModule.TestActors
{
    public class HelloActorTest : TestKitXunit
    {
        protected TestProbe probe;

        public HelloActorTest(ITestOutputHelper output) : base(output)
        {
            Setup();
        }

        public void Setup()
        {
            //스트림을 제공받는 최종 소비자 ( 물을 제공 받는 고객 )
            probe = this.CreateTestProbe();
        }

        /// <summary>
        /// 사용목적 : 특정 메시지를 송신하고, 완료처리를 비동기로 받을때 사용        
        /// </summary>
        /// <param name="timeSec"></param>
        /// <param name="elemntPerSec"></param>
        [Theory(DisplayName = "hello를 전송하면 world를 비동기적으로 수신")]
        [InlineData(3)]
        public void TestFireAndForget(int expectedTestSec) 
        {
            var helloActor = Sys.ActorOf(Props.Create(() => new HelloActor("Minsu")));

            // 응답 받을 액터를 지정한다. (첫번째:지정자 , 두번째:전송자)
            helloActor.Tell(probe.Ref, this.TestActor);

            // 두번째 인자의 Sender(전송장)가 받는 결과값 검사
            ExpectMsg("done", TimeSpan.FromSeconds(1));

            Within(TimeSpan.FromSeconds(expectedTestSec), () =>
            {
                helloActor.Tell("hello", this.TestActor);

                // 지정자가 받는 메시지 검사
                probe.ExpectMsg("world", TimeSpan.FromSeconds(1));
                
                // 송신을 하지만 응답을 꼭 알필요없이 Next수행 - fire and foget(군사용어)
                helloActor.Tell("fire", this.TestActor);
                helloActor.Tell("fire", this.TestActor);
                helloActor.Tell("fire", this.TestActor);

                // 응답메시지가 없음을 검사
                probe.ExpectNoMsg(TimeSpan.FromSeconds(1));
            });
        }
    }
}
