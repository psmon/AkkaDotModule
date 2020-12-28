using Akka.Actor;

namespace AkkaBlazorApp.Actors
{
    public enum CmdCount
    {
        CUR_NUM = 0,
        ADD_NUM = 1
    }

    public class CountActor : ReceiveActor
    {
        protected int currentCount = 0;

        public CountActor()
        {
            ReceiveAsync<CmdCount>(async cmdCount =>
            {
                if(cmdCount == CmdCount.CUR_NUM)
                {
                    Sender.Tell(currentCount);
                }
                else if(cmdCount == CmdCount.ADD_NUM)
                {
                    currentCount += 1;
                    Sender.Tell(currentCount);
                }
            });
        }
    }
}
