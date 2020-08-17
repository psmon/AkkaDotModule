using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Akka.Actor;
using Akka.TestKit;
using AkkaDotModule.ActorUtils;
using AkkaDotModule.Models;
using AkkaNetCoreTest;
using Xunit;
using Xunit.Abstractions;

namespace TestAkkaDotModule.TestActors
{
    public class ThrottleWorkTest : TestKitXunit
    {
        protected TestProbe probe;

        public ThrottleWorkTest(ITestOutputHelper output) : base(output)
        {
            Setup();
        }

        public void Setup()
        {
            //스트림을 제공받는 최종 소비자 ( 물을 제공 받는 고객 )
            probe = this.CreateTestProbe();
        }


        /// <summary>
        /// 사용목적 : 대량의 데이터가 인입될때, 이용하여 초당 처리시간을 결정할수있다.
        /// </summary>
        /// <param name="timeSec"></param>
        /// <param name="elemntPerSec"></param>
        [Theory(DisplayName = "밸브작업자는 초당5개씩만 처리한다")]
        [InlineData(1, 5)]
        public void TestThrottleWork1(int timeSec, int elemntPerSec)
        {
            var throttleWork = Sys.ActorOf(Props.Create(() => new ThrottleWork(elemntPerSec, timeSec)));
            throttleWork.Tell(new SetTarget(probe));

            int totalBatchCount = 10;   //총 테스트 개수
            int expectedTestSec = (totalBatchCount / elemntPerSec) + 2; //완료최대예상시간

            List<object> batchDatas = new List<object>();
            //테스트 데이터 준비
            for (int i = 0; i < totalBatchCount; i++)
            {
                batchDatas.Add(new BatchData()
                {
                    Data = $"test-{i}"
                });
            }

            var batchList = new BatchList(batchDatas.ToImmutableList());
            
            Within(TimeSpan.FromSeconds(expectedTestSec), () =>
            {
                // 데이터를 한꺼번에 큐에 넣는다.
                throttleWork.Tell(batchList);

                BatchData lastMessage = null;
                for (int i=0;i<totalBatchCount;i++)
                {
                    var batchData = probe.ExpectMsg<BatchData>();

                    // 초당 5개씩 처리되는것 확인
                    string strResult = $"{DateTime.Now.ToString()} {batchData.Data as string}";
                    Console.WriteLine(strResult);

                    if(i== totalBatchCount - 1)
                    {
                        lastMessage = batchData;
                    }
                }
                //마지막 메시지 검사
                Assert.Equal($"test-{totalBatchCount-1}", lastMessage.Data as string);
            });
        }
    }
}
