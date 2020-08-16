using System;
using Akka.Actor;
using Akka.Event;
using AkkaDotModule.Models;

namespace AkkaDotBootApi.Actor
{
    /// <summary>
    /// WorkActor의 구현은, 각 어플리케이션에서 직접 구현 
    /// </summary>
    public class WorkActor : ReceiveActor
    {
        private readonly ILoggingAdapter logger = Context.GetLogger();

        public WorkActor()
        {
            ReceiveAsync<BatchData>(async batchData =>
            {
                string strResult = $"{batchData.Data as string}";                
                logger.Info(strResult);
                //TODO : Somthing
            });
        }
    }
}
