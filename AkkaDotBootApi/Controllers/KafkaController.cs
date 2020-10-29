using AkkaDotModule.Kafka;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AkkaDotBootApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KafkaController : ControllerBase
    {
        private ProducerSystem producerSystem;

        public KafkaController(ProducerSystem _producerSystem)
        {
            producerSystem = _producerSystem;
        }

        /// <summary>
        /// Kafka 메시지 생성
        /// 개수와 tps조절가능
        /// 
        /// testTopic : akka100
        /// </summary>
        [HttpGet("HelloActor-Tell")]
        public int Kafka_ProducerMessage(int count, int tps)
        {
            List<string> messages = new List<string>();
            for (int i = 0; i < count; i++)
            {
                messages.Add($"message-{i}");
            }
            producerSystem.SinkMessage("producer1", "akka100", messages, tps);
            return 1;
        }
    }
}
