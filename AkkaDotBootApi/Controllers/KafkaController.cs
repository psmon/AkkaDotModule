using Akka.Actor;
using AkkaDotModule.Config;
using AkkaDotModule.Kafka;
using AkkaDotModule.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AkkaDotBootApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KafkaController : ControllerBase
    {
        private ProducerSystem producerSystem;

        private IActorRef producerActor;

        public KafkaController(ProducerSystem _producerSystem)
        {
            producerSystem = _producerSystem;

            producerActor = AkkaLoad.ActorSelect("producerActor");
        }

        /// <summary>
        /// Kafka 메시지 생성 : System이용
        /// 개수와 tps조절가능
        /// 
        /// testTopic : akka100
        /// </summary>
        [HttpPost("HelloActor-Tell-System")]
        public int Kafka_ProducerMessageByActorSystem(int count, int tps)
        {
            List<string> messages = new List<string>();
            for (int i = 0; i < count; i++)
            {
                messages.Add($"message-{i}");
            }
            producerSystem.SinkMessage("producer1", "akka100", messages, tps);
            return 1;
        }

        /// <summary>
        /// Kafka 메시지 생성 : Actor이용(이모델을 사용추천)        
        /// 
        /// testTopic : akka100
        /// </summary>
        [HttpPost("HelloActor-Tell-Actor")]
        public int Kafka_ProducerMessageByActor(string topic, string message,int loop)
        {
            for(int i = 0; i < loop; i++)
            {
                producerActor.Tell(new BatchData()
                {
                    Data = new KafkaTextMessage()
                    {
                        Topic = "akka100",
                        Message = "testData-" + i
                    }
                });
            }
            return 1;
        }


    }
}
