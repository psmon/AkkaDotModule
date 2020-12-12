using Akka.Actor;
using Akka.Event;
using AkkaDotModule.Kafka;
using AkkaDotModule.Models;
using Confluent.Kafka;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AkkaDotModule.ActorUtils.Confluent
{
    public class ConsumerStart
    {
    };

    public class ConsumerPull
    {
    };

    public class ConsumerStop
    {
    };

    public class ConsumerActor : ReceiveActor
    {
        private readonly string topic;
        
        private readonly ConsumerConfig consumerConfig;
        
        private readonly CancellationTokenSource cancellationTokenSource;

        private readonly IConsumer<Ignore, string> consumer;

        private readonly IActorRef workActor;

        private readonly ILoggingAdapter logger = Context.GetLogger();

        public ConsumerActor(ConsumerAkkaOption consumerAkkaOption)
        {
            topic = consumerAkkaOption.Topics;
            cancellationTokenSource = new CancellationTokenSource();
            workActor = consumerAkkaOption.RelayActor;            

            if (consumerAkkaOption.SecurityOption != null)
            {
                consumerConfig = new ConsumerConfig()
                {
                    GroupId = consumerAkkaOption.KafkaGroupId,
                    BootstrapServers = consumerAkkaOption.BootstrapServers,
                    AutoOffsetReset = consumerAkkaOption.AutoOffsetReset,
                    //For Security
                    SecurityProtocol = consumerAkkaOption.SecurityOption.SecurityProtocol,
                    SaslMechanism = consumerAkkaOption.SecurityOption.SaslMechanism,
                    SaslUsername = consumerAkkaOption.SecurityOption.SaslUsername,
                    SaslPassword = consumerAkkaOption.SecurityOption.SaslPassword,
                    SslCaLocation = consumerAkkaOption.SecurityOption.SslCaLocation,
                };
            }
            else
            {
                consumerConfig = new ConsumerConfig()
                {
                    GroupId = consumerAkkaOption.KafkaGroupId,
                    BootstrapServers = consumerAkkaOption.BootstrapServers,
                    AutoOffsetReset = consumerAkkaOption.AutoOffsetReset,
                };
            }

            consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();

            ReceiveAsync<ConsumerStart>(async msg =>
            {
                IActorRef selfActor = this.Self;
                consumer.Subscribe(topic);

                var cr = consumer.Consume(cancellationTokenSource.Token);
                selfActor.Tell(new KafkaTextMessage()
                {
                    Topic = cr.Topic,
                    Message = cr.Message.Value
                });
            });

            ReceiveAsync<ConsumerPull>(async msg =>
            {
                await Task.Run(async () =>
                {
                    var cr = consumer.Consume(cancellationTokenSource.Token);
                    var kafkamsg = new KafkaTextMessage()
                    {
                        Topic = cr.Topic,
                        Message = cr.Message.Value
                    };
                    return kafkamsg;
                }).PipeTo(Self);
            });

            ReceiveAsync<KafkaTextMessage>(async msg => 
            {
                IActorRef selfActor = this.Self;
                //이 액터는 카프카 메시지소비만 담당하며
                //소비된 메시지는 작업 액터에게 전달한다.
                string logText = $"Consumed message '{msg.Message}' Topic: '{msg.Topic}'.";
                logger.Debug(logText);

                if (workActor != null)
                {
                    workActor.Tell(msg);
                }
                selfActor.Tell(new ConsumerPull());
            });
        }

        protected override void PostStop()
        {
            Console.WriteLine("try down KafkaConsumer.....");
            cancellationTokenSource.Cancel();
        }

    }
}
