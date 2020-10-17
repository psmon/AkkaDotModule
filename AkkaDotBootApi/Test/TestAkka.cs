using Akka.Actor;
using AkkaDotBootApi.Actor;
using AkkaDotModule.ActorSample;
using AkkaDotModule.ActorUtils;
using AkkaDotModule.Config;
using AkkaDotModule.Kafka;
using AkkaDotModule.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace AkkaDotBootApi.Test
{
    public class TestAkka
    {
        static public void Start(IApplicationBuilder app, ActorSystem actorSystem)
        {
            // HelloActor 기본액터
            AkkaLoad.RegisterActor("helloActor" /*AkkaLoad가 인식하는 유니크명*/,
                actorSystem.ActorOf(Props.Create(() => new HelloActor("webnori")), "helloActor" /*AKKA가 인식하는 Path명*/
            ));

            var helloActor = actorSystem.ActorSelection("user/helloActor");
            var helloActor2 = AkkaLoad.ActorSelect("helloActor");

            helloActor.Tell("hello");
            helloActor2.Tell("hello");


            // 밸브 Work : 초당 작업량을 조절                
            int timeSec = 1;
            int elemntPerSec = 5;
            var throttleWork = AkkaLoad.RegisterActor("throttleWork",
                actorSystem.ActorOf(Props.Create(() => new ThrottleWork(elemntPerSec, timeSec)), "throttleWork"));

            // 실제 Work : 밸브에 방출되는 Task를 개별로 처리
            var worker = AkkaLoad.RegisterActor("worker", actorSystem.ActorOf(Props.Create<WorkActor>(), "worker"));

            // 배브의 작업자를 지정
            throttleWork.Tell(new SetTarget(worker));

            // KAFKA 셋팅
            // 각 System은 싱글톤이기때문에 DI를 통해 Controller에서 참조획득가능
            var consumerSystem = app.ApplicationServices.GetService<ConsumerSystem>();
            var producerSystem = app.ApplicationServices.GetService<ProducerSystem>();

            //소비자 : 복수개의 소비자 생성가능
            consumerSystem.Start(new ConsumerAkkaOption()
            {
                KafkaGroupId = "testGroup",
                KafkaUrl = "kafka:9092",
                RelayActor = null,          //소비되는 메시지가 지정 액터로 전달되기때문에,처리기는 액터로 구현
                Topics = "akka100"
            });

            //생산자 : 복수개의 생산자 생성가능
            producerSystem.Start(new ProducerAkkaOption()
            {
                KafkaUrl = "kafka:9092",
                ProducerName = "producer1"
            });

            List<string> messages = new List<string>();
            for(int i=0; i < 10; i++)
            {
                messages.Add($"message-{i}");
            }

            //보너스 : 생산의 속도를 조절할수 있습니다.
            int tps = 10;
            producerSystem.SinkMessage("producer1", "akka100", messages, tps);

        }
    }
}
