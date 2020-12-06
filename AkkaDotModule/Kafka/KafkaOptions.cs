using Akka.Actor;
using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaDotModule.Kafka
{
    public class KafkaSecurityOption
    {
        public SecurityProtocol SecurityProtocol { get; set; }

        public SaslMechanism SaslMechanism { get; set; }

        public string SaslUsername { get; set; }

        public string SaslPassword { get; set; }

        public string SslCaLocation { get; set; }
    }

    public class ConsumerAkkaOption
    {
        public string BootstrapServers { get; set; }

        public string KafkaGroupId { get; set; }

        public IActorRef RelayActor { get; set; }

        public string Topics { get; set; }

        public AutoOffsetReset AutoOffsetReset { get;set;}

        public KafkaSecurityOption SecurityOption { get; set; }
    }

    public class ProducerAkkaOption
    {
        public string BootstrapServers { get; set; }

        public string ProducerName { get; set; }

        public KafkaSecurityOption SecurityOption { get; set; }
    }


}
