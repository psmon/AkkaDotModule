using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using AkkaDotModule.Models;

namespace AkkaDotBootApi.Actor
{
    public class WorkActor : ReceiveActor
    {
        private readonly ILoggingAdapter logger = Context.GetLogger();

        public WorkActor()
        {
            ReceiveAsync<BatchData>(async batchData =>
            {
                string strResult = $"{batchData.Data as string}";
                Console.WriteLine(strResult);
                //TODO : Somthing
            });
        }
    }
}
