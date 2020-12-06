using Akka.Actor;
using Akka.Event;
using AkkaDotModule.Kafka;
using AkkaDotModule.Models;
using Confluent.Kafka;
using System;

namespace AkkaDotModule.ActorUtils.Confluent
{
    public class ProducerActor : ReceiveActor
    {
        private readonly ProducerConfig producerConfig;

        private readonly IProducer<long, string> producer;

        private readonly ILoggingAdapter logger = Context.GetLogger();

        private ulong sentMessageCount { get; set; }

        public ProducerActor(ProducerAkkaOption producerAkkaOption)
        {
            //Actor Init
            sentMessageCount = 0;

            if (producerAkkaOption.SecuritOption != null)
            {
                producerConfig = new ProducerConfig()
                {
                    BootstrapServers = producerAkkaOption.BootstrapServers,
                    SecurityProtocol = producerAkkaOption.SecuritOption.SecurityProtocol,
                    SaslMechanism = producerAkkaOption.SecuritOption.SaslMechanism,
                    SaslUsername = producerAkkaOption.SecuritOption.SaslUsername,
                    SaslPassword = producerAkkaOption.SecuritOption.SaslPassword,
                    SslCaLocation = producerAkkaOption.SecuritOption.SslCaLocation,
                    //Debug = "security,broker,protocol"        //Uncomment for librdkafka debugging information
                };

            }
            else
            {
                producerConfig = new ProducerConfig()
                {
                    BootstrapServers = producerAkkaOption.BootstrapServers                    
                    //Debug = "security,broker,protocol"        //Uncomment for librdkafka debugging information
                };
            }

            producer = new ProducerBuilder<long, string>(producerConfig).SetKeySerializer(Serializers.Int64).SetValueSerializer(Serializers.Utf8).Build();

            //Actor Message : 아래코드는 생성시, 메시지에따른 이벤트 처리기를 셋팅합니다.

            // 속도조절기로부터 OutPut된 이벤트가 이곳으로 인입 
            // 메시지에 따른 분기조건이 많아질시, if,switch 조합보다 패턴매칭 활용 추천 : https://docs.microsoft.com/ko-kr/dotnet/csharp/tutorials/pattern-matching
            ReceiveAsync<BatchData>(async msg =>
            {
                if(msg.Data is KafkaTextMessage)
                {
                    sentMessageCount++;
                    var kafkaMsg = msg.Data as KafkaTextMessage;
                    var deliveryReport = await producer.ProduceAsync(kafkaMsg.Topic, new Message<long, string> { Key = DateTime.UtcNow.Ticks, Value = kafkaMsg.Message });
                    logger.Info(string.Format("Message {0} sent (value: '{1}')", sentMessageCount, kafkaMsg.Message));
                }
            });

        }
    }
}
