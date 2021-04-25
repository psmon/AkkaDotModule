using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Akka;
using Akka.Actor;
using Akka.Configuration;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Streams.Kafka.Dsl;
using Akka.Streams.Kafka.Messages;
using Akka.Streams.Kafka.Settings;
using Confluent.Kafka;

namespace AkkaDotModule.Kafka
{
    public class ProducerSystem
    {
        private ActorSystem producerSystem;        
        
        private ActorMaterializer materializer_producer;

        private Dictionary<string, ProducerSettings<Null, string>> producerList = new Dictionary<string, ProducerSettings<Null, string>>();


        public ProducerSystem()
        {
            string configText = File.ReadAllText("akka.kafka.conf");
            var config = ConfigurationFactory.ParseString(configText);
            producerSystem = ActorSystem.Create("producerSystem", config);
        }

        public void Start(ProducerAkkaOption producerAkkaOption)
        {            
            materializer_producer = producerSystem.Materializer();

            var producer = ProducerSettings<Null, string>.Create(producerSystem, null, null)
                .WithBootstrapServers(producerAkkaOption.BootstrapServers);                

            producerList[producerAkkaOption.ProducerName] = producer;
        }

        public void SinkMessage(string producerName, string topic,List<string> message,int tps)
        {
            ProducerSettings<Null, string> producerSettings = producerList[producerName];

            Source<string, NotUsed> source = Source.From(message);
            source            
            .Select(c =>
            {
                return c;
            })
            .Throttle(tps, TimeSpan.FromSeconds(1), 1000, ThrottleMode.Shaping)      //TPS
            .Select(elem => ProducerMessage.Single(new ProducerRecord<Null, string>(topic, elem)))
            .Via(KafkaProducer.FlexiFlow<Null, string, NotUsed>(producerSettings))
            .Select(result =>
            {
                var response = result as Result<Null, string, NotUsed>;
                Console.WriteLine($"Producer: {response.Metadata.Topic}/{response.Metadata.Partition} {response.Metadata.Offset}: {response.Metadata.Value}");
                return result;
            })
            .RunWith(Sink.Ignore<IResults<Null, string, NotUsed>>(), materializer_producer);
        }

    }
}
