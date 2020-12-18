using Akka.Actor;
using Akka.Event;

namespace AkkaDotModule.ActorSample
{
    /// <summary>
    /// 기본 Actor 
    /// </summary>
    public class HelloActor : ReceiveActor
    {
        private readonly ILoggingAdapter logger = Context.GetLogger();

        private string MyName { get; set; }

        private IActorRef target = null;

        public HelloActor(string name, IActorRef _target)
        {
            MyName = name;
            target = _target;

            ReceiveAsync<string>(async message =>
            {
                string inComeMessage = $"[{MyName}] : {message}";
                logger.Info(inComeMessage);
                if(message == "fire")
                {                    
                }
                else if(message == "hello")
                {
                    Sender.Tell("world");

                    //지정된 액터에게 추가 전송
                    if (target != null)
                    {
                        target.Tell("world");
                    }                        
                }
            });

            // 응답받을 액터지정
            Receive<IActorRef>(actorRef => {
                target = actorRef;
                Sender.Tell("done");
            });

        }
    }
}
