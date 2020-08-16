using System;
using Akka.Actor;
using Akka.TestKit;
using AkkaDotModule.Models;
using AkkaNetCoreTest;
using Xunit;
using Xunit.Abstractions;

namespace TestAkkaDotModule.TestActors
{
    public class PriorityMessageMailboxTest : TestKitXunit
    {
        protected TestProbe probe;

        protected IActorRef mailBoxActor;

        public PriorityMessageMailboxTest(ITestOutputHelper output) : base(output)
        {
            Setup();
        }

        public void Setup()
        {
            //여기서 관찰자는 Qa 알림 역활을 받습니다.
            probe = this.CreateTestProbe();
            var mailboxOpt = Props.Create<MailBoxActor>(probe).WithMailbox("my-custom-mailbox");
            mailBoxActor = Sys.ActorOf(mailboxOpt, "mymailbox");
        }

        /// <summary>
        /// 사용목적 : 동시성으로 발생하는 메시지에 우선순위 부여할때
        /// </summary>
        /// <param name="timeSec"></param>
        /// <param name="elemntPerSec"></param>
        [Theory(DisplayName = "Priority 우선순위가높은 메시지가 먼저 처리가된다")]
        [InlineData(3)]
        public void TestFireAndForget(int expectedTestSec)
        {
            // priority 우선순위높음 = 낮은순자를 위미...
            PriorityMessage msg1 = new PriorityMessage("test1", 5);
            PriorityMessage msg2 = new PriorityMessage("test2", 4);
            PriorityMessage msg3 = new PriorityMessage("test3", 3);
            PriorityMessage msg4 = new PriorityMessage("test4", 2);
            PriorityMessage msg5 = new PriorityMessage("test5", 1);
            mailBoxActor.Tell(msg1);
            mailBoxActor.Tell(msg2);
            mailBoxActor.Tell(msg3);
            mailBoxActor.Tell(msg4);
            mailBoxActor.Tell(msg5);

            for (int i = 0; i < 5; i++)
            {
                probe.ExpectMsg<PriorityMessage>(issue =>
                {
                    Console.WriteLine($"IssueInfo : Message:{issue.Message as string} priority:{issue.Priority}");
                });
            }
            /* 기대결과
            IssueInfo : Message:test5 priority:1
            IssueInfo : Message:test4 priority:2
            IssueInfo : Message:test3 priority:3
            IssueInfo : Message:test2 priority:4
            IssueInfo : Message:test1 priority:5
            */
        }
    }

    public class MailBoxActor : ReceiveActor
    {
        IActorRef notifyQa;
        public MailBoxActor(IActorRef _notifyQa)
        {
            notifyQa = _notifyQa;
            //MailBoxTest
            Receive<PriorityMessage>(issue => {
                //Console.WriteLine($"IssueInfo : Message:{issue.Message} IsSecurityFlaw:{issue.IsSecurityFlaw} IsBug:{issue.IsBug} ");
                notifyQa.Tell(issue);
            });
        }
    }
}
