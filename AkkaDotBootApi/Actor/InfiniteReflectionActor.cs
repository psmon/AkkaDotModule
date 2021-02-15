using Akka.Actor;
using Akka.Event;
using Akka.Monitoring;

namespace AkkaDotBootApi.Actor
{
    public class InfiniteMessage
    {
        public string Message { get; set; }

        public uint Count { get; set; }
    }

    public class DistributedMessage
    {
        public string Message { get; set; }
    }

    public class InfiniteReflectionActor : ReceiveActor
    {
        private IActorRef ReplyActor;

        private readonly ILoggingAdapter logger = Context.GetLogger();

        private ulong recevedCnt;

        public InfiniteReflectionActor()
        {
            recevedCnt = 0;

            ReceiveAsync<IActorRef>(async actorRef =>
            {
                ReplyActor = actorRef;
            });

            ReceiveAsync<InfiniteMessage>(async infiniteMessage =>
            {
                Context.IncrementCounter("akka.infinite.metric");
                var reply = new InfiniteMessage
                {
                    Message = infiniteMessage.Message,
                    Count = ++infiniteMessage.Count
                };
                
                if(reply.Count % 50000 == 0)
                {
                    logger.Info($"Count:{reply.Count}");
                }

                ReplyActor.Tell(reply);
            });

            ReceiveAsync<DistributedMessage>(async distributedMessage =>
            {
                Context.IncrementCounter("akka.distributed.metric");
            });

        }

    }
}
