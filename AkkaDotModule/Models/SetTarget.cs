using Akka.Actor;

namespace AkkaDotModule.Models
{
    public class SetTarget
    {
        public SetTarget(IActorRef @ref)
        {
            Ref = @ref;
        }

        public IActorRef Ref { get; }
    }
}
