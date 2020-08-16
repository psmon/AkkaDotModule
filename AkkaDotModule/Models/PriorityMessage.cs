using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using Akka.Dispatch;
using DotNetty.Codecs;
using AkkaConfig = Akka.Configuration.Config;


//AkkaDotModule.Models.PriorityMessageMailbox
namespace AkkaDotModule.Models
{
    public class PriorityMessage
    {
        public object Message { get; set; }

        public int Priority { get; set; }

        public PriorityMessage(object message,int priority)
        {
            Message = message;
            Priority = priority;
        }
    }

    public class PriorityMessageMailbox : UnboundedPriorityMailbox
    {
        public PriorityMessageMailbox(Settings setting, AkkaConfig config) : base(setting, config)
        {
        }

        protected override int PriorityGenerator(object message)
        {
            var priorityMessage = message as PriorityMessage;

            //큰수의 우선순위가 먼저 처리가 된다.
            if (priorityMessage != null)
            {
                return priorityMessage.Priority;
            }
            return 2;
        }
    }
}
