using Akka.Streams.Dsl;
using Akka.Streams.SignalR.AspNetCore;
using Akka.Streams.SignalR.AspNetCore.Internals;
using AkkaDotModule.Config;
using Microsoft.AspNetCore.SignalR;

// Sample : https://github.com/akkadotnet/Alpakka/tree/dev/src/SignalR.AspNetCore
namespace AkkaDotBootApi.SignalR
{
    public class EchoHub : StreamHub<EchoStream>
    {
        public EchoHub(IStreamDispatcher dispatcher)
            : base(dispatcher)
        { }
    }

    // AKKA STREAM : https://getakka.net/articles/streams/cookbook.html
    public class EchoStream : StreamConnector
    {
        public EchoStream(IHubClients clients, ConnectionSourceSettings sourceSettings = null, ConnectionSinkSettings sinkSettings = null)
            : base(clients, sourceSettings, sinkSettings)
        {
            Source
                .Collect(x => x as Received)
                // Tell everyone
                .Select(x => Signals.Broadcast(x.Data))
                // Or tell everyone else, except one-self
                // .Select(x => Signals.Broadcast(x.Data, new[] { x.Request.ConnectionId }))
                // Or just send it back to one-self
                // .Select(x => Signals.Send(x.Request.ConnectionId, x.Data.Payload))
                .To(Sink)
                .Run(AkkaLoad.Materializer);
        }
    }
}
