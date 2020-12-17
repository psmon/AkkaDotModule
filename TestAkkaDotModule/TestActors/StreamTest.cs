using Akka;
using Akka.Actor;
using Akka.Routing;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.TestKit;
using AkkaDotModule.ActorSample;
using AkkaNetCoreTest;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

// http://wiki.webnori.com/display/AKKA/Basics+and+working+with+Flows
namespace TestAkkaDotModule.TestActors
{
    public class StreamTest : TestKitXunit
    {
        protected TestProbe probe;

        public StreamTest(ITestOutputHelper output) : base(output)
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
        /// <param name="elemntPerSec"></param>
        [Theory(DisplayName = "Graph연산규칙에따라 Stream연산이된다")]
        [InlineData(3)]
        public void TestFireAndForget(int expectedTestSec)
        {
            var materializer = Sys.Materializer();

            var g = RunnableGraph.FromGraph(GraphDsl.Create(builder =>
            {
                var source = Source.From(Enumerable.Range(1, 10));
                var sink = Sink.Ignore<int>().MapMaterializedValue(_ => NotUsed.Instance);
                var sinkConsole = Sink.ForEach<int>(x => Console.WriteLine(x.ToString()))
                    .MapMaterializedValue(_ => NotUsed.Instance);

                var broadcast = builder.Add(new Broadcast<int>(2));
                var merge = builder.Add(new Merge<int>(2));

                var f1 = Flow.Create<int>().Select(x => x + 10);
                var f2 = Flow.Create<int>().Select(x => x + 20);                
                var f3 = Flow.Create<int>().Select(x => x + 1);
                var f4 = Flow.Create<int>().Select(x => x + 10);

                builder.From(source).Via(f1).Via(broadcast).Via(f2).Via(merge).Via(f3).To(sinkConsole);
                builder.From(broadcast).Via(f4).To(merge);

                return ClosedShape.Instance;
            }));

            g.Run(materializer);


            Within(TimeSpan.FromSeconds(expectedTestSec), () =>
            {
                // 응답메시지가 없음을 검사
                probe.ExpectNoMsg(TimeSpan.FromSeconds(1));
            });
        }
    }
}
