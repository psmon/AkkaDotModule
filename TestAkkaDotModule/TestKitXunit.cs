using System;
using System.IO;
using Akka.TestKit.Xunit2;
using Xunit.Abstractions;

namespace AkkaNetCoreTest
{
    /// <summary>
    /// 설명 : XUnit+TestKit 을 활용할수 있는 추상 객체로 상속받아 사용
    /// 목적 : Xunit 테스트툴에 Console.Write 출력가능 / Akka의 옵션적용 가능
    /// </summary>
    public abstract class TestKitXunit : TestKit
    {
        private readonly ITestOutputHelper _output;
        private readonly TextWriter _originalOut;
        private readonly TextWriter _textWriter;

        static public string akkaConfig = @"
akka.loglevel = DEBUG

my-custom-mailbox {
    mailbox-type : ""AkkaDotModule.Models.PriorityMessageMailbox, AkkaDotModule""
}

akka.persistence.max-concurrent-recoveries = 50 #복구 최고 개수
akka.actor.default-mailbox.stash-capacity = 10000
akka.persistence.internal-stash-overflow-strategy = ""akka.persistence.ThrowExceptionConfigurator""

actor.deployment {
    /mymailbox {
        mailbox = my-custom-mailbox
    }
}
";
        public TestKitXunit(ITestOutputHelper output) : base(akkaConfig)
        {
            _output = output;
            _originalOut = Console.Out;
            _textWriter = new StringWriter();
            Console.SetOut(_textWriter);         
        }

        protected override void Dispose(bool disposing)
        {            
            _output.WriteLine(_textWriter.ToString());
            Console.SetOut(_originalOut);
            base.Dispose(disposing);
        }
    }
}
