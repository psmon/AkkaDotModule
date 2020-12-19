using Akka.Actor;
using AkkaDotBootApi.Actor;
using AkkaDotModule.ActorSample;
using AkkaDotModule.ActorUtils;
using AkkaDotModule.ActorUtils.Confluent;
using AkkaDotModule.Config;
using AkkaDotModule.Kafka;
using AkkaDotModule.Models;
using Confluent.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace AkkaDotBootApi.Test
{
    public class TestAkka
    {
        static public void Start(IApplicationBuilder app, ActorSystem actorSystem)
        {
            // http://wiki.webnori.com/display/AKKA/Actors

            // HelloActor 기본액터
            AkkaLoad.RegisterActor("helloActor" /*AkkaLoad가 인식하는 유니크명*/,
                actorSystem.ActorOf(Props.Create(() => new HelloActor("webnori")),
                    "helloActor" /*AKKA가 인식하는 Path명*/));

            var helloActor = actorSystem.ActorSelection("user/helloActor");
            var helloActor2 = AkkaLoad.ActorSelect("helloActor");

            helloActor.Tell("hello");
            helloActor2.Tell("hello");

            // 밸브 Work : 초당 작업량을 조절                
            int timeSec = 1;
            int elemntPerSec = 5;
            var throttleWork = AkkaLoad.RegisterActor("throttleWork",
                actorSystem.ActorOf(Props.Create(() => new ThrottleWork(elemntPerSec, timeSec)),
                "throttleWork"));

            // 실제 Work : 밸브에 방출되는 Task를 개별로 처리
            var worker = AkkaLoad.RegisterActor("worker", actorSystem.ActorOf(Props.Create<WorkActor>(),
                "worker"));

            // 배브의 작업자를 지정
            throttleWork.Tell(new SetTarget(worker));


            // 기호에따라 사용방식이 약간 다른 KAFKA를 선택할수 있습니다.

            //##################################################################
            //##### Confluent.Kafka를 Akka액터 모드로 연결한 모드로
            //##### 보안연결이 지원하기때문에 Saas형태의 Kafka에 보안연결이 가능합니다.
            //##### 커스텀한 액터를 생성하여,AkkaStream을 이해하고 직접 연결할수 있을때 유용합니다.
            //##################################################################
            
            //ProducerActor
            var producerAkkaOption = new ProducerAkkaOption()
            {
                BootstrapServers = "webnori-kafka.servicebus.windows.net:9093",
                ProducerName = "webnori-kafka",
                SecurityOption = new KafkaSecurityOption()
                {
                    SecurityProtocol = SecurityProtocol.SaslSsl,
                    SaslMechanism = SaslMechanism.Plain,
                    SaslUsername = "$ConnectionString",
                    SaslPassword = "Endpoint=sb://webnori-kafka.servicebus.windows.net/;SharedAccessKeyName=kafka-client;SharedAccessKey=PfL0qRUm50AXZHRXLiVfnatIRI3OqAh+dT6Owsqrd2M=",
                    SslCaLocation = "./cacert.pem"
                }
            };

            string producerActorName = "producerActor";
            var producerActor= AkkaLoad.RegisterActor(producerActorName /*AkkaLoad가 인식하는 유니크명*/,
                actorSystem.ActorOf(Props.Create(() => 
                    new ProducerActor(producerAkkaOption)),
                    producerActorName /*AKKA가 인식하는 Path명*/
            ));

            producerActor.Tell(new BatchData()
            {
                Data = new KafkaTextMessage()
                {
                    Topic = "akka100",
                    Message = "testData"
                }
            });

            //ConsumerActor
            var consumerAkkaOption = new ConsumerAkkaOption()
            {
                BootstrapServers = "webnori-kafka.servicebus.windows.net:9093",
                Topics = "akka100",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                KafkaGroupId = "akakTestGroup",
                RelayActor = null,  //작업자 액터를 연결하면, 소비메시지가 작업자에게 전달된다 ( 컨슘기능과 작업자 기능의 분리)
                SecurityOption = new KafkaSecurityOption()
                {
                    SecurityProtocol = SecurityProtocol.SaslSsl,
                    SaslMechanism = SaslMechanism.Plain,
                    SaslUsername = "$ConnectionString",
                    SaslPassword = "Endpoint=sb://webnori-kafka.servicebus.windows.net/;SharedAccessKeyName=kafka-client;SharedAccessKey=PfL0qRUm50AXZHRXLiVfnatIRI3OqAh+dT6Owsqrd2M=",
                    SslCaLocation = "./cacert.pem"
                }
            };

            string consumerActorName = "consumerActor";
            var consumerActor = AkkaLoad.RegisterActor(consumerActorName /*AkkaLoad가 인식하는 유니크명*/,
                actorSystem.ActorOf(Props.Create(() =>
                    new ConsumerActor(consumerAkkaOption)),
                    consumerActorName /*AKKA가 인식하는 Path명*/
            ));

            //컨슈머를 작동시킨다.
            consumerActor.Tell(new ConsumerStart());

            //##################################################################
            //##### Akka.Streams.Kafka(의존:Confluent.Kafka) 을 사용하는 모드로, Security(SSL)이 아직 지원되지 않습니다.
            //##### Private으로 구성된, Kafka Pass 모드일때 사용가능합니다.
            //##### AkkaStream.Kafka가 제공하는 스트림을 활용핼때 장점이 있습니다.
            //##################################################################

            // KAFKA - 
            // 각 System은 싱글톤이기때문에 DI를 통해 Controller에서 참조획득가능
            var consumerSystem = app.ApplicationServices.GetService<ConsumerSystem>();
            var producerSystem = app.ApplicationServices.GetService<ProducerSystem>();

            //소비자 : 복수개의 소비자 생성가능
            consumerSystem.Start(new ConsumerAkkaOption()
            {
                KafkaGroupId = "testGroup",
                BootstrapServers = "kafka:9092",
                RelayActor = null,          //소비되는 메시지가 지정 액터로 전달되기때문에,처리기는 액터로 구현
                Topics = "akka100",
            });

            //생산자 : 복수개의 생산자 생성가능
            producerSystem.Start(new ProducerAkkaOption()
            {
                BootstrapServers  = "kafka:9092",
                ProducerName = "producer1",                
            });

            List<string> messages = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                messages.Add($"message-{i}");
            }

            //보너스 : 생산의 속도를 조절할수 있습니다.
            int tps = 10;
            producerSystem.SinkMessage("producer1", "akka100", messages, tps);
        }
    }
}
