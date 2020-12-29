using Akka.Actor;
using System.Threading.Tasks;

namespace AkkaBlazorApp.Actors
{
    public enum CmdCount
    {
        CUR_NUM = 0,
        ADD_NUM = 1,
        ADD_NUM2 = 2,
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

                    if(currentCount % 10 == 0)
                    {
                        //10의 배수마다 지연증가를 작동합니다.
                        DelayIncrease();
                    }                    
                }
                else if (cmdCount == CmdCount.ADD_NUM2)
                {
                    // 지연증가가 안전하게 처리됩니다.
                    currentCount += 8;                    
                }
            });
        }

        protected void DelayIncrease()
        {
            Task.Run(async () =>
            {
                //긴작업으로 인해 지연이 발생하여도 액터는 멈추지 않습니다.
                await Task.Delay(1000);                
                CmdCount reply = CmdCount.ADD_NUM2;
                return reply;
            }).PipeTo(Self);
        }
    }
}
