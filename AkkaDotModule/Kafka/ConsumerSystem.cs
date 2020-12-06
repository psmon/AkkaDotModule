using System;
using System.IO;
using Akka.Actor;
using Akka.Configuration;
using Akka.Streams;
using Akka.Streams.Kafka.Dsl;
using Akka.Streams.Kafka.Settings;
using Confluent.Kafka;

namespace AkkaDotModule.Kafka
{
    public class ConsumerSystem
    {        
        private ActorSystem consumerSystem;

        public ConsumerSystem()
        {
            string configText = File.ReadAllText("akka.kafka.conf");
            var config = ConfigurationFactory.ParseString(configText);
            consumerSystem = ActorSystem.Create("consumerSystem", config);
        }

        public void Start(ConsumerAkkaOption consumerActorOption)
        {
            IAutoSubscription makeshop_neworder = Subscriptions.Topics(consumerActorOption.Topics);

            var consumerSettings = ConsumerSettings<Null, string>.Create(consumerSystem, null, null)
                .WithBootstrapServers(consumerActorOption.BootstrapServers)
                .WithGroupId(consumerActorOption.KafkaGroupId);

            if(consumerActorOption.SucuritOption != null)
            {
                KafkaSecurityOption kafkaSecurityOption = consumerActorOption.SucuritOption;
            }


            var materializer_consumer = consumerSystem.Materializer();

            KafkaConsumer.CommittableSource(consumerSettings, makeshop_neworder)
            .RunForeach(result =>
            {                
                result.CommitableOffset.Commit();
                Console.WriteLine($"Consumer: {result.Record.Partition}/{result.Record.Topic} {result.Record.Offset}: {result.Record.Value}");
                if (consumerActorOption.RelayActor !=null)
                    consumerActorOption.RelayActor.Tell(result.Record.Value);  //ForgetAndFire 발송

            }, materializer_consumer);
        }
    }
}
