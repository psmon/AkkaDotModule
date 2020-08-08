using System;
using Akka;
using Akka.Actor;
using Akka.Event;
using Akka.Streams;
using Akka.Streams.Dsl;
using AkkaDotModule.Models;

namespace AkkaDotModule.ActorUtils
{    
    public class ThrottleWork : ReceiveActor
    {
        private readonly ILoggingAdapter logger = Context.GetLogger();

        private IActorRef consumer;

        public ThrottleWork(int element, int maxBust)
        {

            ReceiveAsync<SetTarget>(async target =>
            {
                consumer = target.Ref;
            });

            ReceiveAsync<object>(async message =>
            {
                if (message is BatchList batchMessage)
                {
                    int Count = batchMessage.Obj.Count;
                    Source<object, NotUsed> source = Source.From(batchMessage.Obj);

                    using (var materializer = Context.Materializer())
                    {
                        var factorials = source;
                        factorials
                             .Throttle(element, TimeSpan.FromSeconds(1), maxBust, ThrottleMode.Shaping)
                             .RunForeach(obj => {
                                 var nowstr = DateTime.Now.ToString("mm:ss");
                                 if (obj is BatchData batchData)
                                 {
                                     if (consumer != null) consumer.Tell(batchData);
                                 }
                             }, materializer)
                             .Wait();
                    }
                }
            });
        }
    }
}
